using R3;

namespace MyProject.Actor
{
    public class ResultActionsObserver : ActionsObserverBase
    {
        public Observable<Unit> Quit;
        public Observable<Unit> Retry;
        public Observable<Unit> QuitKeyPressed;
        public Observable<Unit> QuitKeyReleased;
        public Observable<Unit> RetryKeyPressed;
        public Observable<Unit> RetryKeyReleased;

        readonly ResultActions.MainActions mainActions;

        public ResultActionsObserver(ResultActions resultActions)
        {
            mainActions = resultActions.Main;

            Quit = ObservePerformed(mainActions.Quit).Select(_ => Unit.Default);
            Retry = ObservePerformed(mainActions.Retry).Select(_ => Unit.Default);
            QuitKeyPressed = ObserveStarted(mainActions.Quit).Select(_ => Unit.Default);
            QuitKeyReleased = ObserveCanceled(mainActions.Quit).Select(_ => Unit.Default);
            RetryKeyPressed = ObserveStarted(mainActions.Retry).Select(_ => Unit.Default);
            RetryKeyReleased = ObserveCanceled(mainActions.Retry).Select(_ => Unit.Default);
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
