using System.Collections.Generic;
using R3;
using UnityEngine;

namespace MyProject.Core
{
    public class ConductorCore
    {
        public ReadOnlyReactiveProperty<float> CurrentBeat => timing.CurrentBeat;
        public ReadOnlyReactiveProperty<float> CurrentSec => timing.CurrentSec;
        public ReadOnlyReactiveProperty<int> CurrentMeasure => timing.CurrentMeasure;
        public IReadOnlyDictionary<int, ReadOnlyReactiveProperty<float>> TimelineToCurrentScroll => timing.TimelineToCurrentScroll;

        readonly ConductorTiming timing;

        double startDspTime;

        public ConductorCore(ConductorTiming timing)
        {
            this.timing = timing;
        }

        public double Start(double delaySec)
        {
            startDspTime = AudioSettings.dspTime + delaySec;
            return startDspTime;
        }

        public void Advance()
        {
            float currentSec = (float)(AudioSettings.dspTime - startDspTime);
            timing.SetTimeBySec(currentSec);
        }
    }
}
