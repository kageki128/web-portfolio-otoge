namespace MyProject.Core
{
    public class AirCore : NoteCoreBase
    {
        public AirCore(NoteProperty property) : base(property) { }

        public override void JudgePress(float currentSec)
        {
            var deltaSec = currentSec - Property.TimingBegin.Sec;
            var judge = GetJudgeType(deltaSec);
            // 早ミスは無視
            if (judge is JudgeType.MissFast)
            {
                return;
            }
            SetJudge(judge);
            state.Value = NoteState.AfterJudge;
        }

        public override void JudgeRelease(float currentSec)
        {
            return;
        }

        public override void JudgeBeginPass(float currentSec)
        {
            return;
        }

        public override void JudgeEndPass(float currentSec)
        {
            return;
        }

        public override void JudgeBeginMiss(float currentSec)
        {
            if (!IsBeginMiss(currentSec))
            {
                return;
            }

            SetJudge(JudgeType.MissLate);
            state.Value = NoteState.AfterJudge;
        }

        public override void JudgeEndMiss(float currentSec)
        {
            return;
        }
    }
}
