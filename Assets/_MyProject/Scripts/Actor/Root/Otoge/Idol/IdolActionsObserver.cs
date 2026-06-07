using System;
using R3;

namespace MyProject.Actor
{
    public class IdolActionsObserver : ActionsObserverBase, IDisposable
    {
        public Observable<int> LanePressed;
        public Observable<int> LaneReleased;

        readonly OtogeActions.IdolActions idolActions;

        public IdolActionsObserver(OtogeActions otogeActions)
        {
            idolActions = otogeActions.Idol;

            LanePressed = Observable.Merge
            (
                ObservePerformed(idolActions.Lane0).Select(_ => 0),
                ObservePerformed(idolActions.Lane1).Select(_ => 1),
                ObservePerformed(idolActions.Lane2).Select(_ => 2),
                ObservePerformed(idolActions.Lane3).Select(_ => 3),
                ObservePerformed(idolActions.Lane4).Select(_ => 4)
            );
            LaneReleased = Observable.Merge
            (
                ObserveCanceled(idolActions.Lane0).Select(_ => 0),
                ObserveCanceled(idolActions.Lane1).Select(_ => 1),
                ObserveCanceled(idolActions.Lane2).Select(_ => 2),
                ObserveCanceled(idolActions.Lane3).Select(_ => 3),
                ObserveCanceled(idolActions.Lane4).Select(_ => 4)
            );
        }

        public override void Enable()
        {
            idolActions.Enable();
        }

        public override void Disable()
        {
            idolActions.Disable();
        }
    }
}
