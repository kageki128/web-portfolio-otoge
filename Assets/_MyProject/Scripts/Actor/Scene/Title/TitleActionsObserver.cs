namespace MyProject.Actor
{
    public class TitleActionsObserver : ActionsObserverBase
    {
        readonly TitleActions.MainActions mainActions;

        public TitleActionsObserver(TitleActions titleActions)
        {
            mainActions = titleActions.Main;
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
