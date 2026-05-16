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
        [SerializeField] GameObject noteParent;
        [SerializeField] AirTapActor tapPrefab;
        [SerializeField] AirHoldActor holdPrefab;
        [SerializeField] AirAirActor airPrefab;
        [SerializeField] LaneLightActor laneLightActor;

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
            airActionsObserver.Disable();

            DestroyNotes();
            laneLightActor.Initialize();

            airActionsObserver.LanePressed.Subscribe(lane => laneLightActor.LightUp(lane)).AddTo(this);
            airActionsObserver.LaneReleased.Subscribe(lane => laneLightActor.LightDown(lane)).AddTo(this);

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

            airActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            airActionsObserver.Disable();

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
                    NoteType.Air => Instantiate(airPrefab, noteParent.transform),
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
