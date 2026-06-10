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
        [SerializeField] ResultInputKeysActor inputKeysActor;

        ActorAnimationTimeline animationTimeline;
        ResultActionsObserver resultActionsObserver;
        readonly CompositeDisposable disposables = new();

        [Inject]
        public void Construct(ResultActionsObserver resultActionsObserver)
        {
            this.resultActionsObserver = resultActionsObserver;
        }

        public override void Initialize()
        {
            animationTimeline = GetComponent<ActorAnimationTimeline>();

            disposables.Clear();
            resultActionsObserver.Disable();
            animationTimeline.Initialize();
            resultActionsObserver.QuitKeyPressed
                .Subscribe(_ => inputKeysActor.LightUpQuitKey())
                .AddTo(disposables);
            resultActionsObserver.QuitKeyReleased
                .Subscribe(_ => inputKeysActor.LightDownQuitKey())
                .AddTo(disposables);
            resultActionsObserver.RetryKeyPressed
                .Subscribe(_ => inputKeysActor.LightUpRetryKey())
                .AddTo(disposables);
            resultActionsObserver.RetryKeyReleased
                .Subscribe(_ => inputKeysActor.LightDownRetryKey())
                .AddTo(disposables);

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

        public void SetResult(BeatmapType beatmapType, int score, int highScore, IReadOnlyDictionary<JudgeType, int> judgeCounts, int maxCombo, int noteCount)
        {
            resultTextActor.SetResult(beatmapType, score, highScore, judgeCounts, maxCombo, noteCount);
        }

        void OnDestroy()
        {
            disposables.Dispose();
        }
    }
}
