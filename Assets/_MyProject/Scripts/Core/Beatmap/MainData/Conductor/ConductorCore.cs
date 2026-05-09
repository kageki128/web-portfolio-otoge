using System.Collections.Generic;
using R3;

namespace MyProject.Core
{
    public class ConductorCore
    {
        public ReadOnlyReactiveProperty<float> CurrentBeat => timing.CurrentBeat;
        public ReadOnlyReactiveProperty<float> CurrentSec => timing.CurrentSec;
        public ReadOnlyReactiveProperty<int> CurrentMeasure => timing.CurrentMeasure;
        public IReadOnlyDictionary<int, ReadOnlyReactiveProperty<float>> TimelineToCurrentScroll => timing.TimelineToCurrentScroll;

        readonly ConductorTiming timing;

        public ConductorCore(ConductorTiming timing)
        {
            this.timing = timing;
        }

        public void AdvanceTimeByDeltaSec(float deltaSec)
        {
            timing.AdvanceTimeByDeltaSec(deltaSec);
        }
    }
}
