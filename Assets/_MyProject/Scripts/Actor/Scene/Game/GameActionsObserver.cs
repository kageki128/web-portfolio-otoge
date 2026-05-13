using System;
using R3;

namespace MyProject.Actor
{
    public class GameActionsObserver : ActionsObserverBase, IDisposable
    {
        public Observable<Unit> Quit;

        readonly GameActions.MainActions mainActions;

        public GameActionsObserver(GameActions gameActions)
        {
            mainActions = gameActions.Main;

            Quit = ObservePerformed(mainActions.Quit).Select(_ => Unit.Default);
        }

        public override void Enable()
        {
            mainActions.Enable();
        }

        public override void Disable()
        {
            mainActions.Disable();
        }
    }
}
