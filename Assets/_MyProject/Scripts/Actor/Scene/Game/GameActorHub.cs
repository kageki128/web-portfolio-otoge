using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

namespace MyProject.Actor
{
    [RequireComponent(typeof(ActorAnimationTimeline))]
    public class GameActorHub : SceneActorHubBase
    {
        public Observable<Unit> ToSelectButtonClicked => toSelectButton.Clicked;

        [SerializeField] StandardButtonActor toSelectButton;

        ActorAnimationTimeline animationTimeline;
        GameActionsObserver gameActionsObserver;

        [Inject]
        public void Construct(GameActionsObserver gameActionsObserver)
        {
            this.gameActionsObserver = gameActionsObserver;
        }

        public override void Initialize()
        {
            animationTimeline = GetComponent<ActorAnimationTimeline>();

            gameActionsObserver.Disable();
            animationTimeline.Initialize();
            gameObject.SetActive(false);
        }

        public override async UniTask InitialShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await animationTimeline.InitialShowAsync(ct);
            gameActionsObserver.Enable();
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await animationTimeline.ShowAsync(ct);
            gameActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            gameActionsObserver.Disable();
            await animationTimeline.HideAsync(ct);
            gameObject.SetActive(false);
        }
    }
}
