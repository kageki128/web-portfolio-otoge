using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using R3;

namespace MyProject.Actor
{
    public class MasterActor : OtogeActorBase
    {
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
            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await UniTask.WhenAll(NoteActors.Select(noteActor => noteActor.ShowAsync(ct)));
            masterActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            masterActionsObserver.Disable();
            await UniTask.WhenAll(NoteActors.Select(noteActor => noteActor.HideAsync(ct)));
            gameObject.SetActive(false);
        }

        public override void CreateNotes(IReadOnlyList<NoteCoreBase> noteCores)
        {
        }
    }
}
