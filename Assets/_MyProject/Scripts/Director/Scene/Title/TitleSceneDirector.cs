using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Actor;
using MyProject.Core;
using R3;

namespace MyProject.Director
{
    public class TitleSceneDirector : ISceneDirector, IDisposable
    {
        public Observable<SceneType> SceneChangeRequest => sceneChangeRequest;
        readonly Subject<SceneType> sceneChangeRequest = new();

        public Observable<Unit> SceneReloadRequest => sceneReloadRequest;
        readonly Subject<Unit> sceneReloadRequest = new();

        readonly TitleActorHub titleActorHub;

        readonly CompositeDisposable disposables = new();

        public TitleSceneDirector(TitleActorHub titleActorHub)
        {
            this.titleActorHub = titleActorHub;
        }

        public async UniTask InitializeAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            titleActorHub.Initialize();
            await UniTask.CompletedTask;
        }

        public async UniTask BeforeEnterAsync(CancellationToken ct)
        {
            await UniTask.CompletedTask;
        }

        public async UniTask InitialEnterAsync(CancellationToken ct)
        {
            await titleActorHub.InitialShowAsync(ct);
            HandleEnter();
        }

        public async UniTask EnterAsync(CancellationToken ct)
        {
            await titleActorHub.ShowAsync(ct);
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
            await titleActorHub.HideAsync(ct);
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
        }
    }
}
