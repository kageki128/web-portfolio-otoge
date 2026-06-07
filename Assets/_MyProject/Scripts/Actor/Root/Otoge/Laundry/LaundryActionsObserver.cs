using System;
using R3;

namespace MyProject.Actor
{
    public class LaundryActionsObserver : ActionsObserverBase, IDisposable
    {
        public Observable<int> LanePressed;
        public Observable<int> LaneReleased;

        readonly OtogeActions.LaundryActions laundryActions;

        public LaundryActionsObserver(OtogeActions otogeActions)
        {
            laundryActions = otogeActions.Laundry;

            LanePressed = Observable.Merge
            (
                ObservePerformed(laundryActions.Lane0).Select(_ => 0),
                ObservePerformed(laundryActions.Lane1).Select(_ => 1),
                ObservePerformed(laundryActions.Lane2).Select(_ => 2),
                ObservePerformed(laundryActions.Lane3).Select(_ => 3),
                ObservePerformed(laundryActions.Lane4).Select(_ => 4),
                ObservePerformed(laundryActions.Lane5).Select(_ => 5),
                ObservePerformed(laundryActions.Lane6).Select(_ => 6),
                ObservePerformed(laundryActions.Lane7).Select(_ => 7)
            );
            LaneReleased = Observable.Merge
            (
                ObserveCanceled(laundryActions.Lane0).Select(_ => 0),
                ObserveCanceled(laundryActions.Lane1).Select(_ => 1),
                ObserveCanceled(laundryActions.Lane2).Select(_ => 2),
                ObserveCanceled(laundryActions.Lane3).Select(_ => 3),
                ObserveCanceled(laundryActions.Lane4).Select(_ => 4),
                ObserveCanceled(laundryActions.Lane5).Select(_ => 5),
                ObserveCanceled(laundryActions.Lane6).Select(_ => 6),
                ObserveCanceled(laundryActions.Lane7).Select(_ => 7)
            );
        }

        public override void Enable()
        {
            laundryActions.Enable();
        }

        public override void Disable()
        {
            laundryActions.Disable();
        }
    }
}
