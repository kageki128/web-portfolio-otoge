using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ObservableCollections;
using R3;

namespace MyProject.Core
{
    public class ScoreCore
    {
        public ReadOnlyReactiveProperty<int> Value => value;
        readonly ReactiveProperty<int> value = new(0);

        public ObservableDictionary<JudgeType, int> JudgeTypeToCount { get; } = new();

        readonly Dictionary<int, List<NoteCoreBase>> laneToRemainingNoteCores = new();

        const int BaseMaxScore = 1000000;

        public void Initialize(IReadOnlyList<NoteCoreBase> noteCores)
        {
            JudgeTypeToCount.Clear();
            laneToRemainingNoteCores.Clear();

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

        public void Judge(NoteCoreBase noteCore, float deltaSec)
        {

        }
    }
}
