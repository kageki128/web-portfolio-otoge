using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using R3;

namespace MyProject.Actor
{
    public class LaundryActor : OtogeActorBase
    {
        LaundryActionsObserver laundryActionsObserver;

        public override void InstallActions(OtogeActions otogeActions)
        {
            laundryActionsObserver = new LaundryActionsObserver(otogeActions);
            LanePressed = laundryActionsObserver.LanePressed;
            LaneReleased = laundryActionsObserver.LaneReleased;
            AirPressed = Observable.Empty<Unit>();
            AirReleased = Observable.Empty<Unit>();
        }

        public override void Initialize()
        {
            laundryActionsObserver.Disable();
            DestroyNotes();
            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await UniTask.WhenAll(NoteActors.Select(noteActor => noteActor.ShowAsync(ct)));
            laundryActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            laundryActionsObserver.Disable();
            await UniTask.WhenAll(NoteActors.Select(noteActor => noteActor.HideAsync(ct)));
            gameObject.SetActive(false);
        }

        public override void CreateNotes(IReadOnlyList<NoteCoreBase> noteCores)
        {
        }
    }
}
