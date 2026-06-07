using R3;

namespace MyProject.Actor
{
    public class SelectActionsObserver : ActionsObserverBase
    {
        public Observable<Unit> StartGame;

        readonly SelectActions.MainActions mainActions;

        public SelectActionsObserver(SelectActions selectActions)
        {
            mainActions = selectActions.Main;

            StartGame = ObservePerformed(mainActions.StartGame).Select(_ => Unit.Default);
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
