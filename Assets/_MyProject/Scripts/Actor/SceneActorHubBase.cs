using System.Threading;
using Cysharp.Threading.Tasks;

namespace MyProject.Actor
{
    public abstract class SceneActorHubBase : ActorBase
    {
        public abstract override void Initialize();
        public abstract override UniTask InitialShowAsync(CancellationToken ct);
        public abstract override UniTask ShowAsync(CancellationToken ct);
        public abstract override UniTask HideAsync(CancellationToken ct);
    }
}
