namespace MyProject.Core
{
    public readonly struct OtogeTypeTransition
    {
        public OtogeType CurrentType { get; }
        public OtogeType NextType { get; }
        public float RemainingSecToNextChange { get; }

        public OtogeTypeTransition(OtogeType currentType, OtogeType nextType, float remainingSecToNextChange)
        {
            CurrentType = currentType;
            NextType = nextType;
            RemainingSecToNextChange = remainingSecToNextChange;
        }
    }
}
