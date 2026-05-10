namespace MyProject.Infrastructure
{
    public sealed class RawHighSpeedChange
    {
        public int Timeline { get; }
        public int Measure { get; }
        public int Tick { get; }
        public float HighSpeed { get; }

        public RawHighSpeedChange(int timeline, int measure, int tick, float highSpeed)
        {
            Timeline = timeline;
            Measure = measure;
            Tick = tick;
            HighSpeed = highSpeed;
        }
    }
}
