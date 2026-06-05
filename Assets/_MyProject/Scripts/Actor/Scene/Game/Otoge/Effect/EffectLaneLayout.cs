namespace MyProject.Actor
{
    internal static class EffectLaneLayout
    {
        public const int CenterLane = 2;
        public const float JudgeEffectRiseOffset = 1.6f;
        public const float CenterLaneX = 1.5f;
        public const int CenterWidthMultiplier = 4;
        const int SortingOrderStride = 10;

        public static bool IsCenterLane(int lane)
        {
            return lane == CenterLane;
        }

        public static int GetVisualWidth(int lane, int width)
        {
            return IsCenterLane(lane) ? width * CenterWidthMultiplier : width;
        }

        public static float GetVisualCenterX(int lane, int width)
        {
            if (IsCenterLane(lane))
            {
                return CenterLaneX;
            }

            var visualLane = lane > CenterLane ? lane - 1 : lane;
            return visualLane + ((width - 1) * 0.5f);
        }

        public static int GetSortingOrder(int layer, int lane)
        {
            return layer * SortingOrderStride + (IsCenterLane(lane) ? 0 : 1);
        }
    }
}
