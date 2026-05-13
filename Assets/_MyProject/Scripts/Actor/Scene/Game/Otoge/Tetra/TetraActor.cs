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
            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await UniTask.WhenAll(NoteActors.Select(noteActor => noteActor.ShowAsync(ct)));
            tetraActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            tetraActionsObserver.Disable();
            await UniTask.WhenAll(NoteActors.Select(noteActor => noteActor.HideAsync(ct)));
            gameObject.SetActive(false);
        }

        public override void CreateNotes(IReadOnlyList<NoteCoreBase> noteCores)
        {
            NoteActors.Clear();
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
