using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using R3;
using UnityEngine;

namespace MyProject.Actor
{
    public class ScanActor : OtogeActorBase
    {
        protected override OtogeType ActorOtogeType => OtogeType.Scan;

        [SerializeField] GameObject noteParent;
        [SerializeField] ScanTapActor tapPrefab;
        [SerializeField] ScanHoldActor holdPrefab;
        [SerializeField] ScanHoldTickActor holdTickPrefab;
        [SerializeField] ScanJudgeLineActor judgeLineActor;
        [SerializeField] LaneLightActor laneLightActor;
        [SerializeField] LaneKeysActor laneKeysActor;
        [SerializeField] JudgeEffectFactory judgeEffectFactory;

        ScanActionsObserver scanActionsObserver;

        public override void InstallActions(OtogeActions otogeActions)
        {
            scanActionsObserver = new ScanActionsObserver(otogeActions);
            ActionsObserver = scanActionsObserver;
            LanePressed = scanActionsObserver.LanePressed;
            LaneReleased = scanActionsObserver.LaneReleased;
            AirPressed = Observable.Empty<Unit>();
            AirReleased = Observable.Empty<Unit>();
        }

        public override void Initialize()
        {
            DestroyNotes();
            laneLightActor.Initialize();
            laneKeysActor.Initialize();
            judgeLineActor.Initialize();
            judgeEffectFactory.Initialize();

            scanActionsObserver.LanePressed.Subscribe(lane => laneLightActor.LightUp(lane)).AddTo(this);
            scanActionsObserver.LanePressed.Subscribe(lane => laneKeysActor.LightUpLane(lane)).AddTo(this);
            scanActionsObserver.LaneReleased.Subscribe(lane => laneLightActor.LightDown(lane)).AddTo(this);
            scanActionsObserver.LaneReleased.Subscribe(lane => laneKeysActor.LightDownLane(lane)).AddTo(this);

            gameObject.SetActive(false);
            DisableActions();
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);

            var showTasks = new List<UniTask>
            {
                SwitchActionsAfterDelayAsync(EnableActionsIfAccepted, ct),
                laneLightActor.ShowAsync(ct),
                laneKeysActor.ShowAsync(ct)
            };
            foreach (var noteActor in NoteActors)
            {
                showTasks.Add(noteActor.ShowAsync(ct));
            }
            showTasks.Add(judgeLineActor.ShowAsync(ct));
            await UniTask.WhenAll(showTasks);
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            var hideTasks = new List<UniTask>
            {
                SwitchActionsAfterDelayAsync(DisableActions, ct),
                laneLightActor.HideAsync(ct),
                laneKeysActor.HideAsync(ct)
            };
            foreach (var noteActor in NoteActors)
            {
                hideTasks.Add(noteActor.HideAsync(ct));
            }
            hideTasks.Add(judgeLineActor.HideAsync(ct));
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

        public override void UpdateNotesByTimeline(int timeline, float currentBeat, float currentScroll, float scrollSpeed)
        {
            base.UpdateNotesByTimeline(timeline, currentBeat, currentScroll, scrollSpeed);
            judgeLineActor.SetPosition(currentBeat);
        }
    }
}
