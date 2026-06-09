using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using R3;
using UnityEngine;
using VContainer;

namespace MyProject.Actor
{
    [RequireComponent(typeof(ActorAnimationTimeline))]
    public class ResultActorHub : SceneActorHubBase
    {
        public Observable<Unit> Quit => resultActionsObserver.Quit;
        public Observable<Unit> Retry => resultActionsObserver.Retry;

        [SerializeField] ResultTextActor resultTextActor;

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

        public void SetResult(BeatmapType beatmapType, int score, IReadOnlyDictionary<JudgeType, int> judgeCounts, int maxCombo)
        {
            resultTextActor.SetResult(beatmapType, score, judgeCounts, maxCombo);
        }
    }
}
