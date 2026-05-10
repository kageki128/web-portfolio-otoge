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
        [SerializeField] TMP_Text text;

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
            text.text =
                $"PerfectCritical(F/L): {judgeCounts[JudgeType.PerfectCriticalFast]} / {judgeCounts[JudgeType.PerfectCriticalLate]}\n" +
                $"Perfect(F/L): {judgeCounts[JudgeType.PerfectFast]} / {judgeCounts[JudgeType.PerfectLate]}\n" +
                $"Good(F/L): {judgeCounts[JudgeType.GoodFast]} / {judgeCounts[JudgeType.GoodLate]}\n" +
                $"Miss(F/L): {judgeCounts[JudgeType.MissFast]} / {judgeCounts[JudgeType.MissLate]}";
        }
    }
}
