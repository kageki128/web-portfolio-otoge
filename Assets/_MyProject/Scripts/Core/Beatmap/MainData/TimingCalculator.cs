using System.Collections.Generic;
using UnityEngine;

namespace MyProject.Core
{
    public static class TimingCalculator
    {
        public static float BeatToSecWithFixedBpm(float beat, float bpm)
        {
            return beat * 60f / bpm;
        }

        public static float SecToBeatWithFixedBpm(float sec, float bpm)
        {
            return sec * bpm / 60f;
        }

        public static float CalculateSecFromBeat(float beat, IReadOnlyList<BpmChange> bpmChanges)
        {
            if (beat < 0f)
            {
                return BeatToSecWithFixedBpm(beat, bpmChanges[0].Bpm);
            }

            var accumulatedSec = 0f;
            var currentBpm = bpmChanges[0].Bpm;
            var previousBeat = 0f;

            for (var i = 1; i < bpmChanges.Count; i++)
            {
                var changeBeat = bpmChanges[i].Beat;
                if (changeBeat > beat)
                {
                    break;
                }

                accumulatedSec += BeatToSecWithFixedBpm(changeBeat - previousBeat, currentBpm);
                previousBeat = changeBeat;
                currentBpm = bpmChanges[i].Bpm;
            }

            accumulatedSec += BeatToSecWithFixedBpm(beat - previousBeat, currentBpm);
            return accumulatedSec;
        }

        public static float CalculateBeatFromSec(float sec, IReadOnlyList<BpmChange> bpmChanges)
        {
            if (sec < 0f)
            {
                return SecToBeatWithFixedBpm(sec, bpmChanges[0].Bpm);
            }

            var accumulatedSec = 0f;
            var currentBpm = bpmChanges[0].Bpm;
            var previousBeat = 0f;

            for (var i = 1; i < bpmChanges.Count; i++)
            {
                var changeBeat = bpmChanges[i].Beat;
                var segmentSec = BeatToSecWithFixedBpm(changeBeat - previousBeat, currentBpm);
                if (accumulatedSec + segmentSec > sec)
                {
                    break;
                }

                accumulatedSec += segmentSec;
                previousBeat = changeBeat;
                currentBpm = bpmChanges[i].Bpm;
            }

            return previousBeat + SecToBeatWithFixedBpm(sec - accumulatedSec, currentBpm);
        }

        public static float CalculateScrollFromBeat
        (
            float beat,
            IReadOnlyList<BpmChange> bpmChanges,
            IReadOnlyList<HighSpeedChange> highSpeedChanges
        )
        {
            if (beat < 0f)
            {
                return CalculateSecFromBeat(beat, bpmChanges) * highSpeedChanges[0].HighSpeed;
            }

            var eventBeats = new List<float>
            {
                0f,
                beat
            };
            for (var i = 1; i < bpmChanges.Count; i++)
            {
                if (bpmChanges[i].Beat < beat)
                {
                    eventBeats.Add(bpmChanges[i].Beat);
                }
            }
            for (var i = 1; i < highSpeedChanges.Count; i++)
            {
                if (highSpeedChanges[i].Beat < beat)
                {
                    eventBeats.Add(highSpeedChanges[i].Beat);
                }
            }

            eventBeats.Sort();

            var accumulatedScroll = 0f;
            var bpmIndex = 0;
            var highSpeedIndex = 0;

            for (var i = 0; i < eventBeats.Count - 1; i++)
            {
                var segmentStartBeat = eventBeats[i];
                var segmentEndBeat = eventBeats[i + 1];

                while (bpmIndex + 1 < bpmChanges.Count && bpmChanges[bpmIndex + 1].Beat <= segmentStartBeat)
                {
                    bpmIndex++;
                }

                while (highSpeedIndex + 1 < highSpeedChanges.Count && highSpeedChanges[highSpeedIndex + 1].Beat <= segmentStartBeat)
                {
                    highSpeedIndex++;
                }

                var segmentSec = BeatToSecWithFixedBpm(segmentEndBeat - segmentStartBeat, bpmChanges[bpmIndex].Bpm);
                accumulatedScroll += segmentSec * highSpeedChanges[highSpeedIndex].HighSpeed;
            }

            return accumulatedScroll;
        }

        public static int CalculateMeasureFromBeat(float beat, IReadOnlyList<MeasureLengthChange> measureLengthChanges)
        {
            if (beat < 0f)
            {
                return (int)Mathf.Floor(beat / measureLengthChanges[0].Length);
            }

            var currentMeasure = 0;
            var currentMeasureStartBeat = 0f;
            var measureLengthChangeIndex = 0;
            var currentMeasureLength = measureLengthChanges[0].Length;

            while (true)
            {
                var nextMeasureStartBeat = currentMeasureStartBeat + currentMeasureLength;
                if (beat < nextMeasureStartBeat)
                {
                    return currentMeasure;
                }

                currentMeasure++;
                currentMeasureStartBeat = nextMeasureStartBeat;

                if (
                    measureLengthChangeIndex + 1 < measureLengthChanges.Count &&
                    measureLengthChanges[measureLengthChangeIndex + 1].Beat == currentMeasureStartBeat
                )
                {
                    measureLengthChangeIndex++;
                    currentMeasureLength = measureLengthChanges[measureLengthChangeIndex].Length;
                }
            }
        }
    }
}
