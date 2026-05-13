using System;
using R3;

namespace MyProject.Actor
{
    public class OctaActionsObserver : ActionsObserverBase, IDisposable
    {
        public Observable<int> LanePressed;
        public Observable<int> LaneReleased;

        readonly OtogeActions.OctaActions octaActions;

        public OctaActionsObserver(OtogeActions otogeActions)
        {
            octaActions = otogeActions.Octa;

            LanePressed = Observable.Merge
            (
                ObservePerformed(octaActions.Lane0).Select(_ => 0),
                ObservePerformed(octaActions.Lane1).Select(_ => 1),
                ObservePerformed(octaActions.Lane2).Select(_ => 2),
                ObservePerformed(octaActions.Lane3).Select(_ => 3),
                ObservePerformed(octaActions.Lane4).Select(_ => 4),
                ObservePerformed(octaActions.Lane5).Select(_ => 5),
                ObservePerformed(octaActions.Lane6).Select(_ => 6),
                ObservePerformed(octaActions.Lane7).Select(_ => 7)
            );
            LaneReleased = Observable.Merge
            (
                ObserveCanceled(octaActions.Lane0).Select(_ => 0),
                ObserveCanceled(octaActions.Lane1).Select(_ => 1),
                ObserveCanceled(octaActions.Lane2).Select(_ => 2),
                ObserveCanceled(octaActions.Lane3).Select(_ => 3),
                ObserveCanceled(octaActions.Lane4).Select(_ => 4),
                ObserveCanceled(octaActions.Lane5).Select(_ => 5),
                ObserveCanceled(octaActions.Lane6).Select(_ => 6),
                ObserveCanceled(octaActions.Lane7).Select(_ => 7)
            );
        }

        public override void Enable()
        {
            octaActions.Enable();
        }

        public override void Disable()
        {
            octaActions.Disable();
        }
    }
}
