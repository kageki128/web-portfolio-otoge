using System;
using R3;

namespace MyProject.Actor
{
    public class ScanActionsObserver : ActionsObserverBase, IDisposable
    {
        public Observable<int> LanePressed;
        public Observable<int> LaneReleased;

        readonly OtogeActions.ScanActions scanActions;

        public ScanActionsObserver(OtogeActions otogeActions)
        {
            scanActions = otogeActions.Scan;

            LanePressed = Observable.Merge
            (
                ObservePerformed(scanActions.Lane0).Select(_ => 0),
                ObservePerformed(scanActions.Lane1).Select(_ => 1),
                ObservePerformed(scanActions.Lane2).Select(_ => 2),
                ObservePerformed(scanActions.Lane3).Select(_ => 3)
            );
            LaneReleased = Observable.Merge
            (
                ObserveCanceled(scanActions.Lane0).Select(_ => 0),
                ObserveCanceled(scanActions.Lane1).Select(_ => 1),
                ObserveCanceled(scanActions.Lane2).Select(_ => 2),
                ObserveCanceled(scanActions.Lane3).Select(_ => 3)
            );
        }

        public override void Enable()
        {
            scanActions.Enable();
        }

        public override void Disable()
        {
            scanActions.Disable();
        }
    }
}
