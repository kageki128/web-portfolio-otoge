using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Actor;
using MyProject.Core;
using R3;

namespace MyProject.Director
{
    public class ResultSceneDirector : ISceneDirector, IDisposable
    {
        public Observable<SceneType> SceneChangeRequest => sceneChangeRequest;
        readonly Subject<SceneType> sceneChangeRequest = new();

        public Observable<Unit> SceneReloadRequest => sceneReloadRequest;
        readonly Subject<Unit> sceneReloadRequest = new();

        readonly GameSessionCore gameSessionCore;
        readonly ResultActorHub resultActorHub;

        readonly CompositeDisposable disposables = new();

        public ResultSceneDirector(GameSessionCore gameSessionCore, ResultActorHub resultActorHub)
        {
            this.gameSessionCore = gameSessionCore;
            this.resultActorHub = resultActorHub;
        }

        public async UniTask InitializeAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            resultActorHub.Initialize();
            await UniTask.CompletedTask;
        }

        public async UniTask BeforeEnterAsync(CancellationToken ct)
        {
            resultActorHub.SetResult(
                gameSessionCore.BeatmapType,
                gameSessionCore.Score.CurrentValue,
                gameSessionCore.JudgeCounts,
                gameSessionCore.MaxCombo
            );
            await UniTask.CompletedTask;
        }

        public async UniTask EnterAsync(CancellationToken ct)
        {
            await resultActorHub.ShowAsync(ct);
        }

        public async UniTask AfterEnterAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            HandleEnter();
            await UniTask.CompletedTask;
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
            await resultActorHub.HideAsync(ct);
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
            resultActorHub.Quit
                .Take(1)
                .Subscribe(_ => sceneChangeRequest.OnNext(SceneType.Select))
                .AddTo(disposables);
            resultActorHub.Retry
                .Take(1)
                .Subscribe(_ => sceneChangeRequest.OnNext(SceneType.Game))
                .AddTo(disposables);
        }
    }
}
