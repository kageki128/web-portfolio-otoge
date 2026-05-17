using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using R3;
using UnityEngine;

namespace MyProject.Actor
{
    public class RunActor : OtogeActorBase
    {
        [SerializeField] GameObject noteParent;
        [SerializeField] RunTapActor tapPrefab;
        [SerializeField] RunHoldActor holdPrefab;
        [SerializeField] LaneLightActor laneLightActor;

        RunActionsObserver runActionsObserver;

        public override void InstallActions(OtogeActions otogeActions)
        {
            runActionsObserver = new RunActionsObserver(otogeActions);
            LanePressed = runActionsObserver.LanePressed;
            LaneReleased = runActionsObserver.LaneReleased;
            AirPressed = Observable.Empty<Unit>();
            AirReleased = Observable.Empty<Unit>();
        }

        public override void Initialize()
        {
            runActionsObserver.Disable();

            DestroyNotes();
            laneLightActor.Initialize();

            runActionsObserver.LanePressed.Subscribe(lane => laneLightActor.LightUp(lane)).AddTo(this);
            runActionsObserver.LaneReleased.Subscribe(lane => laneLightActor.LightDown(lane)).AddTo(this);

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

            runActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            runActionsObserver.Disable();

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
    }
}
