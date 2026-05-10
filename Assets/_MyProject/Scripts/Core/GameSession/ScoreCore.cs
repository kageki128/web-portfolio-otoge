using System;
using System.Collections.Generic;
using R3;

namespace MyProject.Core
{
    public class ScoreCore
    {
        public ReadOnlyReactiveProperty<int> Value => value;
        readonly ReactiveProperty<int> value = new(0);

        readonly Dictionary<int, List<NoteCoreBase>> laneToRemainingNoteCores = new();
        readonly List<NoteCoreBase> afterJudgeNoteCores = new();

        const int BaseMaxScore = 1000000;

        public void Initialize(IReadOnlyList<NoteCoreBase> noteCores)
        {
            laneToRemainingNoteCores.Clear();
            afterJudgeNoteCores.Clear();

            value.Value = 0;
            foreach (var noteCore in noteCores)
            {
                if (noteCore.State is not NoteState.BeforeJudge || noteCore.Judge is not JudgeType.None)
                {
                    throw new InvalidOperationException("All NoteCores must be in the initial state");
                }

                int lane = noteCore.Property.Lane;
                if (!laneToRemainingNoteCores.ContainsKey(lane))
                {
                    laneToRemainingNoteCores[lane] = new List<NoteCoreBase>();
                }
                laneToRemainingNoteCores[lane].Add(noteCore);
            }
            // BeginBeat順にソートする
            foreach (var kvp in laneToRemainingNoteCores)
            {
                kvp.Value.Sort((x, y) => x.Property.TimingBegin.Beat.CompareTo(y.Property.TimingBegin.Beat));
            }
        }

        public void JudgePress(int lane, float currentSec)
        {
            // 指定されたレーンの最も近いノーツを取得
            if (!laneToRemainingNoteCores.TryGetValue(lane, out var remainingNoteCores) || remainingNoteCores.Count == 0)
            {
                return;
            }
            var noteCore = remainingNoteCores[0];

            // ノーツをジャッジ
            noteCore.JudgePress(currentSec);
            HandleAfterJudge(noteCore, remainingNoteCores);
        }

        public void JudgeRelease(int lane, float currentSec)
        {
            // 指定されたレーンの最も近いノーツを取得
            if (!laneToRemainingNoteCores.TryGetValue(lane, out var remainingNoteCores) || remainingNoteCores.Count == 0)
            {
                return;
            }
            var noteCore = remainingNoteCores[0];

            // ノーツをジャッジ
            noteCore.JudgeRelease(currentSec);
            HandleAfterJudge(noteCore, remainingNoteCores);
        }

        public void Update(float currentSec)
        {
            JudgePass(currentSec);
            JudgeMiss(currentSec);
        }

        void JudgePass(float currentSec)
        {
            foreach (var kvp in laneToRemainingNoteCores)
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
                    if (noteCore.State is NoteState.AfterJudge)
                    {
                        afterJudgeCandidates.Add(noteCore);
                    }
                }
                HandleAfterJudges(afterJudgeCandidates, remainingNoteCores);
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
                    if (noteCore.State is NoteState.AfterJudge)
                    {
                        afterJudgeCandidates.Add(noteCore);
                    }
                }
                HandleAfterJudges(afterJudgeCandidates, remainingNoteCores);
            }
        }

        void JudgeMiss(float currentSec)
        {
            foreach (var kvp in laneToRemainingNoteCores)
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
                    if (noteCore.State is NoteState.AfterJudge)
                    {
                        afterJudgeCandidates.Add(noteCore);
                    }
                }
                HandleAfterJudges(afterJudgeCandidates, remainingNoteCores);
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
                    if (noteCore.State is NoteState.AfterJudge)
                    {
                        afterJudgeCandidates.Add(noteCore);
                    }
                }
                HandleAfterJudges(afterJudgeCandidates, remainingNoteCores);
            }
        }

        void HandleAfterJudges(List<NoteCoreBase> noteCores, List<NoteCoreBase> remainingNoteCores)
        {
            foreach (var noteCore in noteCores)
            {
                HandleAfterJudge(noteCore, remainingNoteCores);
            }
        }

        void HandleAfterJudge(NoteCoreBase noteCore, List<NoteCoreBase> remainingNoteCores)
        {
            if (noteCore.State is NoteState.AfterJudge)
            {
                remainingNoteCores.Remove(noteCore);
                afterJudgeNoteCores.Add(noteCore);
            }
        }
    }
}
