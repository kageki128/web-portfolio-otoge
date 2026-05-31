using System.Collections.Generic;
using System.Linq;
using R3;
using static MyProject.Core.TimingCalculator;

namespace MyProject.Core
{
    public class ConductorTiming
    {
        public ReadOnlyReactiveProperty<float> CurrentBeat => currentBeat;
        readonly ReactiveProperty<float> currentBeat = new(0);

        public ReadOnlyReactiveProperty<float> CurrentSec => currentSec;
        readonly ReactiveProperty<float> currentSec = new(0);

        public ReadOnlyReactiveProperty<int> CurrentMeasure => currentMeasure;
        readonly ReactiveProperty<int> currentMeasure = new(0);

        public IReadOnlyDictionary<int, ReadOnlyReactiveProperty<float>> TimelineToCurrentScroll { get; }
        readonly Dictionary<int, ReactiveProperty<float>> timelineToCurrentScroll = new();

        public ReadOnlyReactiveProperty<OtogeTypeTransition> CurrentOtogeTypeTransition => currentOtogeTypeTransition;
        readonly ReactiveProperty<OtogeTypeTransition> currentOtogeTypeTransition = new(new OtogeTypeTransition(OtogeType.Tetra, OtogeType.Tetra, 0f));

        public Observable<Unit> OtogeEvent => otogeEvent;
        readonly Subject<Unit> otogeEvent = new();

        readonly IReadOnlyList<BpmChange> bpmChanges;
        readonly IReadOnlyDictionary<int, IReadOnlyList<HighSpeedChange>> timelineToHighSpeedChanges;
        readonly IReadOnlyList<MeasureLengthChange> measureLengthChanges;
        readonly IReadOnlyList<OtogeChange> otogeChanges;
        readonly float[] sortedOtogeEventBeats;
        int nextOtogeEventIndex;

        public ConductorTiming
        (
            IReadOnlyList<BpmChange> bpmChanges,
            IReadOnlyDictionary<int, IReadOnlyList<HighSpeedChange>> timelineToHighSpeedChanges,
            IReadOnlyList<MeasureLengthChange> measureLengthChanges,
            IReadOnlyList<OtogeChange> otogeChanges,
            IReadOnlyList<float> otogeEventBeats
        )
        {
            this.bpmChanges = bpmChanges;
            this.timelineToHighSpeedChanges = timelineToHighSpeedChanges;
            this.measureLengthChanges = measureLengthChanges;
            this.otogeChanges = otogeChanges.OrderBy(change => change.Beat).ToArray();
            sortedOtogeEventBeats = otogeEventBeats.OrderBy(beat => beat).ToArray();
            nextOtogeEventIndex = 0;

            var readOnlyTimelineToCurrentScroll = new Dictionary<int, ReadOnlyReactiveProperty<float>>();
            foreach (var timelineToHighSpeedChange in timelineToHighSpeedChanges)
            {
                var currentScroll = new ReactiveProperty<float>(0);
                timelineToCurrentScroll[timelineToHighSpeedChange.Key] = currentScroll;
                readOnlyTimelineToCurrentScroll[timelineToHighSpeedChange.Key] = currentScroll;
            }
            TimelineToCurrentScroll = readOnlyTimelineToCurrentScroll;
            currentOtogeTypeTransition.Value = CalculateOtogeTypeTransitionFromBeat(0f, 0f);
        }

        public void SetTimeBySec(float sec)
        {
            var newSec = sec;
            var previousBeat = currentBeat.Value;
            var newBeat = CalculateBeatFromSec(newSec, bpmChanges);
            var newMeasure = CalculateMeasureFromBeat(newBeat, measureLengthChanges);

            currentSec.Value = newSec;
            currentBeat.Value = newBeat;
            currentMeasure.Value = newMeasure;
            currentOtogeTypeTransition.Value = CalculateOtogeTypeTransitionFromBeat(newBeat, newSec);
            TryTriggerOtogeEvent(previousBeat, newBeat);

            foreach (var timelineToHighSpeedChange in timelineToHighSpeedChanges)
            {
                timelineToCurrentScroll[timelineToHighSpeedChange.Key].Value = CalculateScrollFromBeat(newBeat, bpmChanges, timelineToHighSpeedChange.Value);
            }
        }

        void TryTriggerOtogeEvent(float previousBeat, float newBeat)
        {
            if (nextOtogeEventIndex >= sortedOtogeEventBeats.Length)
            {
                return;
            }

            var nextEventBeat = sortedOtogeEventBeats[nextOtogeEventIndex];
            if (previousBeat < nextEventBeat && newBeat >= nextEventBeat)
            {
                nextOtogeEventIndex++;
                otogeEvent.OnNext(Unit.Default);
            }
        }

        OtogeTypeTransition CalculateOtogeTypeTransitionFromBeat(float beat, float sec)
        {
            var currentType = OtogeType.Tetra;
            for (var i = 0; i < otogeChanges.Count; i++)
            {
                var change = otogeChanges[i];
                if (change.Beat >= beat)
                {
                    var nextSec = CalculateSecFromBeat(change.Beat, bpmChanges);
                    var remainingSec = nextSec - sec;
                    if (remainingSec < 0f)
                    {
                        remainingSec = 0f;
                    }

                    return new OtogeTypeTransition(currentType, change.Type, remainingSec);
                }

                currentType = change.Type;
            }

            return new OtogeTypeTransition(currentType, currentType, 0f);
        }
    }
}
