namespace MyProject.Actor
{
    public class ResultActionsObserver : ActionsObserverBase
    {
        readonly ResultActions.MainActions mainActions;

        public ResultActionsObserver(ResultActions resultActions)
        {
            mainActions = resultActions.Main;
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
