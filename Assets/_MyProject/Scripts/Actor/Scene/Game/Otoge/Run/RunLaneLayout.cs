namespace MyProject.Actor
{
    internal static class RunLaneLayout
    {
        const float LaneYStep = 2f;

        public static float GetLaneY(int lane)
        {
            return lane * LaneYStep;
        }
    }
}
