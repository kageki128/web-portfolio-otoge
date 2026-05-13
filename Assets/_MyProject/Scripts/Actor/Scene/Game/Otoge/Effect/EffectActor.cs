using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using R3;

namespace MyProject.Actor
{
    public class EffectActor : OtogeActorBase
    {
        EffectActionsObserver effectActionsObserver;

        public override void InstallActions(OtogeActions otogeActions)
        {
            effectActionsObserver = new EffectActionsObserver(otogeActions);
            LanePressed = effectActionsObserver.LanePressed;
            LaneReleased = effectActionsObserver.LaneReleased;
            AirPressed = Observable.Empty<Unit>();
            AirReleased = Observable.Empty<Unit>();
        }

        public override void Initialize()
        {
            effectActionsObserver.Disable();
            DestroyNotes();
            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await UniTask.WhenAll(NoteActors.Select(noteActor => noteActor.ShowAsync(ct)));
            effectActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            effectActionsObserver.Disable();
            await UniTask.WhenAll(NoteActors.Select(noteActor => noteActor.HideAsync(ct)));
            gameObject.SetActive(false);
        }

        public override void CreateNotes(IReadOnlyList<NoteCoreBase> noteCores)
        {
        }
    }
}
