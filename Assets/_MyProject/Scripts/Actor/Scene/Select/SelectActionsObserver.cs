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
        public Observable<int> InputKeyPressed;
        public Observable<int> InputKeyReleased;

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
            InputKeyPressed = Observable.Merge
            (
                ObservePerformed(mainActions.key0).Select(_ => 0),
                ObservePerformed(mainActions.key1).Select(_ => 1),
                ObservePerformed(mainActions.key2).Select(_ => 2),
                ObservePerformed(mainActions.key3).Select(_ => 3),
                ObservePerformed(mainActions.key4).Select(_ => 4),
                ObservePerformed(mainActions.key5).Select(_ => 5),
                ObservePerformed(mainActions.key6).Select(_ => 6),
                ObservePerformed(mainActions.key7).Select(_ => 7),
                ObservePerformed(mainActions.key8).Select(_ => 8)
            );
            InputKeyReleased = Observable.Merge
            (
                ObserveCanceled(mainActions.key0).Select(_ => 0),
                ObserveCanceled(mainActions.key1).Select(_ => 1),
                ObserveCanceled(mainActions.key2).Select(_ => 2),
                ObserveCanceled(mainActions.key3).Select(_ => 3),
                ObserveCanceled(mainActions.key4).Select(_ => 4),
                ObserveCanceled(mainActions.key5).Select(_ => 5),
                ObserveCanceled(mainActions.key6).Select(_ => 6),
                ObserveCanceled(mainActions.key7).Select(_ => 7),
                ObserveCanceled(mainActions.key8).Select(_ => 8)
            );
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
