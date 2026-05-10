namespace MyProject.Core
{
    public class HoldCore : NoteCoreBase
    {
        public HoldCore(NoteProperty property) : base(property) { }

        public override void JudgePress(float currentSec)
        {
            var deltaSec = currentSec - Property.TimingBegin.Sec;
            switch (state.Value)
            {
                case NoteState.BeforeJudge:
                    var judge = GetJudgeType(deltaSec);
                    // 早ミスは無視
                    if (judge is JudgeType.MissFast)
                    {
                        return;
                    }
                    SetJudge(judge);
                    state.Value = NoteState.Holding;
                    return;
                case NoteState.Missed:
                    state.Value = NoteState.Holding;
                    return;
                case NoteState.Released:
                    state.Value = NoteState.Holding;
                    return;
                default:
                    return;
            }
        }

        public override void JudgeRelease(float currentSec)
        {
            switch (state.Value)
            {
                case NoteState.Holding:
                    state.Value = NoteState.Released;
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

            switch (state.Value)
            {
                case NoteState.Holding:
                case NoteState.Released:
                    state.Value = NoteState.AfterJudge;
                    return;
                default:
                    return;
            }
        }

        public override void JudgeBeginMiss(float currentSec)
        {
            if (state.Value is not NoteState.BeforeJudge || !IsBeginMiss(currentSec))
            {
                return;
            }

            SetJudge(JudgeType.MissLate);
            state.Value = NoteState.Missed;
        }

        public override void JudgeEndMiss(float currentSec)
        {
            if (!IsEndMiss(currentSec))
            {
                return;
            }

            switch (state.Value)
            {
                case NoteState.Missed:
                case NoteState.Holding:
                case NoteState.Released:
                    state.Value = NoteState.AfterJudge;
                    return;
                default:
                    return;
            }
        }
    }
}
