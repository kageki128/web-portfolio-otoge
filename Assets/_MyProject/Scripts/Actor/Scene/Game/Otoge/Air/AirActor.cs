using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using R3;
using UnityEngine;

namespace MyProject.Actor
{
    public class AirActor : OtogeActorBase
    {
        const float ScrollSpeedMultiplierValue = 4f;

        protected override OtogeType ActorOtogeType => OtogeType.Air;
        protected override float ScrollSpeedMultiplier => ScrollSpeedMultiplierValue;

        [SerializeField] GameObject noteParent;
        [SerializeField] AirTapActor tapPrefab;
        [SerializeField] AirHoldActor holdPrefab;
        [SerializeField] AirHoldTickActor holdTickPrefab;
        [SerializeField] AirAirActor airPrefab;
        [SerializeField] AirMeasureLineActor measureLinePrefab;
        [SerializeField] LaneLightActor laneLightActor;
        [SerializeField] LaneKeysActor laneKeysActor;
        [SerializeField] JudgeEffectFactory judgeEffectFactory;

        AirActionsObserver airActionsObserver;

        public override void InstallActions(OtogeActions otogeActions)
        {
            airActionsObserver = new AirActionsObserver(otogeActions);
            LanePressed = airActionsObserver.LanePressed;
            LaneReleased = airActionsObserver.LaneReleased;
            AirPressed = airActionsObserver.AirPressed;
            AirReleased = airActionsObserver.AirReleased;
        }

        public override void Initialize()
        {
            DestroyNotes();
            laneLightActor.Initialize();
            laneKeysActor.Initialize();
            judgeEffectFactory.Initialize();

            airActionsObserver.LanePressed.Subscribe(lane => laneLightActor.LightUp(lane)).AddTo(this);
            airActionsObserver.LanePressed.Subscribe(lane => laneKeysActor.LightUpLane(lane)).AddTo(this);
            airActionsObserver.LaneReleased.Subscribe(lane => laneLightActor.LightDown(lane)).AddTo(this);
            airActionsObserver.LaneReleased.Subscribe(lane => laneKeysActor.LightDownLane(lane)).AddTo(this);
            airActionsObserver.AirPressed.Subscribe(_ => laneKeysActor.LightUpAir()).AddTo(this);
            airActionsObserver.AirReleased.Subscribe(_ => laneKeysActor.LightDownAir()).AddTo(this);

            gameObject.SetActive(false);
            airActionsObserver.Disable();
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);

            var showTasks = new List<UniTask>
            {
                SwitchActionsAfterDelayAsync(airActionsObserver.Enable, ct),
                laneLightActor.ShowAsync(ct),
                laneKeysActor.ShowAsync(ct)
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
                SwitchActionsAfterDelayAsync(airActionsObserver.Disable, ct),
                laneLightActor.HideAsync(ct),
                laneKeysActor.HideAsync(ct)
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
                    NoteType.Air => Instantiate(airPrefab, noteParent.transform),
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

    }
}
