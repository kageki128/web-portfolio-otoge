namespace MyProject.Actor
{
    public class SelectActionsObserver : ActionsObserverBase
    {
        readonly SelectActions.MainActions mainActions;

        public SelectActionsObserver(SelectActions selectActions)
        {
            mainActions = selectActions.Main;
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
