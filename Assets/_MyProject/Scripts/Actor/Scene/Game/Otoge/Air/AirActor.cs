using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using R3;

namespace MyProject.Actor
{
    public class AirActor : OtogeActorBase
    {
        AirActionsObserver airActionsObserver;

        public override void Initialize()
        {
            airActionsObserver.Disable();
            DestroyNotes();
            gameObject.SetActive(false);
        }

        public override void InstallActions(OtogeActions otogeActions)
        {
            airActionsObserver = new AirActionsObserver(otogeActions);
            LanePressed = airActionsObserver.LanePressed;
            LaneReleased = airActionsObserver.LaneReleased;
            AirPressed = airActionsObserver.AirPressed;
            AirReleased = airActionsObserver.AirReleased;
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await UniTask.WhenAll(NoteActors.Select(noteActor => noteActor.ShowAsync(ct)));
            airActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            airActionsObserver.Disable();
            await UniTask.WhenAll(NoteActors.Select(noteActor => noteActor.HideAsync(ct)));
            gameObject.SetActive(false);
        }

        public override void CreateNotes(IReadOnlyList<NoteCoreBase> noteCores)
        {
        }
    }
}
