using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using MyProject.Core;
using R3;
using UnityEngine;

namespace MyProject.Actor
{
    public class EffectActor : OtogeActorBase
    {
        const float ScrollSpeedMultiplierValue = 5.5f;
        const float EventMoveDuration = 1.6f;
        const float EventRotateZ = 360f;

        protected override OtogeType ActorOtogeType => OtogeType.Effect;
        protected override float ScrollSpeedMultiplier => ScrollSpeedMultiplierValue;

        [SerializeField] GameObject noteParent;
        [SerializeField] EffectTapActor tapPrefab;
        [SerializeField] EffectHoldActor holdPrefab;
        [SerializeField] EffectHoldTickActor holdTickPrefab;
        [SerializeField] EffectMeasureLineActor measureLinePrefab;
        [SerializeField] LaneLightActor laneLightActor;
        [SerializeField] JudgeEffectFactory judgeEffectFactory;
        [SerializeField] Vector3 eventCameraLocalPosition;
        [SerializeField] float eventCameraLocalEulerX;

        EffectActionsObserver effectActionsObserver;

        CancellationTokenSource eventCts;
        MotionHandle eventMoveHandle;
        MotionHandle eventApproachRotateXHandle;
        MotionHandle eventReturnHandle;
        MotionHandle eventReturnRotateXHandle;
        MotionHandle eventRotateHandle;
        Transform mainCameraTransform;
        Vector3 eventCameraBaseLocalPosition;
        Vector3 eventCameraBaseLocalEulerAngles;
        bool hasEventCameraBaseState;

        public override void InstallActions(OtogeActions otogeActions)
        {
            effectActionsObserver = new EffectActionsObserver(otogeActions);
            LanePressed = effectActionsObserver.LanePressed;
            LaneReleased = effectActionsObserver.LaneReleased;
            AirPressed = Observable.Empty<Unit>();
            AirReleased = Observable.Empty<Unit>();
        }

        public override void Initialize()
        {
            CancelEvent(restoreCamera: true);
            DestroyNotes();
            laneLightActor.Initialize();
            judgeEffectFactory.Initialize();

            effectActionsObserver.LanePressed.Subscribe(lane => laneLightActor.LightUp(lane)).AddTo(this);
            effectActionsObserver.LaneReleased.Subscribe(lane => laneLightActor.LightDown(lane)).AddTo(this);

            gameObject.SetActive(false);
            effectActionsObserver.Disable();
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);

            var showTasks = new List<UniTask>
            {
                SwitchActionsAfterDelayAsync(effectActionsObserver.Enable, ct),
                laneLightActor.ShowAsync(ct)
            };
            foreach (var noteActor in NoteActors)
            {
                showTasks.Add(noteActor.ShowAsync(ct));
            }
            await UniTask.WhenAll(showTasks);
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            CancelEvent(restoreCamera: true);

            var hideTasks = new List<UniTask>
            {
                SwitchActionsAfterDelayAsync(effectActionsObserver.Disable, ct),
                laneLightActor.HideAsync(ct)
            };
            foreach (var noteActor in NoteActors)
            {
                hideTasks.Add(noteActor.HideAsync(ct));
            }
            await UniTask.WhenAll(hideTasks);

            gameObject.SetActive(false);
        }

        public override void CreateNotes(IReadOnlyList<NoteCoreBase> noteCores)
        {
            DestroyNotes();
            foreach (var noteCore in noteCores)
            {
                if (!IsOwnedNote(noteCore))
                {
                    continue;
                }

                var noteType = noteCore.Property.NoteType;
                NoteActorBase noteActor = noteType switch
                {
                    NoteType.Tap => Instantiate(tapPrefab, noteParent.transform),
                    NoteType.Hold => Instantiate(holdPrefab, noteParent.transform),
                    NoteType.HoldTick => Instantiate(holdTickPrefab, noteParent.transform),
                    NoteType.MeasureLine => Instantiate(measureLinePrefab, noteParent.transform),
                    _ => null
                };

                if (noteActor == null)
                {
                    continue;
                }

                noteActor.Initialize();
                noteActor.Install(noteCore, judgeEffectFactory);
                NoteActors.Add(noteActor);
            }
        }

        public override async UniTask ExecuteEvent()
        {
            CancelEvent(restoreCamera: true);

            eventCts = new CancellationTokenSource();
            var token = eventCts.Token;

            try
            {
                token.ThrowIfCancellationRequested();

                mainCameraTransform ??= Camera.main.transform;
                eventCameraBaseLocalPosition = mainCameraTransform.localPosition;
                eventCameraBaseLocalEulerAngles = mainCameraTransform.localEulerAngles;
                hasEventCameraBaseState = true;

                eventMoveHandle = LMotion.Create(eventCameraBaseLocalPosition, eventCameraLocalPosition, EventMoveDuration)
                    .WithEase(Ease.Linear)
                    .Bind(value => mainCameraTransform.localPosition = value)
                    .AddTo(this);

                var startApproachEuler = mainCameraTransform.localEulerAngles;
                var startApproachRotation = mainCameraTransform.localRotation;
                var targetApproachRotation = Quaternion.Euler(eventCameraLocalEulerX, startApproachEuler.y, startApproachEuler.z);
                eventApproachRotateXHandle = LMotion.Create(startApproachRotation, targetApproachRotation, EventMoveDuration)
                    .WithEase(Ease.Linear)
                    .Bind(value => mainCameraTransform.localRotation = value)
                    .AddTo(this);

                await UniTask.WhenAll
                (
                    eventMoveHandle.ToUniTask(CancelBehavior.Cancel, false, token),
                    eventApproachRotateXHandle.ToUniTask(CancelBehavior.Cancel, false, token)
                );

                var startZ = mainCameraTransform.localEulerAngles.z;
                var startReturnEuler = mainCameraTransform.localEulerAngles;
                var fromReturnXRotation = Quaternion.Euler(startReturnEuler.x, startReturnEuler.y, 0f);
                var toReturnXRotation = Quaternion.Euler(eventCameraBaseLocalEulerAngles.x, eventCameraBaseLocalEulerAngles.y, 0f);
                var currentReturnXRotation = fromReturnXRotation;
                var currentReturnZ = startZ;

                void ApplyReturnRotation()
                {
                    var zRotation = Quaternion.AngleAxis(currentReturnZ, Vector3.forward);
                    mainCameraTransform.localRotation = currentReturnXRotation * zRotation;
                }

                eventReturnHandle = LMotion.Create(mainCameraTransform.localPosition, eventCameraBaseLocalPosition, EventMoveDuration)
                    .WithEase(Ease.OutQuart)
                    .Bind(value => mainCameraTransform.localPosition = value)
                    .AddTo(this);

                eventReturnRotateXHandle = LMotion.Create(fromReturnXRotation, toReturnXRotation, EventMoveDuration)
                    .WithEase(Ease.OutQuart)
                    .Bind(value =>
                    {
                        currentReturnXRotation = value;
                        ApplyReturnRotation();
                    })
                    .AddTo(this);

                eventRotateHandle = LMotion.Create(0f, EventRotateZ, EventMoveDuration)
                    .WithEase(Ease.OutQuart)
                    .Bind(value =>
                    {
                        currentReturnZ = startZ + value;
                        ApplyReturnRotation();
                    })
                    .AddTo(this);

                await UniTask.WhenAll
                (
                    eventReturnHandle.ToUniTask(CancelBehavior.Cancel, false, token),
                    eventReturnRotateXHandle.ToUniTask(CancelBehavior.Cancel, false, token),
                    eventRotateHandle.ToUniTask(CancelBehavior.Cancel, false, token)
                );

                RestoreCamera();
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
            }
            finally
            {
                if (eventCts != null && eventCts.Token == token)
                {
                    eventCts.Dispose();
                    eventCts = null;
                }
            }
        }

        void OnDestroy()
        {
            CancelEvent(restoreCamera: false);
        }

        void CancelEvent(bool restoreCamera)
        {
            eventCts?.Cancel();
            eventCts?.Dispose();
            eventCts = null;

            eventMoveHandle.TryCancel();
            eventApproachRotateXHandle.TryCancel();
            eventReturnHandle.TryCancel();
            eventReturnRotateXHandle.TryCancel();
            eventRotateHandle.TryCancel();

            if (restoreCamera)
            {
                RestoreCamera();
            }
        }

        void RestoreCamera()
        {
            if (!hasEventCameraBaseState || mainCameraTransform == null)
            {
                return;
            }

            mainCameraTransform.localPosition = eventCameraBaseLocalPosition;
            mainCameraTransform.localEulerAngles = eventCameraBaseLocalEulerAngles;
            hasEventCameraBaseState = false;
        }

    }
}
