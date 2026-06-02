namespace MyProject.Core
{
    public readonly struct OtogeTypeTransition
    {
        public OtogeType CurrentType { get; }
        public OtogeType NextType { get; }
        public float RemainingBeat { get; }
        public float RemainingSec { get; }
        public float DurationSec { get; }

        public OtogeTypeTransition(OtogeType currentType, OtogeType nextType, float remainingBeat, float remainingSec, float durationSec = 0f)
        {
            CurrentType = currentType;
            NextType = nextType;
            RemainingBeat = remainingBeat;
            RemainingSec = remainingSec;
            DurationSec = durationSec;
        }
    }
}
