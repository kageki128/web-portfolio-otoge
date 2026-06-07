using System;
using R3;

namespace MyProject.Actor
{
    public class RunActionsObserver : ActionsObserverBase, IDisposable
    {
        public Observable<int> LanePressed;
        public Observable<int> LaneReleased;

        readonly OtogeActions.RunActions runActions;

        public RunActionsObserver(OtogeActions otogeActions)
        {
            runActions = otogeActions.Run;

            LanePressed = Observable.Merge
            (
                ObservePerformed(runActions.Lane0_L).Select(_ => 0),
                ObservePerformed(runActions.Lane0_R).Select(_ => 0),
                ObservePerformed(runActions.Lane1_L).Select(_ => 1),
                ObservePerformed(runActions.Lane1_R).Select(_ => 1)
            );
            LaneReleased = Observable.Merge
            (
                ObserveCanceled(runActions.Lane0_L).Select(_ => 0),
                ObserveCanceled(runActions.Lane0_R).Select(_ => 0),
                ObserveCanceled(runActions.Lane1_L).Select(_ => 1),
                ObserveCanceled(runActions.Lane1_R).Select(_ => 1)
            );
        }

        public override void Enable()
        {
            runActions.Enable();
        }

        public override void Disable()
        {
            runActions.Disable();
        }
    }
}
