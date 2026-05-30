using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;
using R3;

namespace MyProject.Actor
{
    public class TetraActor : OtogeActorBase
    {
        const float ScrollSpeedMultiplierValue = 1f;

        protected override OtogeType ActorOtogeType => OtogeType.Tetra;
        protected override float ScrollSpeedMultiplier => ScrollSpeedMultiplierValue;

        [SerializeField] GameObject noteParent;
        [SerializeField] TetraTapActor tapPrefab;
        [SerializeField] TetraHoldActor holdPrefab;
        [SerializeField] TetraMeasureLineActor measureLinePrefab;
        [SerializeField] LaneLightActor laneLightActor;
        [SerializeField] JudgeEffectFactory judgeEffectFactory;

        TetraActionsObserver tetraActionsObserver;

        public override void InstallActions(OtogeActions otogeActions)
        {
            tetraActionsObserver = new TetraActionsObserver(otogeActions);
            LanePressed = tetraActionsObserver.LanePressed;
            LaneReleased = tetraActionsObserver.LaneReleased;
            AirPressed = Observable.Empty<Unit>();
            AirReleased = Observable.Empty<Unit>();
        }

        public override void Initialize()
        {
            DestroyNotes();
            laneLightActor.Initialize();

            tetraActionsObserver.LanePressed.Subscribe(lane => laneLightActor.LightUp(lane)).AddTo(this);
            tetraActionsObserver.LaneReleased.Subscribe(lane => laneLightActor.LightDown(lane)).AddTo(this);

            gameObject.SetActive(false);
            tetraActionsObserver.Disable();
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);

            var showTasks = new List<UniTask>
            {
                SwitchActionsAfterDelayAsync(tetraActionsObserver.Enable, ct),
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
            var hideTasks = new List<UniTask>
            {
                SwitchActionsAfterDelayAsync(tetraActionsObserver.Disable, ct),
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
                    NoteType.MeasureLine => Instantiate(measureLinePrefab, noteParent.transform),
                    _ => null
                };

                if (noteActor == null)
                {
                    continue;
                }
                noteActor.Install(noteCore, judgeEffectFactory);
                NoteActors.Add(noteActor);
            }
        }

    }
}
