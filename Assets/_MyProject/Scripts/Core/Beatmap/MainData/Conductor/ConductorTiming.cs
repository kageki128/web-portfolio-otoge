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

        public ReadOnlyReactiveProperty<OtogeType> CurrentOtogeType => currentOtogeType;
        readonly ReactiveProperty<OtogeType> currentOtogeType = new(OtogeType.Tetra);

        readonly IReadOnlyList<BpmChange> bpmChanges;
        readonly IReadOnlyDictionary<int, IReadOnlyList<HighSpeedChange>> timelineToHighSpeedChanges;
        readonly IReadOnlyList<MeasureLengthChange> measureLengthChanges;
        readonly IReadOnlyList<OtogeChange> otogeChanges;

        public ConductorTiming
        (
            IReadOnlyList<BpmChange> bpmChanges,
            IReadOnlyDictionary<int, IReadOnlyList<HighSpeedChange>> timelineToHighSpeedChanges,
            IReadOnlyList<MeasureLengthChange> measureLengthChanges,
            IReadOnlyList<OtogeChange> otogeChanges
        )
        {
            this.bpmChanges = bpmChanges;
            this.timelineToHighSpeedChanges = timelineToHighSpeedChanges;
            this.measureLengthChanges = measureLengthChanges;
            this.otogeChanges = otogeChanges.OrderBy(change => change.Beat).ToArray();

            var readOnlyTimelineToCurrentScroll = new Dictionary<int, ReadOnlyReactiveProperty<float>>();
            foreach (var timelineToHighSpeedChange in timelineToHighSpeedChanges)
            {
                var currentScroll = new ReactiveProperty<float>(0);
                timelineToCurrentScroll[timelineToHighSpeedChange.Key] = currentScroll;
                readOnlyTimelineToCurrentScroll[timelineToHighSpeedChange.Key] = currentScroll;
            }
            TimelineToCurrentScroll = readOnlyTimelineToCurrentScroll;
            currentOtogeType.Value = ResolveCurrentOtogeType(0f);
        }

        public void SetTimeBySec(float sec)
        {
            var newSec = sec;
            var newBeat = CalculateBeatFromSec(newSec, bpmChanges);
            var newMeasure = CalculateMeasureFromBeat(newBeat, measureLengthChanges);

            currentSec.Value = newSec;
            currentBeat.Value = newBeat;
            currentMeasure.Value = newMeasure;
            currentOtogeType.Value = ResolveCurrentOtogeType(newBeat);

            foreach (var timelineToHighSpeedChange in timelineToHighSpeedChanges)
            {
                timelineToCurrentScroll[timelineToHighSpeedChange.Key].Value = CalculateScrollFromBeat(newBeat, bpmChanges, timelineToHighSpeedChange.Value);
            }
        }

        OtogeType ResolveCurrentOtogeType(float beat)
        {
            var type = OtogeType.Tetra;
            foreach (var change in otogeChanges)
            {
                if (change.Beat > beat)
                {
                    break;
                }

                type = change.Type;
            }

            return type;
        }
    }
}
