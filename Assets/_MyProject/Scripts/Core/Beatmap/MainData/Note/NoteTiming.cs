using System.Collections.Generic;
using static MyProject.Core.TimingCalculator;

namespace MyProject.Core
{
    public class NoteTiming
    {
        public float Beat { get; }
        public float Sec { get; }
        public int Measure { get; }
        public IReadOnlyDictionary<int, float> TimelineToScroll => timelineToScroll;
        readonly Dictionary<int, float> timelineToScroll = new();

        public NoteTiming
        (
            float beat,
            IReadOnlyList<BpmChange> bpmChanges,
            IReadOnlyDictionary<int, IReadOnlyList<HighSpeedChange>> timelineToHighSpeedChanges,
            IReadOnlyList<MeasureLengthChange> measureLengthChanges
        )
        {
            Beat = beat;
            Sec = CalculateSecFromBeat(beat, bpmChanges);
            Measure = CalculateMeasureFromBeat(beat, measureLengthChanges);
            foreach (var highSpeedChanges in timelineToHighSpeedChanges)
            {
                timelineToScroll[highSpeedChanges.Key] = CalculateScrollFromBeat(beat, bpmChanges, highSpeedChanges.Value);
            }
        }
    }
}
