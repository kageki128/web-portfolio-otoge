namespace MyProject.Core
{
    public class TapCore : NoteCoreBase
    {
        public TapCore(NoteProperty property) : base(property) { }

        public override void JudgePress(float currentSec)
        {
            var deltaSec = currentSec - Property.TimingBegin.Sec;
            var judge = GetJudgeType(deltaSec);
            // 早ミスは無視
            if (judge is JudgeType.MissFast)
            {
                return;
            }
            Judge = judge;
            State = NoteState.AfterJudge;
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

            Judge = JudgeType.MissLate;
            State = NoteState.AfterJudge;
        }

        public override void JudgeEndMiss(float currentSec)
        {
            return;
        }
    }
}
