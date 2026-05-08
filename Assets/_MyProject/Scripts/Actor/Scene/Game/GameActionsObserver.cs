namespace MyProject.Actor
{
    public class GameActionsObserver : ActionsObserverBase
    {
        readonly GameActions.MainActions mainActions;

        public GameActionsObserver(GameActions gameActions)
        {
            mainActions = gameActions.Main;
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
