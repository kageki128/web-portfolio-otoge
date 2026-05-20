namespace MyProject.Core
{
    public class OtogeChange
    {
        public float Beat { get; }
        public OtogeType Type { get; }

        public OtogeChange(float beat, OtogeType type)
        {
            Beat = beat;
            Type = type;
        }
    }
}
