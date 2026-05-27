using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using R3;
using UnityEngine;

namespace MyProject.Actor
{
    public class EffectActor : OtogeActorBase
    {
        const float ScrollSpeedMultiplierValue = 5.5f;

        protected override OtogeType ActorOtogeType => OtogeType.Effect;
        protected override float ScrollSpeedMultiplier => ScrollSpeedMultiplierValue;

        [SerializeField] GameObject noteParent;
        [SerializeField] EffectTapActor tapPrefab;
        [SerializeField] EffectHoldActor holdPrefab;
        [SerializeField] LaneLightActor laneLightActor;

        EffectActionsObserver effectActionsObserver;

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
            DestroyNotes();
            laneLightActor.Initialize();

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

        public override void ExecuteEvent()
        {
            Debug.Log("EffectActor: ExecuteEvent");
        }

    }
}
