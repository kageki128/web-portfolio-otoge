namespace MyProject.Core
{
    public class HoldCore : NoteCoreBase
    {
        public HoldCore(NoteProperty property) : base(property) { }

        public override void JudgePress(float currentSec)
        {
            var deltaSec = currentSec - Property.TimingBegin.Sec;
            switch (State)
            {
                case NoteState.BeforeJudge:
                    var judge = GetJudgeType(deltaSec);
                    // 早ミスは無視
                    if (judge is JudgeType.MissFast)
                    {
                        return;
                    }
                    Judge = judge;
                    State = NoteState.Holding;
                    return;
                case NoteState.Missed:
                    State = NoteState.Holding;
                    return;
                case NoteState.Released:
                    State = NoteState.Holding;
                    return;
                default:
                    return;
            }
        }

        public override void JudgeRelease(float currentSec)
        {
            switch (State)
            {
                case NoteState.Holding:
                    State = NoteState.Released;
                    return;
                default:
                    return;
            }
        }

        public override void JudgeBeginPass(float currentSec)
        {
            return;
        }

        public override void JudgeEndPass(float currentSec)
        {
            if (!IsEndPass(currentSec))
            {
                return;
            }

            switch (State)
            {
                case NoteState.Holding:
                case NoteState.Released:
                    State = NoteState.AfterJudge;
                    return;
                default:
                    return;
            }
        }

        public override void JudgeBeginMiss(float currentSec)
        {
            if (State is not NoteState.BeforeJudge || !IsBeginMiss(currentSec))
            {
                return;
            }

            Judge = JudgeType.MissLate;
            State = NoteState.Missed;
        }

        public override void JudgeEndMiss(float currentSec)
        {
            if (!IsEndMiss(currentSec))
            {
                return;
            }

            switch (State)
            {
                case NoteState.Missed:
                case NoteState.Holding:
                case NoteState.Released:
                    State = NoteState.AfterJudge;
                    return;
                default:
                    return;
            }
        }
    }
}
