using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using MyProject.Core;

namespace MyProject.Actor
{
    public abstract class OtogeSharedActorBase : ActorBase
    {
        protected const float StateTransitionDuration = 0.5f;
        protected const Ease StateTransitionEase = Ease.OutCubic;

        // OtogeTypeに応じたTransformや長さなどの切り替えを行う
        public abstract void SetState(OtogeType otogeType);
        public abstract UniTask SetStateAsync(OtogeType otogeType, CancellationToken cancellationToken);
    }
}
