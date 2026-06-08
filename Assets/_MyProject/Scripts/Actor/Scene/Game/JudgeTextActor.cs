using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using TMPro;
using UnityEngine;

namespace MyProject.Actor
{
    [RequireComponent(typeof(StandardTransitionAnimator))]
    public class JudgeTextActor : ActorBase
    {
        [SerializeField] TMP_Text perfectText;
        [SerializeField] TMP_Text goodText;
        [SerializeField] TMP_Text missText;
        [SerializeField] TMP_Text fastText;
        [SerializeField] TMP_Text lateText;

        StandardTransitionAnimator transitionAnimator;

        public override void Initialize()
        {
            transitionAnimator = GetComponent<StandardTransitionAnimator>();
            transitionAnimator.Initialize();

            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await transitionAnimator.ShowAsync(ct);
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            await transitionAnimator.HideAsync(ct);
            gameObject.SetActive(false);
        }

        public void SetJudgeCounts(IReadOnlyDictionary<JudgeType, int> judgeCounts)
        {
            var perfectCount = judgeCounts[JudgeType.PerfectCriticalFast] + judgeCounts[JudgeType.PerfectCriticalLate] + judgeCounts[JudgeType.PerfectFast] + judgeCounts[JudgeType.PerfectLate];
            var goodCount = judgeCounts[JudgeType.GoodFast] + judgeCounts[JudgeType.GoodLate];
            var missCount = judgeCounts[JudgeType.MissFast] + judgeCounts[JudgeType.MissLate];
            var fastCount = judgeCounts[JudgeType.PerfectFast] + judgeCounts[JudgeType.GoodFast];
            var lateCount = judgeCounts[JudgeType.PerfectLate] + judgeCounts[JudgeType.GoodLate];

            SetJudgeCountText(perfectText, perfectCount);
            SetJudgeCountText(goodText, goodCount);
            SetJudgeCountText(missText, missCount);
            SetJudgeCountText(fastText, fastCount);
            SetJudgeCountText(lateText, lateCount);
        }

        void SetJudgeCountText(TMP_Text valueText, int count)
        {
            var isShow = count != 0;
            valueText.gameObject.SetActive(isShow);

            if (!isShow)
            {
                return;
            }

            valueText.text = $"{count}";
        }
    }
}
