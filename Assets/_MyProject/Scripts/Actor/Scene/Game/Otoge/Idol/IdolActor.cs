using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using R3;

namespace MyProject.Actor
{
    public class IdolActor : OtogeActorBase
    {
        IdolActionsObserver idolActionsObserver;

        public override void InstallActions(OtogeActions otogeActions)
        {
            idolActionsObserver = new IdolActionsObserver(otogeActions);
            LanePressed = idolActionsObserver.LanePressed;
            LaneReleased = idolActionsObserver.LaneReleased;
            AirPressed = Observable.Empty<Unit>();
            AirReleased = Observable.Empty<Unit>();
        }

        public override void Initialize()
        {
            idolActionsObserver.Disable();
            DestroyNotes();
            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await UniTask.WhenAll(NoteActors.Select(noteActor => noteActor.ShowAsync(ct)));
            idolActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            idolActionsObserver.Disable();
            await UniTask.WhenAll(NoteActors.Select(noteActor => noteActor.HideAsync(ct)));
            gameObject.SetActive(false);
        }

        public override void CreateNotes(IReadOnlyList<NoteCoreBase> noteCores)
        {
        }
    }
}
