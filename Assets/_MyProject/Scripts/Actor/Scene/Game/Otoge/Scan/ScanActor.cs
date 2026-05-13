using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using R3;

namespace MyProject.Actor
{
    public class ScanActor : OtogeActorBase
    {
        ScanActionsObserver scanActionsObserver;

        public override void InstallActions(OtogeActions otogeActions)
        {
            scanActionsObserver = new ScanActionsObserver(otogeActions);
            LanePressed = scanActionsObserver.LanePressed;
            LaneReleased = scanActionsObserver.LaneReleased;
            AirPressed = Observable.Empty<Unit>();
            AirReleased = Observable.Empty<Unit>();
        }

        public override void Initialize()
        {
            scanActionsObserver.Disable();
            DestroyNotes();
            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await UniTask.WhenAll(NoteActors.Select(noteActor => noteActor.ShowAsync(ct)));
            scanActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            scanActionsObserver.Disable();
            await UniTask.WhenAll(NoteActors.Select(noteActor => noteActor.HideAsync(ct)));
            gameObject.SetActive(false);
        }

        public override void CreateNotes(IReadOnlyList<NoteCoreBase> noteCores)
        {
        }
    }
}
