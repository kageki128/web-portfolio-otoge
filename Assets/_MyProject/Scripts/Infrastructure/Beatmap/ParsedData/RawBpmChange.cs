namespace MyProject.Infrastructure
{
    public sealed class RawBpmChange
    {
        public int Measure { get; }
        public int Tick { get; }
        public float Bpm { get; }

        public RawBpmChange(int measure, int tick, float bpm)
        {
            Measure = measure;
            Tick = tick;
            Bpm = bpm;
        }
    }
}
