using System;
using System.Collections.Generic;
using ObservableCollections;
using R3;
using UnityEngine;

namespace MyProject.Core
{
    public class ScoreCore
    {
        public ReadOnlyReactiveProperty<int> Score => score;
        readonly ReactiveProperty<int> score = new(0);

        public ReadOnlyReactiveProperty<int> Combo => combo;
        readonly ReactiveProperty<int> combo = new(0);

        public ObservableDictionary<JudgeType, int> JudgeCounts { get; } = new();

        readonly Dictionary<int, List<NoteCoreBase>> remainingLaneNoteCores = new();
        readonly List<NoteCoreBase> remainingAirNoteCores = new();
        readonly List<NoteCoreBase> afterJudgeNoteCores = new();

        readonly Dictionary<JudgeType, float> judgeTypeToBaseScoreRate = new()
        {
            { JudgeType.PerfectCriticalFast, 1f },
            { JudgeType.PerfectCriticalLate, 1f },
            { JudgeType.PerfectFast, 1f },
            { JudgeType.PerfectLate, 1f },
            { JudgeType.GoodFast, 0.5f },
            { JudgeType.GoodLate, 0.5f },
            { JudgeType.MissFast, 0f },
            { JudgeType.MissLate, 0f },
        };

        const int BaseMaxScore = 1000000;
        int maxCombo;
        // スコア最大値 = ベース最大値 + コンボ数 (PerfectCriticalだと追加で1点入るため)
        int maxScore;

        public void Initialize(IReadOnlyList<NoteCoreBase> noteCores)
        {
            JudgeCounts.Clear();
            remainingLaneNoteCores.Clear();
            remainingAirNoteCores.Clear();
            afterJudgeNoteCores.Clear();

            foreach (var noteCore in noteCores)
            {
                if (noteCore.State.CurrentValue is not NoteState.BeforeJudge || noteCore.Judge.CurrentValue is not JudgeType.None)
                {
                    throw new InvalidOperationException("All NoteCores must be in the initial state");
                }

                // Airノーツは専用のリストで管理する
                if (noteCore.Property.Type == NoteType.Air)
                {
                    remainingAirNoteCores.Add(noteCore);
                    continue;
                }

                // 通常ノーツはレーンと幅を考慮して管理する
                foreach (var lane in GetCoveredLanes(noteCore))
                {
                    if (!remainingLaneNoteCores.ContainsKey(lane))
                    {
                        remainingLaneNoteCores[lane] = new List<NoteCoreBase>();
                    }
                    remainingLaneNoteCores[lane].Add(noteCore);
                }
            }
            // BeginBeat順にソートする
            foreach (var kvp in remainingLaneNoteCores)
            {
                kvp.Value.Sort((x, y) => x.Property.TimingBegin.Beat.CompareTo(y.Property.TimingBegin.Beat));
            }
            remainingAirNoteCores.Sort((x, y) => x.Property.TimingBegin.Beat.CompareTo(y.Property.TimingBegin.Beat));

            // ジャッジカウントを初期化
            JudgeCounts[JudgeType.PerfectCriticalFast] = 0;
            JudgeCounts[JudgeType.PerfectCriticalLate] = 0;
            JudgeCounts[JudgeType.PerfectFast] = 0;
            JudgeCounts[JudgeType.PerfectLate] = 0;
            JudgeCounts[JudgeType.GoodFast] = 0;
            JudgeCounts[JudgeType.GoodLate] = 0;
            JudgeCounts[JudgeType.MissFast] = 0;
            JudgeCounts[JudgeType.MissLate] = 0;

            score.Value = 0;
            combo.Value = 0;
            maxCombo = noteCores.Count;
            maxScore = BaseMaxScore + maxCombo;
        }

        public void JudgePressLane(int lane, float currentSec)
        {
            // 指定されたレーンの最も近いノーツを取得
            if (!remainingLaneNoteCores.TryGetValue(lane, out var remainingNoteCores) || remainingNoteCores.Count == 0)
            {
                return;
            }
            var noteCore = remainingNoteCores[0];

            // ノーツをジャッジ
            noteCore.JudgePress(currentSec);
            HandleAfterJudge(noteCore);
        }

        public void JudgeReleaseLane(int lane, float currentSec)
        {
            // 指定されたレーンの最も近いノーツを取得
            if (!remainingLaneNoteCores.TryGetValue(lane, out var remainingNoteCores) || remainingNoteCores.Count == 0)
            {
                return;
            }
            var noteCore = remainingNoteCores[0];

            // ノーツをジャッジ
            noteCore.JudgeRelease(currentSec);
            HandleAfterJudge(noteCore);
        }

        public void JudgePressAir(float currentSec)
        {
            if (remainingAirNoteCores.Count == 0)
             {
                 return;
             }
             var noteCore = remainingAirNoteCores[0];

             // ノーツをジャッジ
             noteCore.JudgePress(currentSec);
             HandleAfterJudge(noteCore);
        }

        public void JudgeReleaseAir(float currentSec)
        {
            if (remainingAirNoteCores.Count == 0)
             {
                 return;
             }
             var noteCore = remainingAirNoteCores[0];

             // ノーツをジャッジ
             noteCore.JudgeRelease(currentSec);
             HandleAfterJudge(noteCore);
        }

        public void Update(float currentSec)
        {
            JudgePass(currentSec);
            JudgeMiss(currentSec);
        }

        void JudgePass(float currentSec)
        {
            foreach (var kvp in remainingLaneNoteCores)
            {
                var remainingNoteCores = kvp.Value;
                var afterJudgeCandidates = new List<NoteCoreBase>();
                // Begin
                foreach (var noteCore in remainingNoteCores)
                {
                    // 判定ラインを過ぎているノーツをジャッジ
                    if (!noteCore.IsBeginPass(currentSec))
                    {
                        break;
                    }
                    noteCore.JudgeBeginPass(currentSec);
                    if (noteCore.State.CurrentValue is NoteState.AfterJudge)
                    {
                        afterJudgeCandidates.Add(noteCore);
                    }
                }
                HandleAfterJudges(afterJudgeCandidates);
                afterJudgeCandidates.Clear();

                // End
                foreach (var noteCore in remainingNoteCores)
                {
                    // 判定ラインを過ぎているノーツをジャッジ
                    if (!noteCore.IsEndPass(currentSec))
                    {
                        break;
                    }
                    noteCore.JudgeEndPass(currentSec);
                    if (noteCore.State.CurrentValue is NoteState.AfterJudge)
                    {
                        afterJudgeCandidates.Add(noteCore);
                    }
                }
                HandleAfterJudges(afterJudgeCandidates);
            }
        }

        void JudgeMiss(float currentSec)
        {
            foreach (var kvp in remainingLaneNoteCores)
            {
                var remainingNoteCores = kvp.Value;
                var afterJudgeCandidates = new List<NoteCoreBase>();
                // Begin
                foreach (var noteCore in remainingNoteCores)
                {
                    // 判定幅を過ぎているノーツをジャッジ
                    if (!noteCore.IsBeginMiss(currentSec))
                    {
                        break;
                    }
                    noteCore.JudgeBeginMiss(currentSec);
                    if (noteCore.State.CurrentValue is NoteState.AfterJudge)
                    {
                        afterJudgeCandidates.Add(noteCore);
                    }
                }
                HandleAfterJudges(afterJudgeCandidates);
                afterJudgeCandidates.Clear();

                // End
                foreach (var noteCore in remainingNoteCores)
                {
                    // 判定幅を過ぎているノーツをジャッジ
                    if (!noteCore.IsEndMiss(currentSec))
                    {
                        break;
                    }
                    noteCore.JudgeEndMiss(currentSec);
                    if (noteCore.State.CurrentValue is NoteState.AfterJudge)
                    {
                        afterJudgeCandidates.Add(noteCore);
                    }
                }
                HandleAfterJudges(afterJudgeCandidates);
            }
        }

        static IReadOnlyList<int> GetCoveredLanes(NoteCoreBase noteCore)
        {
            int startLane = noteCore.Property.Lane;
            int endLane = startLane + noteCore.Property.Width;
            var coveredLanes = new List<int>();
            for (int lane = startLane; lane < endLane; lane++)
            {
                coveredLanes.Add(lane);
            }
            return coveredLanes;
        }

        void HandleAfterJudges(List<NoteCoreBase> noteCores)
        {
            foreach (var noteCore in noteCores)
            {
                HandleAfterJudge(noteCore);
            }
        }

        void HandleAfterJudge(NoteCoreBase noteCore)
        {
            if (noteCore.State.CurrentValue is NoteState.AfterJudge)
            {
                foreach (var lane in GetCoveredLanes(noteCore))
                {
                    if (remainingLaneNoteCores.TryGetValue(lane, out var remainingNoteCores))
                    {
                        remainingNoteCores.Remove(noteCore);
                    }
                }
                afterJudgeNoteCores.Add(noteCore);
                var judge = noteCore.Judge.CurrentValue;
                JudgeCounts[judge]++;
                AddScore(judge);
                UpdateCombo(judge);

                Debug.Log($"Judge: {judge}, Score: {score.Value}, Combo: {combo.Value}");
            }
        }

        void AddScore(JudgeType judge)
        {
            if (!judgeTypeToBaseScoreRate.TryGetValue(judge, out var baseScoreRate))
            {
                throw new InvalidOperationException($"Unknown JudgeType: {judge}");
            }

            // 小数点以下切り捨て
            int baseScore = (int)(baseScoreRate * BaseMaxScore / maxCombo);
            int bonusScore = judge is JudgeType.PerfectCriticalFast or JudgeType.PerfectCriticalLate ? 1 : 0;
            int totalScore = baseScore + bonusScore;
            score.Value = Math.Min(score.Value + totalScore, maxScore);

            int perfectCriticalCount = JudgeCounts[JudgeType.PerfectCriticalFast] + JudgeCounts[JudgeType.PerfectCriticalLate];
            int perfectCount = perfectCriticalCount + JudgeCounts[JudgeType.PerfectFast] + JudgeCounts[JudgeType.PerfectLate];
            if (perfectCount == maxCombo)
            {
                score.Value = BaseMaxScore + perfectCriticalCount;
            }
        }

        void UpdateCombo(JudgeType judge)
        {
            if (judge is JudgeType.MissFast or JudgeType.MissLate)
            {
                combo.Value = 0;
            }
            else
            {
                combo.Value++;
            }
        }
    }
}
