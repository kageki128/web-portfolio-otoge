using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;
using R3;

namespace MyProject.Actor
{
    public class TetraActor : OtogeActorBase
    {
        [SerializeField] GameObject noteParent;
        [SerializeField] TetraTapActor tapPrefab;
        [SerializeField] TetraHoldActor holdPrefab;
        [SerializeField] LaneLightActor laneLightActor;

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
            tetraActionsObserver.Disable();

            DestroyNotes();
            laneLightActor.Initialize();

            tetraActionsObserver.LanePressed.Subscribe(lane => laneLightActor.LightUp(lane)).AddTo(this);
            tetraActionsObserver.LaneReleased.Subscribe(lane => laneLightActor.LightDown(lane)).AddTo(this);

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

            tetraActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            tetraActionsObserver.Disable();

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
