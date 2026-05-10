namespace MyProject.Infrastructure
{
    public sealed class RawMeasureLengthChange
    {
        public int Measure { get; }
        public int Length { get; }
        public int LineNum { get; }

        public RawMeasureLengthChange(int measure, int length, int lineNum)
        {
            Measure = measure;
            Length = length;
            LineNum = lineNum;
        }
    }
}
