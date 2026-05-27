namespace MyProject.Core
{
    public class MeasureLineCore : NoteCoreBase
    {
        public MeasureLineCore(NoteProperty property) : base(property)
        {
            state.Value = NoteState.None;
        }

        public override void JudgePress(float currentSec)
        {
            return;
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
            return;
        }

        public override void JudgeEndMiss(float currentSec)
        {
            return;
        }
    }
}
