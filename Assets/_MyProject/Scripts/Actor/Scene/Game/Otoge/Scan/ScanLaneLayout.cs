using UnityEngine;

namespace MyProject.Actor
{
    internal static class ScanLaneLayout
    {
        public const int CenterLane = 2;
        public const float MinY = -2.5f;
        public const float MaxY = 2.5f;
        public const float RoundTripBeats = 4f;
        public const float LaneStepX = 2f;

        public static bool IsCenterLane(int lane)
        {
            return lane == CenterLane;
        }

        public static float GetLaneCenterX(int lane, int width)
        {
            var laneCenter = lane + ((width - 1) * 0.5f);
            return laneCenter * LaneStepX;
        }

        public static float GetJudgeLineY(float beat)
        {
            var halfTripBeats = RoundTripBeats * 0.5f;
            var phase = Mathf.Repeat(beat, RoundTripBeats);
            var t = phase < halfTripBeats
                ? phase / halfTripBeats
                : 1f - ((phase - halfTripBeats) / halfTripBeats);

            return Mathf.Lerp(MinY, MaxY, t);
        }
    }
}
