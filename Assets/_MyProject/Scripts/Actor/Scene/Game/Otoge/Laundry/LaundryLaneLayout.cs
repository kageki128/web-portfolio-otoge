using UnityEngine;

namespace MyProject.Actor
{
    internal static class LaundryLaneLayout
    {
        public const float JudgeRadius = 4.774f;
        public const float InnerRadius = 1f;
        public const float ScaleUpDistance = 1f;

        const float laneStartAngleDeg = 112.5f;
        const float laneStepAngleDeg = 45f;

        public static Vector2 GetDirection(int lane, int width)
        {
            var angleDeg = GetLaneAngleDeg(lane, width);
            var angleRad = angleDeg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        }

        public static float GetJudgeDistance()
        {
            return JudgeRadius;
        }

        public static float GetRawDistance(float scroll, float currentScroll, float scrollSpeed, float judgeDistance)
        {
            return judgeDistance - ((scroll - currentScroll) * scrollSpeed);
        }

        public static float GetScale(float rawDistance)
        {
            if (rawDistance < InnerRadius)
            {
                return 0f;
            }

            return Mathf.Clamp01((rawDistance - InnerRadius) / ScaleUpDistance);
        }

        public static float GetLaneAngleDeg(int lane, int width)
        {
            var laneCenter = lane + ((width - 1) * 0.5f);
            return laneStartAngleDeg + (laneStepAngleDeg * laneCenter);
        }
    }
}
