using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

namespace MyProject.Core
{
    public class GameSessionCore
    {
        public ReadOnlyReactiveProperty<GameState> State => state;
        readonly ReactiveProperty<GameState> state = new(GameState.Preparing);

        public ReadOnlyReactiveProperty<int> Score => scoreCore.Value;
        readonly ScoreCore scoreCore = new();

        public IReadOnlyList<NoteCore> NoteCores
        {
            get
            {
                if (beatmapCore == null)
                {
                    throw new InvalidOperationException("Beatmap is not loaded yet.");
                }
                return beatmapCore.NoteCores;
            }
        }

        public IReadOnlyDictionary<int, ReadOnlyReactiveProperty<float>> TimelineToCurrentScroll
        {
            get
            {
                if (beatmapCore == null)
                {
                    throw new InvalidOperationException("Beatmap is not loaded yet.");
                }
                return beatmapCore.TimelineToCurrentScroll;
            }
        }

        BeatmapCore beatmapCore;

        readonly IBeatmapRepository beatmapRepository;

        public GameSessionCore(IBeatmapRepository beatmapRepository)
        {
            this.beatmapRepository = beatmapRepository;
        }

        public async UniTask InitializeAsync(CancellationToken ct)
        {
            state.Value = GameState.Preparing;

            beatmapCore = await beatmapRepository.GetAsync(ct);
            scoreCore.Initialize();

            state.Value = GameState.Ready;
        }

        public double StartGame()
        {
            if (state.Value is not GameState.Ready)
            {
                throw new InvalidOperationException($"Cannot start game from state {state.Value}");
            }

            double startDspTime = beatmapCore.Start(0.5);
            state.Value = GameState.Playing;

            return startDspTime;
        }

        public void ProceedGame()
        {
            if (state.Value is not GameState.Playing)
            {
                return;
            }

            beatmapCore.AdvanceTime();
        }
    }
}
