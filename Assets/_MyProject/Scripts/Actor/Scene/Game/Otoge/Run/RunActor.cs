using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using R3;

namespace MyProject.Actor
{
    public class RunActor : OtogeActorBase
    {
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
            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await UniTask.WhenAll(NoteActors.Select(noteActor => noteActor.ShowAsync(ct)));
            runActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            runActionsObserver.Disable();
            await UniTask.WhenAll(NoteActors.Select(noteActor => noteActor.HideAsync(ct)));
            gameObject.SetActive(false);
        }

        public override void CreateNotes(IReadOnlyList<NoteCoreBase> noteCores)
        {
        }
    }
}
