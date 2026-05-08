using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace MyProject.Actor
{
    [RequireComponent(typeof(ActorAnimationTimeline))]
    public class ResultActorHub : SceneActorHubBase
    {
        ActorAnimationTimeline animationTimeline;
        ResultActionsObserver resultActionsObserver;

        [Inject]
        public void Construct(ResultActionsObserver resultActionsObserver)
        {
            this.resultActionsObserver = resultActionsObserver;
        }

        public override void Initialize()
        {
            animationTimeline = GetComponent<ActorAnimationTimeline>();

            resultActionsObserver.Disable();
            animationTimeline.Initialize();
            gameObject.SetActive(false);
        }

        public override async UniTask InitialShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await animationTimeline.InitialShowAsync(ct);
            resultActionsObserver.Enable();
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await animationTimeline.ShowAsync(ct);
            resultActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            resultActionsObserver.Disable();
            await animationTimeline.HideAsync(ct);
            gameObject.SetActive(false);
        }
    }
}
