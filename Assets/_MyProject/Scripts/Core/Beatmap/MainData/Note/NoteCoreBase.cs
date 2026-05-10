using UnityEngine;

namespace MyProject.Core
{
    public abstract class NoteCoreBase
    {
        public NoteProperty Property { get; }
        public NoteState State { get; protected set; } = NoteState.BeforeJudge;
        public JudgeType Judge { get; protected set; } = JudgeType.None;

        const float PerfectCriticalWidthSec = 0.033f;
        const float PerfectWidthSec = 0.066f;
        const float GoodWidthSec = 0.100f;

        public NoteCoreBase(NoteProperty property)
        {
            Property = property;
        }

        public abstract void JudgePress(float deltaSec);
        public abstract void JudgeRelease(float deltaSec);
        public abstract void JudgeBeginPass();
        public abstract void JudgeEndPass();
        public abstract void JudgeBeginMiss();
        public abstract void JudgeEndMiss();

        protected JudgeType GetJudgeType(float deltaSec)
        {
            float absDeltaSec = Mathf.Abs(deltaSec);
            if (absDeltaSec <= PerfectCriticalWidthSec)
            {
                return deltaSec < 0 ? JudgeType.PerfectCriticalFast : JudgeType.PerfectCriticalLate;
            }
            else if (absDeltaSec <= PerfectWidthSec)
            {
                return deltaSec < 0 ? JudgeType.PerfectFast : JudgeType.PerfectLate;
            }
            else if (absDeltaSec <= GoodWidthSec)
            {
                return deltaSec < 0 ? JudgeType.GoodFast : JudgeType.GoodLate;
            }
            else
            {
                return deltaSec < 0 ? JudgeType.MissFast : JudgeType.MissLate;
            }
        }
    }
}
