using R3;

namespace MyProject.Actor
{
    public class ResultActionsObserver : ActionsObserverBase
    {
        public Observable<Unit> Quit;
        public Observable<Unit> Retry;

        readonly ResultActions.MainActions mainActions;

        public ResultActionsObserver(ResultActions resultActions)
        {
            mainActions = resultActions.Main;

            Quit = ObservePerformed(mainActions.Quit).Select(_ => Unit.Default);
            Retry = ObservePerformed(mainActions.Retry).Select(_ => Unit.Default);
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
