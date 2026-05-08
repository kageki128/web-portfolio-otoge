using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using R3;
using VContainer.Unity;

namespace MyProject.Director
{
    public class MainEntryPoint : IAsyncStartable, ITickable, IDisposable
    {
        readonly RootDirector rootDirector;
        readonly Dictionary<SceneType, ISceneDirector> sceneDirectors = new();

        readonly SemaphoreSlim sceneChangeSemaphore = new(1, 1);
        readonly CompositeDisposable disposables = new();
        readonly CancellationTokenSource cts = new();
        SceneType currentScene;

        public MainEntryPoint
        (
            GameConfigSO gameConfig,
            RootDirector rootDirector,
            TitleSceneDirector titleSceneDirector,
            SelectSceneDirector selectSceneDirector,
            GameSceneDirector gameSceneDirector,
            ResultSceneDirector resultSceneDirector
        )
        {
            this.rootDirector = rootDirector;
            currentScene = gameConfig.InitialSceneType;
            sceneDirectors.Add(SceneType.Title, titleSceneDirector);
            sceneDirectors.Add(SceneType.Select, selectSceneDirector);
            sceneDirectors.Add(SceneType.Game, gameSceneDirector);
            sceneDirectors.Add(SceneType.Result, resultSceneDirector);
        }

        public async UniTask StartAsync(CancellationToken ct)
        {
            await rootDirector.InitializeAsync(ct);
            await ResetSceneAsync(ct);
        }

        public void Tick()
        {
            rootDirector.Tick();

            var director = GetSceneDirector(currentScene);
            director.Tick();
        }

        public void Dispose()
        {
            disposables.Dispose();
            cts.Cancel();
            cts.Dispose();
            sceneChangeSemaphore.Dispose();
        }

        async UniTask ResetSceneAsync(CancellationToken ct)
        {
            disposables.Clear();

            foreach (var director in sceneDirectors.Values)
            {
                await director.InitializeAsync(ct);
            }

            // シーンチェンジリクエストを購読
            var sceneChangeRequests = sceneDirectors.Values
                .Select(director => director.SceneChangeRequest)
                .ToArray();
            Observable.Merge(sceneChangeRequests)
                .Subscribe(HandleSceneChangeRequest)
                .AddTo(disposables);

            var sceneReloadRequests = sceneDirectors.Values
                .Select(director => director.SceneReloadRequest)
                .ToArray();
            Observable.Merge(sceneReloadRequests)
                .Subscribe(_ => HandleSceneChangeRequest(currentScene))
                .AddTo(disposables);

            // 初期シーンを起動
            var initialSceneDirector = GetSceneDirector(currentScene);
            await initialSceneDirector.BeforeEnterAsync(ct);
            await initialSceneDirector.InitialEnterAsync(ct);
        }

        void HandleSceneChangeRequest(SceneType to)
        {
            if (!sceneChangeSemaphore.Wait(0))
            {
                throw new InvalidOperationException("Scene change was requested while another scene change is in progress.");
            }

            var from = currentScene;
            currentScene = to;
            ExecuteSceneTransitionAsync(from, to).Forget();
        }

        async UniTask ExecuteSceneTransitionAsync(SceneType from, SceneType to)
        {
            var fromDirector = GetSceneDirector(from);
            var toDirector = GetSceneDirector(to);

            try
            {
                await fromDirector.BeforeExitAsync(cts.Token);
                await toDirector.BeforeEnterAsync(cts.Token);
                await UniTask.WhenAll
                (
                    fromDirector.ExitAsync(cts.Token),
                    toDirector.EnterAsync(cts.Token)
                );
            }
            catch (Exception ex)
            {
                // シーンを初期化
                await ResetSceneAsync(cts.Token);

                throw new InvalidOperationException($"Scene transition failed from {from} to {to}. Scene has been reset.", ex);

            }
            finally
            {
                if (sceneChangeSemaphore.CurrentCount == 1)
                {
                    throw new InvalidOperationException("No scene change is in progress.");
                }

                sceneChangeSemaphore.Release();
            }
        }

        ISceneDirector GetSceneDirector(SceneType sceneType)
        {
            if (sceneDirectors.TryGetValue(sceneType, out var director))
            {
                return director;
            }

            throw new InvalidOperationException($"SceneDirector not found for SceneType: {sceneType}");
        }
    }
}
