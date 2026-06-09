using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using R3;
using UnityEngine;
using VContainer;

namespace MyProject.Actor
{
    [RequireComponent(typeof(ActorAnimationTimeline))]
    public class SelectActorHub : SceneActorHubBase
    {
        public Observable<Unit> StartGame => selectActionsObserver.StartGame;
        public Observable<int> DifficultyScrolled => selectActionsObserver.DifficultyScrolled;

        [SerializeField] DifficultySelectActor difficultySelectActor;
        [SerializeField] InputKeysActor inputKeysActor;
        ActorAnimationTimeline animationTimeline;
        SelectActionsObserver selectActionsObserver;
        readonly CompositeDisposable disposables = new();

        [Inject]
        public void Construct(SelectActionsObserver selectActionsObserver)
        {
            this.selectActionsObserver = selectActionsObserver;
        }

        public override void Initialize()
        {
            animationTimeline = GetComponent<ActorAnimationTimeline>();

            disposables.Clear();
            selectActionsObserver.Disable();
            animationTimeline.Initialize();

            selectActionsObserver.DifficultyScrollStarted
                .Subscribe(LightUpDifficultyKey)
                .AddTo(disposables);
            selectActionsObserver.DifficultyScrollCanceled
                .Subscribe(_ => LightDownDifficultyKeys())
                .AddTo(disposables);
            selectActionsObserver.InputKeyPressed
                .Subscribe(inputKeysActor.LightUpKey)
                .AddTo(disposables);
            selectActionsObserver.InputKeyReleased
                .Subscribe(inputKeysActor.LightDownKey)
                .AddTo(disposables);

            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await animationTimeline.ShowAsync(ct);
            selectActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            selectActionsObserver.Disable();
            await animationTimeline.HideAsync(ct);
            gameObject.SetActive(false);
        }

        public void SetDifficulty(BeatmapType beatmapType)
        {
            difficultySelectActor.SetBeatmapType(beatmapType);
        }

        void LightUpDifficultyKey(int direction)
        {
            if (direction > 0)
            {
                difficultySelectActor.LightUpUpKey();
                return;
            }

            difficultySelectActor.LightUpDownKey();
        }

        void LightDownDifficultyKeys()
        {
            difficultySelectActor.LightDownUpKey();
            difficultySelectActor.LightDownDownKey();
        }

        void OnDestroy()
        {
            disposables.Dispose();
        }
    }
}
