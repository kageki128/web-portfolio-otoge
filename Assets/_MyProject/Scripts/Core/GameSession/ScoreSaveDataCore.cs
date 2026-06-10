namespace MyProject.Core
{
    public class ScoreSaveDataCore
    {
        public int NormalHighScore { get; }
        public int HardHighScore { get; }

        public ScoreSaveDataCore(int normalHighScore, int hardHighScore)
        {
            NormalHighScore = normalHighScore;
            HardHighScore = hardHighScore;
        }
    }
}
