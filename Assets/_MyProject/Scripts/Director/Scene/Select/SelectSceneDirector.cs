using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Actor;
using MyProject.Core;
using R3;

namespace MyProject.Director
{
    public class SelectSceneDirector : ISceneDirector, IDisposable
    {
        public Observable<SceneType> SceneChangeRequest => sceneChangeRequest;
        readonly Subject<SceneType> sceneChangeRequest = new();

        public Observable<Unit> SceneReloadRequest => sceneReloadRequest;
        readonly Subject<Unit> sceneReloadRequest = new();

        readonly SelectActorHub selectActorHub;
        readonly RootActorHub rootActorHub;
        readonly PlayerSettingsCore playerSettingsCore;
        readonly GameSessionCore gameSessionCore;

        readonly CompositeDisposable disposables = new();
        readonly CompositeDisposable demoDisposables = new();
        CancellationTokenSource demoCts;
        bool isRestartingDemo;

        public SelectSceneDirector
        (
            SelectActorHub selectActorHub,
            RootActorHub rootActorHub,
            PlayerSettingsCore playerSettingsCore,
            GameSessionCore gameSessionCore
        )
        {
            this.selectActorHub = selectActorHub;
            this.rootActorHub = rootActorHub;
            this.playerSettingsCore = playerSettingsCore;
            this.gameSessionCore = gameSessionCore;
        }

        public async UniTask InitializeAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            selectActorHub.Initialize();
            await UniTask.CompletedTask;
        }

        public async UniTask BeforeEnterAsync(CancellationToken ct)
        {
            rootActorHub.SetOtogeInputEnabled(false);
            ResetDemoCancellationToken(ct);
            await InitializeDemoAsync(demoCts.Token);
            SubscribeCoreForActor();
        }

        public async UniTask EnterAsync(CancellationToken ct)
        {
            await selectActorHub.ShowAsync(ct);
        }

        public async UniTask AfterEnterAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            SubscribeDemoSession();
            StartDemo();
            SubscribeActorForCore();
            await UniTask.CompletedTask;
        }

        public void Tick()
        {
            gameSessionCore.ProceedGame(playerSettingsCore.NoteOffset.CurrentValue, true);
        }

        public async UniTask BeforeExitAsync(CancellationToken ct)
        {
            disposables.Clear();
            demoDisposables.Clear();
            gameSessionCore.PauseGame();
            demoCts?.Cancel();
            demoCts?.Dispose();
            demoCts = null;

            await UniTask.CompletedTask;
        }

        public async UniTask ExitAsync(CancellationToken ct)
        {
            await UniTask.WhenAll(
                 selectActorHub.HideAsync(ct),
                 rootActorHub.HideAndDestroyNotesAsync(ct)
             );
        }

        public void Dispose()
        {
            disposables.Dispose();
            demoDisposables.Dispose();
            demoCts?.Cancel();
            demoCts?.Dispose();
            sceneChangeRequest.Dispose();
            sceneReloadRequest.Dispose();
        }

        void SubscribeCoreForActor()
        {
            SubscribeDemoCoreForActor();

            playerSettingsCore.SelectedBeatmapType
                .Subscribe(beatmapType => selectActorHub.SetDifficultyText(beatmapType.ToString()))
                .AddTo(disposables);
        }

        void SubscribeActorForCore()
        {
            selectActorHub.StartGame
                .Take(1)
                .Subscribe(_ => sceneChangeRequest.OnNext(SceneType.Game))
                .AddTo(disposables);
            selectActorHub.DifficultyScrolled
                .Subscribe(direction => playerSettingsCore.ChangeBeatmapType(direction))
                .AddTo(disposables);
        }

        void SubscribeDemoCoreForActor()
        {
            demoDisposables.Clear();

            gameSessionCore.CurrentOtogeTypeTransition
                .Subscribe(transition => rootActorHub.ApplyOtogeTypeTransition(transition))
                .AddTo(demoDisposables);
            gameSessionCore.OtogeEventTriggered
                .Subscribe(_ => rootActorHub.ExecuteOtogeEvent())
                .AddTo(demoDisposables);
        }

        void SubscribeDemoSession()
        {
            var noteCores = new List<NoteCoreBase>();
            noteCores.AddRange(gameSessionCore.NoteCores);
            noteCores.AddRange(gameSessionCore.MeasureLineCores);
            rootActorHub.CreateNotes(noteCores);

            gameSessionCore.EndReached
                .Subscribe(_ => RestartDemoAsync().Forget())
                .AddTo(demoDisposables);

            foreach (var kvp in gameSessionCore.TimelineToCurrentScroll)
            {
                var timeline = kvp.Key;
                kvp.Value
                    .Subscribe(scroll => rootActorHub.UpdateNotesByTimeline(timeline, gameSessionCore.CurrentBeat.CurrentValue, scroll, playerSettingsCore.ScrollSpeed.CurrentValue))
                    .AddTo(demoDisposables);
            }

        }

        void StartDemo()
        {
            gameSessionCore.StartGame();
        }

        async UniTask RestartDemoAsync()
        {
            if (isRestartingDemo || demoCts == null || demoCts.IsCancellationRequested)
            {
                return;
            }

            isRestartingDemo = true;
            try
            {
                var ct = demoCts.Token;
                demoDisposables.Clear();
                gameSessionCore.PauseGame();
                await rootActorHub.HideAndDestroyNotesAsync(ct);
                await InitializeDemoAsync(ct);
                SubscribeDemoCoreForActor();
                SubscribeDemoSession();
                StartDemo();
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                isRestartingDemo = false;
            }
        }

        async UniTask InitializeDemoAsync(CancellationToken ct)
        {
            await gameSessionCore.InitializeAsync(BeatmapType.Demo, ct);
        }

        void ResetDemoCancellationToken(CancellationToken ct)
        {
            demoCts?.Cancel();
            demoCts?.Dispose();
            demoCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        }
    }
}
