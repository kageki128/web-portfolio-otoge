namespace MyProject.Core
{
    public class HoldCore : NoteCoreBase
    {
        public HoldCore(NoteProperty property) : base(property) { }

        public override void JudgePress(float deltaSec)
        {
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

        public override void JudgeRelease(float deltaSec)
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

        public override void JudgeBeginPass()
        {
            return;
        }

        public override void JudgeEndPass()
        {
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

        public override void JudgeBeginMiss()
        {
            Judge = JudgeType.MissLate;
            State = NoteState.Missed;
        }

        public override void JudgeEndMiss()
        {
            Judge = JudgeType.MissLate;
            State = NoteState.AfterJudge;
        }
    }
}
