namespace MyProject.Infrastructure
{
    public sealed class RawNote
    {
        public int Timeline { get; }
        public int Measure { get; }
        public int Tick { get; }
        public char NoteType { get; }
        public char Lane { get; }
        public char Width { get; }
        public int Length { get; }
        public int LineNum { get; }

        public RawNote(int timeline, int measure, int tick, char noteType, char lane, char width, int length, int lineNum)
        {
            Timeline = timeline;
            Measure = measure;
            Tick = tick;
            NoteType = noteType;
            Lane = lane;
            Width = width;
            Length = length;
            LineNum = lineNum;
        }
    }
}
