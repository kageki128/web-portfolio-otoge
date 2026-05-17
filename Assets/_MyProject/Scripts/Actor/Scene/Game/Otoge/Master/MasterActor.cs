using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using R3;
using UnityEngine;

namespace MyProject.Actor
{
    public class MasterActor : OtogeActorBase
    {
        [SerializeField] GameObject noteParent;
        [SerializeField] MasterTapActor tapPrefab;
        [SerializeField] LaneLightActor laneLightActor;

        MasterActionsObserver masterActionsObserver;

        public override void InstallActions(OtogeActions otogeActions)
        {
            masterActionsObserver = new MasterActionsObserver(otogeActions);
            LanePressed = masterActionsObserver.LanePressed;
            LaneReleased = masterActionsObserver.LaneReleased;
            AirPressed = Observable.Empty<Unit>();
            AirReleased = Observable.Empty<Unit>();
        }

        public override void Initialize()
        {
            masterActionsObserver.Disable();

            DestroyNotes();
            laneLightActor.Initialize();

            masterActionsObserver.LanePressed.Subscribe(lane => laneLightActor.LightUp(lane)).AddTo(this);
            masterActionsObserver.LaneReleased.Subscribe(lane => laneLightActor.LightDown(lane)).AddTo(this);

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
            await UniTask.WhenAll(showTasks);

            masterActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            masterActionsObserver.Disable();

            var hideTasks = new List<UniTask>();
            foreach (var noteActor in NoteActors)
            {
                hideTasks.Add(noteActor.HideAsync(ct));
            }
            hideTasks.Add(laneLightActor.HideAsync(ct));
            await UniTask.WhenAll(hideTasks);

            gameObject.SetActive(false);
        }

        public override void CreateNotes(IReadOnlyList<NoteCoreBase> noteCores)
        {
            DestroyNotes();
            foreach (var noteCore in noteCores)
            {
                if (noteCore.Property.Type is not NoteType.Tap)
                {
                    continue;
                }

                var noteActor = Instantiate(tapPrefab, noteParent.transform);
                noteActor.InstallCore(noteCore);
                NoteActors.Add(noteActor);
            }
        }
    }
}
