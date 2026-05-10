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

        public BeatmapMetaData MetaData => beatmapCore.MetaData;
        public IReadOnlyList<NoteCoreBase> NoteCores => beatmapCore.NoteCores;

        public IReadOnlyDictionary<int, ReadOnlyReactiveProperty<float>> TimelineToCurrentScroll => beatmapCore.TimelineToCurrentScroll;

        // 初期化忘れに注意～(今は許容)
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
            scoreCore.Initialize(beatmapCore.NoteCores);

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
