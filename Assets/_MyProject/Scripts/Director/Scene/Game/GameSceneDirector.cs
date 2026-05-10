using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Actor;
using MyProject.Core;
using R3;

namespace MyProject.Director
{
    public class GameSceneDirector : ISceneDirector, IDisposable
    {
        public Observable<SceneType> SceneChangeRequest => sceneChangeRequest;
        readonly Subject<SceneType> sceneChangeRequest = new();

        public Observable<Unit> SceneReloadRequest => sceneReloadRequest;
        readonly Subject<Unit> sceneReloadRequest = new();

        readonly GameSessionCore gameSessionCore;
        readonly GameActorHub gameActorHub;

        readonly CompositeDisposable disposables = new();

        public GameSceneDirector(GameSessionCore gameSessionCore, GameActorHub gameActorHub)
        {
            this.gameSessionCore = gameSessionCore;
            this.gameActorHub = gameActorHub;
        }

        public async UniTask InitializeAsync(CancellationToken ct)
        {
            gameActorHub.Initialize();
            await gameSessionCore.InitializeAsync(ct);
        }

        public async UniTask BeforeEnterAsync(CancellationToken ct)
        {
            await gameSessionCore.InitializeAsync(ct);
        }

        public async UniTask InitialEnterAsync(CancellationToken ct)
        {
            await gameActorHub.InitialShowAsync(ct);
            HandleEnter();
        }

        public async UniTask EnterAsync(CancellationToken ct)
        {
            await gameActorHub.ShowAsync(ct);
            HandleEnter();
        }

        public void Tick()
        {
        }

        public async UniTask BeforeExitAsync(CancellationToken ct)
        {
            disposables.Clear();
            await UniTask.CompletedTask;
        }

        public async UniTask ExitAsync(CancellationToken ct)
        {
            await gameActorHub.HideAsync(ct);
        }

        public void Dispose()
        {
            disposables.Dispose();
            sceneChangeRequest.Dispose();
            sceneReloadRequest.Dispose();
        }

        void HandleEnter()
        {
            disposables.Clear();

            var noteCores = gameSessionCore.NoteCores;
            var timelineToCurrentScroll = gameSessionCore.TimelineToCurrentScroll;
            gameActorHub.CreateNotes(noteCores);
            foreach (var kvp in timelineToCurrentScroll)
            {
                int timeline = kvp.Key;
                var currentScroll = kvp.Value;
                currentScroll.Subscribe(scroll => gameActorHub.UpdateNotesByTimeline(timeline, scroll, 5f)).AddTo(disposables);
            }

            gameActorHub.ToSelectButtonClicked
                .Take(1)
                .Subscribe(_ => sceneChangeRequest.OnNext(SceneType.Select))
                .AddTo(disposables);
        }
    }
}
