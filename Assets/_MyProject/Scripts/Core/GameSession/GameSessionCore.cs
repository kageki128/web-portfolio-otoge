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
                if (beatmap == null)
                {
                    throw new InvalidOperationException("Beatmap is not loaded yet.");
                }
                return beatmap.MainData.NoteCores;
            }
        }

        public IReadOnlyDictionary<int, ReadOnlyReactiveProperty<float>> TimelineToCurrentScroll
        {
            get
            {
                if (beatmap == null)
                {
                    throw new InvalidOperationException("Beatmap is not loaded yet.");
                }
                return beatmap.MainData.ConductorCore.TimelineToCurrentScroll;
            }
        }

        Beatmap beatmap;

        readonly IBeatmapRepository beatmapRepository;

        public GameSessionCore(IBeatmapRepository beatmapRepository)
        {
            this.beatmapRepository = beatmapRepository;
        }

        public async UniTask InitializeAsync(CancellationToken ct)
        {
            state.Value = GameState.Preparing;

            beatmap = await beatmapRepository.GetAsync(ct);
            scoreCore.Initialize();

            state.Value = GameState.Ready;
        }
    }
}
