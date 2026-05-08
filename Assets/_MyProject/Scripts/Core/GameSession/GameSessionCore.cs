using System;
using R3;

namespace MyProject.Core
{
    public class GameSessionCore
    {
        public ReadOnlyReactiveProperty<GameState> State => state;
        readonly ReactiveProperty<GameState> state = new(GameState.Ready);

        public ReadOnlyReactiveProperty<int> Score => scoreCore.Value;
        readonly ScoreCore scoreCore = new();

        public void Reset()
        {
            SetState(GameState.Ready);
            scoreCore.Reset();
        }

        public void Start()
        {
            SetState(GameState.Playing);
        }

        public void Pause()
        {
            SetState(GameState.Paused);
        }

        public void Resume()
        {
            SetState(GameState.Playing);
        }

        public void Finish()
        {
            if (state.Value is not GameState.Playing)
            {
                throw new InvalidOperationException("Cannot finish unless the game is playing.");
            }

            SetState(GameState.Finished);
        }

        public void AddScore(int amount)
        {
            if (state.Value is not GameState.Playing)
            {
                throw new InvalidOperationException("Cannot add score unless the game is playing.");
            }

            scoreCore.Add(amount);
        }

        void SetState(GameState next)
        {
            state.Value = next;
        }
    }
}
