namespace MyProject.Core
{
    public class BpmChange
    {
        public float Bpm { get; }
        public float Beat { get; }

        public BpmChange(float bpm, float beat)
        {
            Bpm = bpm;
            Beat = beat;
        }
    }
}
