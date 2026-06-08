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
        public ReadOnlyReactiveProperty<OtogeTypeTransition> CurrentOtogeTypeTransition => timing.CurrentOtogeTypeTransition;
        public Observable<Unit> OtogeEvent => timing.OtogeEvent;
        public Observable<Unit> EndReached => endReached;
        public IReadOnlyDictionary<int, ReadOnlyReactiveProperty<float>> TimelineToCurrentScroll => timing.TimelineToCurrentScroll;

        readonly ConductorTiming timing;
        readonly Subject<Unit> endReached = new();
        readonly float endSec;

        double startDspTime;
        bool hasReachedEnd;

        const float SmoothTime = 0.1f;
        float smoothedSec;
        float smoothVelocity;

        public ConductorCore(ConductorTiming timing, float endSec = float.PositiveInfinity)
        {
            this.timing = timing;
            this.endSec = endSec;
        }

        public double Start(double delaySec)
        {
            startDspTime = AudioSettings.dspTime + delaySec;
            smoothedSec = (float)(AudioSettings.dspTime - startDspTime);
            smoothVelocity = 0f;
            hasReachedEnd = false;
            timing.SetTimeBySec(smoothedSec);
            return startDspTime;
        }

        public void Advance(float secOffset)
        {
            float targetSec = (float)(AudioSettings.dspTime - startDspTime) + secOffset;
            smoothedSec = Mathf.SmoothDamp(smoothedSec, targetSec, ref smoothVelocity, SmoothTime);
            timing.SetTimeBySec(smoothedSec);
            if (!hasReachedEnd && smoothedSec >= endSec)
            {
                hasReachedEnd = true;
                endReached.OnNext(Unit.Default);
            }
        }
    }
}
