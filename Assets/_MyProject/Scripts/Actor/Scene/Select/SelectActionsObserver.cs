using R3;
using UnityEngine.InputSystem;

namespace MyProject.Actor
{
    public class SelectActionsObserver : ActionsObserverBase
    {
        public Observable<Unit> StartGame;
        public Observable<int> DifficultyScrolled;
        public Observable<int> DifficultyScrollStarted;
        public Observable<Unit> DifficultyScrollCanceled;

        readonly SelectActions.MainActions mainActions;

        public SelectActionsObserver(SelectActions selectActions)
        {
            mainActions = selectActions.Main;

            StartGame = ObservePerformed(mainActions.StartGame).Select(_ => Unit.Default);
            DifficultyScrolled = ObservePerformed(mainActions.ScrollDifficulty)
                .Select(ReadDirection)
                .Where(direction => direction != 0);
            DifficultyScrollStarted = ObserveStarted(mainActions.ScrollDifficulty)
                .Select(ReadDirection)
                .Where(direction => direction != 0);
            DifficultyScrollCanceled = ObserveCanceled(mainActions.ScrollDifficulty)
                .Select(_ => Unit.Default);
        }

        public override void Enable()
        {
            mainActions.Enable();
        }

        public override void Disable()
        {
            mainActions.Disable();
        }

        static int ReadDirection(InputAction.CallbackContext context)
        {
            var value = context.ReadValue<float>();
            if (value > 0f)
            {
                return 1;
            }

            return value < 0f ? -1 : 0;
        }
    }
}
