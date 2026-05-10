using System;
using R3;

namespace MyProject.Actor
{
    public class GameActionsObserver : ActionsObserverBase, IDisposable
    {
        public Observable<int> LanePressed => lanePressed;
        readonly Subject<int> lanePressed = new();

        readonly GameActions.QuadActions quadActions;

        readonly CompositeDisposable disposables = new();

        public GameActionsObserver(GameActions gameActions)
        {
            quadActions = gameActions.Quad;

            ObserveStarted(quadActions.Lane0).Subscribe(_ => lanePressed.OnNext(0)).AddTo(disposables);
            ObserveStarted(quadActions.Lane1).Subscribe(_ => lanePressed.OnNext(1)).AddTo(disposables);
            ObserveStarted(quadActions.Lane2).Subscribe(_ => lanePressed.OnNext(2)).AddTo(disposables);
            ObserveStarted(quadActions.Lane3).Subscribe(_ => lanePressed.OnNext(3)).AddTo(disposables);
        }

        public override void Enable()
        {
            quadActions.Enable();
        }

        public override void Disable()
        {
            quadActions.Disable();
        }

        public override void Dispose()
        {
            base.Dispose();
            lanePressed.Dispose();
            disposables.Dispose();
        }
    }
}
