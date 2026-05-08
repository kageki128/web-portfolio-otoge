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

        readonly GameActorHub gameActorHub;

        readonly CompositeDisposable disposables = new();

        public GameSceneDirector(GameActorHub gameActorHub)
        {
            this.gameActorHub = gameActorHub;
        }

        public async UniTask InitializeAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            gameActorHub.Initialize();
            await UniTask.CompletedTask;
        }

        public async UniTask BeforeEnterAsync(CancellationToken ct)
        {
            await UniTask.CompletedTask;
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
            gameActorHub.ToSelectButtonClicked
                .Take(1)
                .Subscribe(_ => sceneChangeRequest.OnNext(SceneType.Select))
                .AddTo(disposables);
        }
    }
}
