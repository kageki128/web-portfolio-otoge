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
        public ReadOnlyReactiveProperty<OtogeType> CurrentOtogeType => timing.CurrentOtogeType;
        public IReadOnlyDictionary<int, ReadOnlyReactiveProperty<float>> TimelineToCurrentScroll => timing.TimelineToCurrentScroll;

        readonly ConductorTiming timing;

        double startDspTime;

        const float SmoothTime = 0.1f;
        float smoothedSec;
        float smoothVelocity;

        public ConductorCore(ConductorTiming timing)
        {
            this.timing = timing;
        }

        public double Start(double delaySec)
        {
            startDspTime = AudioSettings.dspTime + delaySec;
            smoothedSec = (float)(AudioSettings.dspTime - startDspTime);
            timing.SetTimeBySec(smoothedSec);
            return startDspTime;
        }

        public void Advance()
        {
            float targetSec = (float)(AudioSettings.dspTime - startDspTime);
            smoothedSec = Mathf.SmoothDamp(smoothedSec, targetSec, ref smoothVelocity, SmoothTime);
            timing.SetTimeBySec(smoothedSec);
        }
    }
}
