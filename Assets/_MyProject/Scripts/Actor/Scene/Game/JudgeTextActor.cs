using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using TMPro;
using UnityEngine;

namespace MyProject.Actor
{
    public class JudgeTextActor : ActorBase
    {
        [SerializeField] TMP_Text perfectText;
        [SerializeField] TMP_Text goodText;
        [SerializeField] TMP_Text missText;
        [SerializeField] TMP_Text fastText;
        [SerializeField] TMP_Text lateText;

        public override void Initialize()
        {
            gameObject.SetActive(false);
        }

        public override UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            return UniTask.CompletedTask;
        }

        public override UniTask HideAsync(CancellationToken ct)
        {
            gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }

        public void SetJudgeCounts(IReadOnlyDictionary<JudgeType, int> judgeCounts)
        {
            var perfectCount = judgeCounts[JudgeType.PerfectCriticalFast] + judgeCounts[JudgeType.PerfectCriticalLate] + judgeCounts[JudgeType.PerfectFast] + judgeCounts[JudgeType.PerfectLate];
            var goodCount = judgeCounts[JudgeType.GoodFast] + judgeCounts[JudgeType.GoodLate];
            var missCount = judgeCounts[JudgeType.MissFast] + judgeCounts[JudgeType.MissLate];
            var fastCount = judgeCounts[JudgeType.PerfectFast] + judgeCounts[JudgeType.GoodFast];
            var lateCount = judgeCounts[JudgeType.PerfectLate] + judgeCounts[JudgeType.GoodLate];

            perfectText.text = $"{perfectCount}";
            goodText.text = $"{goodCount}";
            missText.text = $"{missCount}";
            fastText.text = $"{fastCount}";
            lateText.text = $"{lateCount}";
        }
    }
}
