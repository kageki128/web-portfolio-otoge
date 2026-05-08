using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace MyProject.Actor
{
    [RequireComponent(typeof(ActorAnimationTimeline))]
    public class TitleActorHub : SceneActorHubBase
    {
        ActorAnimationTimeline animationTimeline;
        TitleActionsObserver titleActionsObserver;

        [Inject]
        public void Construct(TitleActionsObserver titleActionsObserver)
        {
            this.titleActionsObserver = titleActionsObserver;
        }

        public override void Initialize()
        {
            animationTimeline = GetComponent<ActorAnimationTimeline>();

            titleActionsObserver.Disable();
            animationTimeline.Initialize();
            gameObject.SetActive(false);
        }

        public override async UniTask InitialShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await animationTimeline.InitialShowAsync(ct);
            titleActionsObserver.Enable();
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await animationTimeline.ShowAsync(ct);
            titleActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            titleActionsObserver.Disable();
            await animationTimeline.HideAsync(ct);
            gameObject.SetActive(false);
        }
    }
}
