using System;
using UnityEngine;

namespace MyProject.Core
{
    public abstract class NoteCoreBase
    {
        public NoteProperty Property { get; }
        public NoteState State { get; protected set; } = NoteState.BeforeJudge;
        // Noneから一度だけ変更できる
        public JudgeType Judge
        {
            get => judge;
            protected set
            {
                if (judge is not JudgeType.None)
                {
                    throw new InvalidOperationException($"Judge can only be set once.");
                }
                if (value is JudgeType.None)
                {
                    throw new InvalidOperationException("Judge must not be None.");
                }
                judge = value;
            }
        }
        JudgeType judge = JudgeType.None;

        const float PerfectCriticalWidthSec = 0.033f;
        const float PerfectWidthSec = 0.066f;
        const float GoodWidthSec = 0.100f;

        public NoteCoreBase(NoteProperty property)
        {
            Property = property;
        }

        public abstract void JudgePress(float currentSec);
        public abstract void JudgeRelease(float currentSec);
        public abstract void JudgeBeginPass(float currentSec);
        public abstract void JudgeEndPass(float currentSec);
        public abstract void JudgeBeginMiss(float currentSec);
        public abstract void JudgeEndMiss(float currentSec);

        public bool IsBeginPass(float currentSec)
        {
            return currentSec >= Property.TimingBegin.Sec;
        }
        public bool IsEndPass(float currentSec)
        {
            return currentSec >= Property.TimingEnd.Sec;
        }
        public bool IsBeginMiss(float currentSec)
        {
            return currentSec >= Property.TimingBegin.Sec + GoodWidthSec;
        }
        public bool IsEndMiss(float currentSec)
        {
            return currentSec >= Property.TimingEnd.Sec + GoodWidthSec;
        }

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
