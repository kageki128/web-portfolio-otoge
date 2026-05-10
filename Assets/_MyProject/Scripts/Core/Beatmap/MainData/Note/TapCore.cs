namespace MyProject.Core
{
    public class TapCore : NoteCoreBase
    {
        public TapCore(NoteProperty property) : base(property) { }

        public override void JudgePress(float deltaSec)
        {
            var judge = GetJudgeType(deltaSec);
            // 早ミスは無視
            if (judge is JudgeType.MissFast)
            {
                return;
            }
            Judge = judge;
            State = NoteState.AfterJudge;
        }

        public override void JudgeRelease(float deltaSec)
        {
            return;
        }

        public override void JudgeBeginPass()
        {
            return;
        }

        public override void JudgeEndPass()
        {
            return;
        }

        public override void JudgeBeginMiss()
        {
            Judge = JudgeType.MissLate;
            State = NoteState.AfterJudge;
        }

        public override void JudgeEndMiss()
        {
            return;
        }
    }
}
