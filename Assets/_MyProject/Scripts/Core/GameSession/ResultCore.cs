namespace MyProject.Core
{
    public class ResultCore
    {
        public BeatmapType BeatmapType { get; }
        public int Score { get; }

        public ResultCore(BeatmapType beatmapType, int score)
        {
            BeatmapType = beatmapType;
            Score = score;
        }
    }
}
