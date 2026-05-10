namespace MyProject.Infrastructure
{
    public sealed class MeasureEntry
    {
        public int Measure { get; }
        public int Length { get; }
        public float BeatStart { get; }

        public MeasureEntry(int measure, int length, float beatStart)
        {
            Measure = measure;
            Length = length;
            BeatStart = beatStart;
        }
    }
}
