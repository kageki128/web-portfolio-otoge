using System;
using R3;

namespace MyProject.Actor
{
    public class GameActionsObserver : ActionsObserverBase, IDisposable
    {
        public Observable<Unit> Quit;
        public Observable<Unit> BackKeyPressed;
        public Observable<Unit> BackKeyReleased;

        readonly GameActions.MainActions mainActions;

        public GameActionsObserver(GameActions gameActions)
        {
            mainActions = gameActions.Main;

            Quit = ObservePerformed(mainActions.Quit).Select(_ => Unit.Default);
            BackKeyPressed = ObserveStarted(mainActions.Quit).Select(_ => Unit.Default);
            BackKeyReleased = ObserveCanceled(mainActions.Quit).Select(_ => Unit.Default);
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
