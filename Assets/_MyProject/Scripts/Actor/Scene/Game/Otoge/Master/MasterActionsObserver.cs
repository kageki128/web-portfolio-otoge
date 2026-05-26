using System;
using R3;

namespace MyProject.Actor
{
    public class MasterActionsObserver : ActionsObserverBase, IDisposable
    {
        public Observable<int> LanePressed;
        public Observable<int> LaneReleased;

        readonly OtogeActions.MasterActions masterActions;

        public MasterActionsObserver(OtogeActions otogeActions)
        {
            masterActions = otogeActions.Master;

            LanePressed = Observable.Merge
            (
                ObservePerformed(masterActions.Lane0_L).Select(_ => 0),
                ObservePerformed(masterActions.Lane0_R).Select(_ => 0),
                ObservePerformed(masterActions.Lane1_L).Select(_ => 1),
                ObservePerformed(masterActions.Lane1_R).Select(_ => 1)
            );
            LaneReleased = Observable.Merge
            (
                ObserveCanceled(masterActions.Lane0_L).Select(_ => 0),
                ObserveCanceled(masterActions.Lane0_R).Select(_ => 0),
                ObserveCanceled(masterActions.Lane1_L).Select(_ => 1),
                ObserveCanceled(masterActions.Lane1_R).Select(_ => 1)
            );
        }

        public override void Enable()
        {
            masterActions.Enable();
        }

        public override void Disable()
        {
            masterActions.Disable();
        }
    }
}
