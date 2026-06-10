using System;
using R3;

namespace MyProject.Actor
{
    public class GameActionsObserver : ActionsObserverBase, IDisposable
    {
        public Observable<Unit> Quit;
        public Observable<Unit> ChangeAuto;
        public Observable<Unit> BackKeyPressed;
        public Observable<Unit> BackKeyReleased;

        readonly GameActions.MainActions mainActions;

        public GameActionsObserver(GameActions gameActions)
        {
            mainActions = gameActions.Main;

            Quit = ObservePerformed(mainActions.Quit).Select(_ => Unit.Default);
#if UNITY_EDITOR || UNITY_STANDALONE
            ChangeAuto = ObservePerformed(mainActions.ChangeAuto).Select(_ => Unit.Default);
#else
            ChangeAuto = Observable.Empty<Unit>();
#endif
            BackKeyPressed = ObserveStarted(mainActions.Quit).Select(_ => Unit.Default);
            BackKeyReleased = ObserveCanceled(mainActions.Quit).Select(_ => Unit.Default);
        }

        public override void Enable()
        {
            mainActions.Enable();
#if !UNITY_EDITOR && !UNITY_STANDALONE
            mainActions.ChangeAuto.Disable();
#endif
        }

        public override void Disable()
        {
            mainActions.Disable();
        }
    }
}
