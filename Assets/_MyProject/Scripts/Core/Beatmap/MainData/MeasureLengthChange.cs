namespace MyProject.Core
{
    public class MeasureLengthChange
    {
        public int Length { get; }
        public float Beat { get; }

        public MeasureLengthChange(int length, float beat)
        {
            Length = length;
            Beat = beat;
        }
    }
}
