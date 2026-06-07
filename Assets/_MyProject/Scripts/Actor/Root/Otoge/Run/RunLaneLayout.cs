namespace MyProject.Actor
{
    internal static class RunLaneLayout
    {
        public const float JudgeEffectRiseOffset = 0f;
        const float LaneYStep = 2f;

        public static float GetLaneY(int lane)
        {
            return lane * LaneYStep;
        }
    }
}
