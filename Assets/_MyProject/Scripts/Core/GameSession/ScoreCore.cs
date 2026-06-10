using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
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

        public int MaxCombo => maxCombo;
        public int HighScore => highScore;

        readonly Dictionary<int, List<NoteCoreBase>> remainingLaneNoteCores = new();
        readonly Dictionary<int, int> lanePressCounts = new();
        readonly List<NoteCoreBase> remainingAirNoteCores = new();
        readonly List<NoteCoreBase> autoPlayNoteCores = new();
        readonly List<NoteCoreBase> afterJudgeNoteCores = new();
        readonly HashSet<NoteCoreBase> countedNoteCores = new();
        readonly ISaveDataRepository saveDataRepository;

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
        int noteCount;
        int maxCombo;
        // スコア最大値 = ベース最大値 + コンボ数 (PerfectCriticalだと追加で1点入るため)
        int maxScore;
        int highScore;
        BeatmapType beatmapType;
        ScoreSaveDataCore saveData;

        public ScoreCore(ISaveDataRepository saveDataRepository)
        {
            this.saveDataRepository = saveDataRepository;
        }

        public async UniTask InitializeAsync(IReadOnlyList<NoteCoreBase> noteCores, BeatmapType newBeatmapType, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            beatmapType = newBeatmapType;
            saveData = await saveDataRepository.LoadScoreAsync(ct) ?? new ScoreSaveDataCore(0, 0);
            highScore = GetHighScore(saveData, beatmapType);

            JudgeCounts.Clear();
            remainingLaneNoteCores.Clear();
            lanePressCounts.Clear();
            remainingAirNoteCores.Clear();
            autoPlayNoteCores.Clear();
            afterJudgeNoteCores.Clear();
            countedNoteCores.Clear();

            foreach (var noteCore in noteCores)
            {
                if (noteCore.State.CurrentValue is not NoteState.BeforeJudge || noteCore.Judge.CurrentValue is not JudgeType.None)
                {
                    throw new InvalidOperationException("All NoteCores must be in the initial state");
                }

                autoPlayNoteCores.Add(noteCore);

                // Airノーツは専用のリストで管理する
                if (noteCore.Property.NoteType == NoteType.Air)
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
            autoPlayNoteCores.Sort(CompareAutoPlayOrder);

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
            noteCount = noteCores.Count;
            maxCombo = 0;
            maxScore = BaseMaxScore + noteCount;
        }

        public async UniTask SaveHighScoreAsync(CancellationToken ct)
        {
            var currentScore = score.CurrentValue;
            var nextSaveData = beatmapType switch
            {
                BeatmapType.Normal => new ScoreSaveDataCore(Math.Max(saveData.NormalHighScore, currentScore), saveData.HardHighScore),
                BeatmapType.Hard => new ScoreSaveDataCore(saveData.NormalHighScore, Math.Max(saveData.HardHighScore, currentScore)),
                _ => throw new ArgumentOutOfRangeException(nameof(beatmapType), beatmapType, "This beatmap type is not saved as score data."),
            };

            saveData = nextSaveData;
            highScore = GetHighScore(saveData, beatmapType);
            await saveDataRepository.SaveScoreAsync(nextSaveData, ct);
        }

        static int GetHighScore(ScoreSaveDataCore scoreSaveData, BeatmapType beatmapType)
        {
            return beatmapType switch
            {
                BeatmapType.Normal => scoreSaveData.NormalHighScore,
                BeatmapType.Hard => scoreSaveData.HardHighScore,
                BeatmapType.Demo => 0,
                _ => throw new ArgumentOutOfRangeException(nameof(beatmapType), beatmapType, "Unknown beatmap type."),
            };
        }

        public void JudgePressLane(int lane, float currentSec)
        {
            IncrementLanePressCount(lane);

            // 指定されたレーンの最も近いノーツを取得
            if (!remainingLaneNoteCores.TryGetValue(lane, out var remainingNoteCores) || remainingNoteCores.Count == 0)
            {
                return;
            }
            var noteCore = GetLeadingInputJudgeTarget(remainingNoteCores);
            if (noteCore == null)
            {
                return;
            }

            // ノーツをジャッジ
            var beforeState = noteCore.State.CurrentValue;
            noteCore.JudgePress(currentSec);
            HandleJudgeCount(noteCore, beforeState);
            HandleAfterJudge(noteCore);
        }

        public void JudgeReleaseLane(int lane, float currentSec)
        {
            DecrementLanePressCount(lane);

            // 指定されたレーンの最も近いノーツを取得
            if (!remainingLaneNoteCores.TryGetValue(lane, out var remainingNoteCores) || remainingNoteCores.Count == 0)
            {
                return;
            }
            var noteCore = GetLeadingInputJudgeTarget(remainingNoteCores);
            if (noteCore == null)
            {
                return;
            }

            // ノーツをジャッジ
            var beforeState = noteCore.State.CurrentValue;
            noteCore.JudgeRelease(currentSec);
            HandleJudgeCount(noteCore, beforeState);
            HandleAfterJudge(noteCore);
        }

        public void JudgePressAir(float currentSec)
        {
            var noteCores = GetLeadingAirNoteCoresWithSameSec();
            if (noteCores.Count == 0)
            {
                return;
            }

            foreach (var noteCore in noteCores)
            {
                var beforeState = noteCore.State.CurrentValue;
                noteCore.JudgePress(currentSec);
                HandleJudgeCount(noteCore, beforeState);
            }
            HandleAfterJudges(noteCores);
        }

        public void JudgeReleaseAir(float currentSec)
        {
            var noteCores = GetLeadingAirNoteCoresWithSameSec();
            if (noteCores.Count == 0)
            {
                return;
            }

            foreach (var noteCore in noteCores)
            {
                var beforeState = noteCore.State.CurrentValue;
                noteCore.JudgeRelease(currentSec);
                HandleJudgeCount(noteCore, beforeState);
            }
            HandleAfterJudges(noteCores);
        }

        public void Update(float currentSec, bool isAutoPlay = false)
        {
            if (isAutoPlay)
            {
                JudgeAutoPlay(currentSec);
            }

            JudgePass(currentSec);
            JudgeMiss(currentSec);
        }

        void JudgeAutoPlay(float currentSec)
        {
            foreach (var noteCore in autoPlayNoteCores)
            {
                if (!noteCore.IsBeginPass(currentSec))
                {
                    break;
                }
                if (noteCore.State.CurrentValue is not NoteState.BeforeJudge)
                {
                    continue;
                }

                var judgeSec = noteCore.Property.TimingBegin.Sec;
                var beforeState = noteCore.State.CurrentValue;
                if (noteCore is HoldTickCore)
                {
                    noteCore.JudgeBeginPass(judgeSec);
                }
                else
                {
                    noteCore.JudgePress(judgeSec);
                }
                HandleJudgeCount(noteCore, beforeState);
                HandleAfterJudge(noteCore);
            }
        }

        static int CompareAutoPlayOrder(NoteCoreBase x, NoteCoreBase y)
        {
            var secCompare = x.Property.TimingBegin.Sec.CompareTo(y.Property.TimingBegin.Sec);
            return secCompare != 0 ? secCompare : GetAutoPlayPriority(x).CompareTo(GetAutoPlayPriority(y));
        }

        static int GetAutoPlayPriority(NoteCoreBase noteCore)
        {
            return noteCore is HoldTickCore ? 1 : 0;
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
                    var beforeState = noteCore.State.CurrentValue;
                    if (noteCore is HoldTickCore holdTickCore)
                    {
                        if (!IsAnyCoveredLanePressed(holdTickCore))
                        {
                            continue;
                        }
                        holdTickCore.JudgeBeginPass(currentSec);
                    }
                    else
                    {
                        noteCore.JudgeBeginPass(currentSec);
                    }
                    HandleJudgeCount(noteCore, beforeState);
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
                    var beforeState = noteCore.State.CurrentValue;
                    noteCore.JudgeEndPass(currentSec);
                    HandleJudgeCount(noteCore, beforeState);
                    if (noteCore.State.CurrentValue is NoteState.AfterJudge)
                    {
                        afterJudgeCandidates.Add(noteCore);
                    }
                }
                HandleAfterJudges(afterJudgeCandidates);
            }

            var airAfterJudgeCandidates = new List<NoteCoreBase>();
            foreach (var noteCore in remainingAirNoteCores)
            {
                if (!noteCore.IsBeginPass(currentSec))
                {
                    break;
                }
                var beforeState = noteCore.State.CurrentValue;
                noteCore.JudgeBeginPass(currentSec);
                HandleJudgeCount(noteCore, beforeState);
                if (noteCore.State.CurrentValue is NoteState.AfterJudge)
                {
                    airAfterJudgeCandidates.Add(noteCore);
                }
            }
            HandleAfterJudges(airAfterJudgeCandidates);
            airAfterJudgeCandidates.Clear();

            foreach (var noteCore in remainingAirNoteCores)
            {
                if (!noteCore.IsEndPass(currentSec))
                {
                    break;
                }
                var beforeState = noteCore.State.CurrentValue;
                noteCore.JudgeEndPass(currentSec);
                HandleJudgeCount(noteCore, beforeState);
                if (noteCore.State.CurrentValue is NoteState.AfterJudge)
                {
                    airAfterJudgeCandidates.Add(noteCore);
                }
            }
            HandleAfterJudges(airAfterJudgeCandidates);
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
                    var beforeState = noteCore.State.CurrentValue;
                    noteCore.JudgeBeginMiss(currentSec);
                    HandleJudgeCount(noteCore, beforeState);
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
                    var beforeState = noteCore.State.CurrentValue;
                    noteCore.JudgeEndMiss(currentSec);
                    HandleJudgeCount(noteCore, beforeState);
                    if (noteCore.State.CurrentValue is NoteState.AfterJudge)
                    {
                        afterJudgeCandidates.Add(noteCore);
                    }
                }
                HandleAfterJudges(afterJudgeCandidates);
            }

            var airAfterJudgeCandidates = new List<NoteCoreBase>();
            foreach (var noteCore in remainingAirNoteCores)
            {
                if (!noteCore.IsBeginMiss(currentSec))
                {
                    break;
                }
                var beforeState = noteCore.State.CurrentValue;
                noteCore.JudgeBeginMiss(currentSec);
                HandleJudgeCount(noteCore, beforeState);
                if (noteCore.State.CurrentValue is NoteState.AfterJudge)
                {
                    airAfterJudgeCandidates.Add(noteCore);
                }
            }
            HandleAfterJudges(airAfterJudgeCandidates);
            airAfterJudgeCandidates.Clear();

            foreach (var noteCore in remainingAirNoteCores)
            {
                if (!noteCore.IsEndMiss(currentSec))
                {
                    break;
                }
                var beforeState = noteCore.State.CurrentValue;
                noteCore.JudgeEndMiss(currentSec);
                HandleJudgeCount(noteCore, beforeState);
                if (noteCore.State.CurrentValue is NoteState.AfterJudge)
                {
                    airAfterJudgeCandidates.Add(noteCore);
                }
            }
            HandleAfterJudges(airAfterJudgeCandidates);
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

        static NoteCoreBase GetLeadingInputJudgeTarget(IReadOnlyList<NoteCoreBase> noteCores)
        {
            foreach (var noteCore in noteCores)
            {
                if (noteCore.Property.NoteType is not NoteType.HoldTick)
                {
                    return noteCore;
                }
            }

            return null;
        }

        bool IsAnyCoveredLanePressed(NoteCoreBase noteCore)
        {
            foreach (var lane in GetCoveredLanes(noteCore))
            {
                if (IsLanePressed(lane))
                {
                    return true;
                }
            }

            return false;
        }

        bool IsLanePressed(int lane)
        {
            return lanePressCounts.TryGetValue(lane, out var count) && count > 0;
        }

        void IncrementLanePressCount(int lane)
        {
            lanePressCounts.TryGetValue(lane, out var currentCount);
            lanePressCounts[lane] = currentCount + 1;
        }

        void DecrementLanePressCount(int lane)
        {
            if (!lanePressCounts.TryGetValue(lane, out var currentCount))
            {
                return;
            }

            if (currentCount <= 1)
            {
                lanePressCounts.Remove(lane);
                return;
            }

            lanePressCounts[lane] = currentCount - 1;
        }

        List<NoteCoreBase> GetLeadingAirNoteCoresWithSameSec()
        {
            var noteCores = new List<NoteCoreBase>();
            if (remainingAirNoteCores.Count == 0)
            {
                return noteCores;
            }

            var leadingSec = remainingAirNoteCores[0].Property.TimingBegin.Sec;
            foreach (var noteCore in remainingAirNoteCores)
            {
                if (!Mathf.Approximately(noteCore.Property.TimingBegin.Sec, leadingSec))
                {
                    break;
                }
                noteCores.Add(noteCore);
            }
            return noteCores;
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
                remainingAirNoteCores.Remove(noteCore);
                foreach (var lane in GetCoveredLanes(noteCore))
                {
                    if (remainingLaneNoteCores.TryGetValue(lane, out var remainingNoteCores))
                    {
                        remainingNoteCores.Remove(noteCore);
                    }
                }
                afterJudgeNoteCores.Add(noteCore);
            }
        }

        void HandleJudgeCount(NoteCoreBase noteCore, NoteState beforeState)
        {
            if (beforeState is not NoteState.BeforeJudge || noteCore.State.CurrentValue is NoteState.BeforeJudge)
            {
                return;
            }
            if (!countedNoteCores.Add(noteCore))
            {
                return;
            }

            var judge = noteCore.Judge.CurrentValue;
            if (judge is JudgeType.None)
            {
                throw new InvalidOperationException("Judge must be set when leaving BeforeJudge.");
            }

            JudgeCounts[judge]++;
            AddScore(judge);
            UpdateCombo(judge);

            Debug.Log($"Judge: {judge}, Score: {score.Value}, Combo: {combo.Value}");
        }

        void AddScore(JudgeType judge)
        {
            if (!judgeTypeToBaseScoreRate.TryGetValue(judge, out var baseScoreRate))
            {
                throw new InvalidOperationException($"Unknown JudgeType: {judge}");
            }

            // 小数点以下切り捨て
            int baseScore = (int)(baseScoreRate * BaseMaxScore / noteCount);
            int bonusScore = judge is JudgeType.PerfectCriticalFast or JudgeType.PerfectCriticalLate ? 1 : 0;
            int totalScore = baseScore + bonusScore;
            score.Value = Math.Min(score.Value + totalScore, maxScore);

            int perfectCriticalCount = JudgeCounts[JudgeType.PerfectCriticalFast] + JudgeCounts[JudgeType.PerfectCriticalLate];
            int perfectCount = perfectCriticalCount + JudgeCounts[JudgeType.PerfectFast] + JudgeCounts[JudgeType.PerfectLate];
            if (perfectCount == noteCount)
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
                maxCombo = Math.Max(maxCombo, combo.Value);
            }
        }
    }
}
