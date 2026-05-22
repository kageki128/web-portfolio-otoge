namespace MyProject.Core
{
    public class NoteProperty
    {
        public NoteType NoteType { get; }
        public OtogeType OtogeType { get; }
        public int Timeline { get; }

        public NoteTiming TimingBegin { get; }
        public NoteTiming TimingEnd { get; }

        public float ScrollBegin { get; }
        public float ScrollEnd { get; }

        public int Lane { get; }
        public int Width { get; }
        public int Layer { get; }

        public NoteProperty
        (
            NoteType noteType,
            OtogeType otogeType,
            int timeline,
            NoteTiming timingBegin,
            NoteTiming timingEnd,
            float scrollBegin,
            float scrollEnd,
            int lane,
            int width,
            int layer
        )
        {
            NoteType = noteType;
            OtogeType = otogeType;
            Timeline = timeline;
            TimingBegin = timingBegin;
            TimingEnd = timingEnd;
            ScrollBegin = scrollBegin;
            ScrollEnd = scrollEnd;
            Lane = lane;
            Width = width;
            Layer = layer;
        }
    }
}
