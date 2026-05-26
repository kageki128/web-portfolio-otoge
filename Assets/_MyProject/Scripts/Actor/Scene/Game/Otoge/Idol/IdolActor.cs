using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using MyProject.Core;
using R3;
using UnityEngine;

namespace MyProject.Actor
{
    public class IdolActor : OtogeActorBase
    {
        const float ScrollSpeedMultiplierValue = 2f;

        protected override OtogeType ActorOtogeType => OtogeType.Idol;
        protected override float ScrollSpeedMultiplier => ScrollSpeedMultiplierValue;

        [SerializeField] GameObject noteParent;
        [SerializeField] IdolTapActor tapPrefab;
        [SerializeField] IdolHoldActor holdPrefab;
        [SerializeField] LaneLightActor laneLightActor;
        [SerializeField] SpriteRenderer[] points;

        IdolActionsObserver idolActionsObserver;
        Color[] pointBaseColors;
        MotionHandle[] pointHandles;

        public override void InstallActions(OtogeActions otogeActions)
        {
            idolActionsObserver = new IdolActionsObserver(otogeActions);
            LanePressed = idolActionsObserver.LanePressed;
            LaneReleased = idolActionsObserver.LaneReleased;
            AirPressed = Observable.Empty<Unit>();
            AirReleased = Observable.Empty<Unit>();
        }

        public override void Initialize()
        {
            DestroyNotes();
            laneLightActor.Initialize();
            pointBaseColors = new Color[points.Length];
            pointHandles = new MotionHandle[points.Length];
            for (var i = 0; i < points.Length; i++)
            {
                pointBaseColors[i] = points[i].color;
                points[i].color = WithAlpha(pointBaseColors[i], 0f);
            }

            idolActionsObserver.LanePressed.Subscribe(lane => laneLightActor.LightUp(lane)).AddTo(this);
            idolActionsObserver.LaneReleased.Subscribe(lane => laneLightActor.LightDown(lane)).AddTo(this);

            gameObject.SetActive(false);
            idolActionsObserver.Disable();
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);

            var showTasks = new List<UniTask>
            {
                SwitchActionsAfterDelayAsync(idolActionsObserver.Enable, ct),
                laneLightActor.ShowAsync(ct)
            };
            foreach (var noteActor in NoteActors)
            {
                showTasks.Add(noteActor.ShowAsync(ct));
            }
            showTasks.Add(FadePointsAsync(true, ct));
            await UniTask.WhenAll(showTasks);
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            var hideTasks = new List<UniTask>
            {
                SwitchActionsAfterDelayAsync(idolActionsObserver.Disable, ct),
                laneLightActor.HideAsync(ct)
            };
            foreach (var noteActor in NoteActors)
            {
                hideTasks.Add(noteActor.HideAsync(ct));
            }
            hideTasks.Add(FadePointsAsync(false, ct));
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
                    _ => null
                };

                if (noteActor == null)
                {
                    continue;
                }

                noteActor.InstallCore(noteCore);
                NoteActors.Add(noteActor);
            }
        }

        async UniTask FadePointsAsync(bool show, CancellationToken ct)
        {
            if (!show)
            {
                for (var i = 0; i < points.Length; i++)
                {
                    pointHandles[i].TryCancel();
                    points[i].color = WithAlpha(pointBaseColors[i], 0f);
                }
                return;
            }

            var duration = OtogeAppearance.StateTransitionDuration;
            var ease = OtogeAppearance.StateTransitionEase;
            var fadeTasks = new List<UniTask>(points.Length);

            for (var i = 0; i < points.Length; i++)
            {
                pointHandles[i].TryCancel();

                var point = points[i];
                pointHandles[i] = LMotion.Create(point.color, pointBaseColors[i], duration)
                    .WithEase(ease)
                    .Bind(value => point.color = value)
                    .AddTo(this);
                fadeTasks.Add(pointHandles[i].ToUniTask(CancelBehavior.Cancel, false, ct));
            }

            await UniTask.WhenAll(fadeTasks);
        }

        static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

    }
}
