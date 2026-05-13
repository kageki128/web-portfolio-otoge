using System;
using R3;

namespace MyProject.Actor
{
    public class EffectActionsObserver : ActionsObserverBase, IDisposable
    {
        public Observable<int> LanePressed;
        public Observable<int> LaneReleased;

        readonly OtogeActions.EffectActions effectActions;

        public EffectActionsObserver(OtogeActions otogeActions)
        {
            effectActions = otogeActions.Effect;

            LanePressed = Observable.Merge
            (
                ObservePerformed(effectActions.Lane0).Select(_ => 0),
                ObservePerformed(effectActions.Lane1).Select(_ => 1),
                ObservePerformed(effectActions.Lane2).Select(_ => 2),
                ObservePerformed(effectActions.Lane3).Select(_ => 3),
                ObservePerformed(effectActions.Lane4).Select(_ => 4)
            );
            LaneReleased = Observable.Merge
            (
                ObserveCanceled(effectActions.Lane0).Select(_ => 0),
                ObserveCanceled(effectActions.Lane1).Select(_ => 1),
                ObserveCanceled(effectActions.Lane2).Select(_ => 2),
                ObserveCanceled(effectActions.Lane3).Select(_ => 3),
                ObserveCanceled(effectActions.Lane4).Select(_ => 4)
            );
        }

        public override void Enable()
        {
            effectActions.Enable();
        }

        public override void Disable()
        {
            effectActions.Disable();
        }
    }
}
