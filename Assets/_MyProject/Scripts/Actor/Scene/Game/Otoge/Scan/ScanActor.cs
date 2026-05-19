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
        [SerializeField] GameObject noteParent;
        [SerializeField] ScanTapActor tapPrefab;
        [SerializeField] ScanHoldActor holdPrefab;
        [SerializeField] ScanJudgeLineActor judgeLineActor;
        [SerializeField] LaneLightActor laneLightActor;

        ScanActionsObserver scanActionsObserver;

        public override void InstallActions(OtogeActions otogeActions)
        {
            scanActionsObserver = new ScanActionsObserver(otogeActions);
            LanePressed = scanActionsObserver.LanePressed;
            LaneReleased = scanActionsObserver.LaneReleased;
            AirPressed = Observable.Empty<Unit>();
            AirReleased = Observable.Empty<Unit>();
        }

        public override void Initialize()
        {
            scanActionsObserver.Disable();

            DestroyNotes();
            laneLightActor.Initialize();
            judgeLineActor.Initialize();

            scanActionsObserver.LanePressed.Subscribe(lane => laneLightActor.LightUp(lane)).AddTo(this);
            scanActionsObserver.LaneReleased.Subscribe(lane => laneLightActor.LightDown(lane)).AddTo(this);

            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);

            var showTasks = new List<UniTask>();
            foreach (var noteActor in NoteActors)
            {
                showTasks.Add(noteActor.ShowAsync(ct));
            }
            showTasks.Add(laneLightActor.ShowAsync(ct));
            showTasks.Add(judgeLineActor.ShowAsync(ct));
            await UniTask.WhenAll(showTasks);

            scanActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            scanActionsObserver.Disable();

            var hideTasks = new List<UniTask>();
            foreach (var noteActor in NoteActors)
            {
                hideTasks.Add(noteActor.HideAsync(ct));
            }
            hideTasks.Add(laneLightActor.HideAsync(ct));
            hideTasks.Add(judgeLineActor.HideAsync(ct));
            await UniTask.WhenAll(hideTasks);

            gameObject.SetActive(false);
        }

        public override void CreateNotes(IReadOnlyList<NoteCoreBase> noteCores)
        {
            DestroyNotes();
            foreach (var noteCore in noteCores)
            {
                var noteType = noteCore.Property.Type;
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

        public override void UpdateNotesByTimeline(int timeline, float currentBeat, float currentScroll, float scrollSpeed)
        {
            base.UpdateNotesByTimeline(timeline, currentBeat, currentScroll, scrollSpeed);
            judgeLineActor.SetPosition(currentBeat);
        }
    }
}
