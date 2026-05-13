using System;
using R3;

namespace MyProject.Actor
{
    public class TetraActionsObserver : ActionsObserverBase, IDisposable
    {
        public Observable<int> LanePressed;
        public Observable<int> LaneReleased;

        readonly OtogeActions.TetraActions tetraActions;

        public TetraActionsObserver(OtogeActions otogeActions)
        {
            tetraActions = otogeActions.Tetra;

            LanePressed = Observable.Merge
            (
                ObservePerformed(tetraActions.Lane0).Select(_ => 0),
                ObservePerformed(tetraActions.Lane1).Select(_ => 1),
                ObservePerformed(tetraActions.Lane2).Select(_ => 2),
                ObservePerformed(tetraActions.Lane3).Select(_ => 3)
            );
            LaneReleased = Observable.Merge
            (
                ObserveCanceled(tetraActions.Lane0).Select(_ => 0),
                ObserveCanceled(tetraActions.Lane1).Select(_ => 1),
                ObserveCanceled(tetraActions.Lane2).Select(_ => 2),
                ObserveCanceled(tetraActions.Lane3).Select(_ => 3)
            );
        }

        public override void Enable()
        {
            tetraActions.Enable();
        }

        public override void Disable()
        {
            tetraActions.Disable();
        }
    }
}
