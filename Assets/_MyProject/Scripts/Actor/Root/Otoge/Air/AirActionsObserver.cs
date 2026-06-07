using System;
using R3;

namespace MyProject.Actor
{
    public class AirActionsObserver : ActionsObserverBase, IDisposable
    {
        public Observable<int> LanePressed;
        public Observable<int> LaneReleased;
        public Observable<Unit> AirPressed;
        public Observable<Unit> AirReleased;

        readonly OtogeActions.AirActions airActions;

        public AirActionsObserver(OtogeActions otogeActions)
        {
            airActions = otogeActions.Air;

            LanePressed = Observable.Merge
            (
                ObservePerformed(airActions.Lane0).Select(_ => 0),
                ObservePerformed(airActions.Lane1).Select(_ => 1),
                ObservePerformed(airActions.Lane2).Select(_ => 2),
                ObservePerformed(airActions.Lane3).Select(_ => 3),
                ObservePerformed(airActions.Lane4).Select(_ => 4),
                ObservePerformed(airActions.Lane5).Select(_ => 5),
                ObservePerformed(airActions.Lane6).Select(_ => 6),
                ObservePerformed(airActions.Lane7).Select(_ => 7)
            );
            LaneReleased = Observable.Merge
            (
                ObserveCanceled(airActions.Lane0).Select(_ => 0),
                ObserveCanceled(airActions.Lane1).Select(_ => 1),
                ObserveCanceled(airActions.Lane2).Select(_ => 2),
                ObserveCanceled(airActions.Lane3).Select(_ => 3),
                ObserveCanceled(airActions.Lane4).Select(_ => 4),
                ObserveCanceled(airActions.Lane5).Select(_ => 5),
                ObserveCanceled(airActions.Lane6).Select(_ => 6),
                ObserveCanceled(airActions.Lane7).Select(_ => 7)
            );
            AirPressed = ObservePerformed(airActions.Air).Select(_ => Unit.Default);
            AirReleased = ObserveCanceled(airActions.Air).Select(_ => Unit.Default);
        }

        public override void Enable()
        {
            airActions.Enable();
        }

        public override void Disable()
        {
            airActions.Disable();
        }
    }
}
