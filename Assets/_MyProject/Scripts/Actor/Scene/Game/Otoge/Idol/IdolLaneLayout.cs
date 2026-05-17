using UnityEngine;

namespace MyProject.Actor
{
    internal static class IdolLaneLayout
    {
        public const int CenterLane = 2;
        public const float InnerRadius = 5f;
        public const float ScaleUpDistance = 1f;

        static readonly Vector2 center = new(0f, 15.914481f);
        static readonly Vector2[] judgePoints =
        {
            new(-7.667306f, 1.968760f),
            new(-3.958017f, 0.500046f),
            new(0f, 0f),
            new(3.958017f, 0.500046f),
            new(7.667306f, 1.968760f)
        };

        public static Vector2 GetCenterPosition()
        {
            return center;
        }

        public static Vector2 GetDirection(int lane, int width)
        {
            var targetPoint = GetTargetPoint(lane, width);
            return (targetPoint - center).normalized;
        }

        public static float GetJudgeDistance(int lane, int width)
        {
            var targetPoint = GetTargetPoint(lane, width);
            return Vector2.Distance(center, targetPoint);
        }

        public static float GetLaneAngleDeg(int lane, int width)
        {
            var direction = GetDirection(lane, width);
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }

        static Vector2 GetTargetPoint(int lane, int width)
        {
            var laneCenter = lane + ((width - 1) * 0.5f);
            var clampedLaneCenter = Mathf.Clamp(laneCenter, 0f, judgePoints.Length - 1f);

            var startIndex = Mathf.FloorToInt(clampedLaneCenter);
            var endIndex = Mathf.Min(startIndex + 1, judgePoints.Length - 1);
            var t = clampedLaneCenter - startIndex;

            return Vector2.Lerp(judgePoints[startIndex], judgePoints[endIndex], t);
        }
    }
}
