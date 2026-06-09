using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using TMPro;
using UnityEngine;

namespace MyProject.Actor
{
    public class ResultTextActor : ActorBase
    {
        [Header("Texts")]
        [SerializeField] TMP_Text difficulty;
        [SerializeField] TMP_Text newRecord;
        [SerializeField] TMP_Text score;
        [SerializeField] TMP_Text bestScore;
        [SerializeField] TMP_Text rank;
        [SerializeField] TMP_Text perfect;
        [SerializeField] TMP_Text good;
        [SerializeField] TMP_Text miss;
        [SerializeField] TMP_Text perfectFast;
        [SerializeField] TMP_Text perfectLate;
        [SerializeField] TMP_Text goodFast;
        [SerializeField] TMP_Text goodLate;
        [SerializeField] TMP_Text maxCombo;

        [Header("Difficulty Colors")]
        [SerializeField] Color normalDifficultyColor = Color.white;
        [SerializeField] Color hardDifficultyColor = Color.red;

        public override void Initialize()
        {
            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await UniTask.CompletedTask;
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            gameObject.SetActive(false);
            await UniTask.CompletedTask;
        }

        public void SetResult(BeatmapType beatmapType, int scoreValue, IReadOnlyDictionary<JudgeType, int> judgeCounts, int maxComboValue)
        {
            difficulty.text = ToDifficultyText(beatmapType);
            difficulty.color = beatmapType is BeatmapType.Hard ? hardDifficultyColor : normalDifficultyColor;

            score.text = $"{scoreValue:D7}";
            rank.text = ToRankText(scoreValue);

            perfect.text = $"{GetPerfectCount(judgeCounts)}";
            good.text = $"{judgeCounts[JudgeType.GoodFast] + judgeCounts[JudgeType.GoodLate]}";
            miss.text = $"{judgeCounts[JudgeType.MissFast] + judgeCounts[JudgeType.MissLate]}";
            perfectFast.text = $"{judgeCounts[JudgeType.PerfectCriticalFast] + judgeCounts[JudgeType.PerfectFast]}";
            perfectLate.text = $"{judgeCounts[JudgeType.PerfectCriticalLate] + judgeCounts[JudgeType.PerfectLate]}";
            goodFast.text = $"{judgeCounts[JudgeType.GoodFast]}";
            goodLate.text = $"{judgeCounts[JudgeType.GoodLate]}";
            maxCombo.text = $"{maxComboValue}";
        }

        static int GetPerfectCount(IReadOnlyDictionary<JudgeType, int> judgeCounts)
        {
            return judgeCounts[JudgeType.PerfectCriticalFast]
                + judgeCounts[JudgeType.PerfectCriticalLate]
                + judgeCounts[JudgeType.PerfectFast]
                + judgeCounts[JudgeType.PerfectLate];
        }

        static string ToDifficultyText(BeatmapType beatmapType)
        {
            return beatmapType switch
            {
                BeatmapType.Demo => "DEMO",
                BeatmapType.Normal => "NORMAL",
                BeatmapType.Hard => "HARD",
                _ => throw new System.ArgumentOutOfRangeException(nameof(beatmapType), beatmapType, null)
            };
        }

        static string ToRankText(int scoreValue)
        {
            return scoreValue switch
            {
                >= 990000 => "SSS",
                >= 980000 => "SS",
                >= 950000 => "S",
                >= 920000 => "A",
                >= 890000 => "B",
                >= 860000 => "C",
                _ => "D"
            };
        }
    }
}
