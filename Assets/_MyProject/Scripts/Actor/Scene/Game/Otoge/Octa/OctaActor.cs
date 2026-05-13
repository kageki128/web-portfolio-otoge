using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using R3;

namespace MyProject.Actor
{
    public class OctaActor : OtogeActorBase
    {
        OctaActionsObserver octaActionsObserver;

        public override void InstallActions(OtogeActions otogeActions)
        {
            octaActionsObserver = new OctaActionsObserver(otogeActions);
            LanePressed = octaActionsObserver.LanePressed;
            LaneReleased = octaActionsObserver.LaneReleased;
            AirPressed = Observable.Empty<Unit>();
            AirReleased = Observable.Empty<Unit>();
        }

        public override void Initialize()
        {
            octaActionsObserver.Disable();
            DestroyNotes();
            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await UniTask.WhenAll(NoteActors.Select(noteActor => noteActor.ShowAsync(ct)));
            octaActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            octaActionsObserver.Disable();
            await UniTask.WhenAll(NoteActors.Select(noteActor => noteActor.HideAsync(ct)));
            gameObject.SetActive(false);
        }

        public override void CreateNotes(IReadOnlyList<NoteCoreBase> noteCores)
        {
        }
    }
}
