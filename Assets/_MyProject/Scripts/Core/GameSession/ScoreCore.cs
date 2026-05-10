using R3;

namespace MyProject.Core
{
    public class ScoreCore
    {
        public ReadOnlyReactiveProperty<int> Value => value;
        readonly ReactiveProperty<int> value = new(0);

        public void Initialize()
        {
            value.Value = 0;
        }

        public void Add(int amount)
        {
            value.Value += amount;
        }
    }
}
