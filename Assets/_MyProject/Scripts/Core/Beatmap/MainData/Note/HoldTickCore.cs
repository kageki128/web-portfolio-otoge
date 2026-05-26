namespace MyProject.Core
{
    public class HoldTickCore : NoteCoreBase
    {
        public HoldCore ParentHoldCore { get; }

        public HoldTickCore(NoteProperty property, HoldCore parentHoldCore) : base(property)
        {
            ParentHoldCore = parentHoldCore;
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
            if (state.Value is not NoteState.BeforeJudge)
            {
                return;
            }
            if (!IsBeginPass(currentSec))
            {
                return;
            }
            if (IsBeginMiss(currentSec))
            {
                return;
            }
            if (ParentHoldCore.State.CurrentValue is not NoteState.Holding)
            {
                return;
            }

            SetJudge(JudgeType.PerfectCriticalLate);
            state.Value = NoteState.AfterJudge;
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
