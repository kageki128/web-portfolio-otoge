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
            ResetDemoCancellationToken(ct);
            await InitializeDemoAsync(demoCts.Token);
        }

        public async UniTask EnterAsync(CancellationToken ct)
        {
            await selectActorHub.ShowAsync(ct);
            SubscribeDemoSession();
            StartDemo();
            HandleEnter();
        }

        public void Tick()
        {
            gameSessionCore.ProceedGame(playerSettingsCore.NoteOffset.CurrentValue);
        }

        public async UniTask BeforeExitAsync(CancellationToken ct)
        {
            disposables.Clear();
            demoDisposables.Clear();
            gameSessionCore.PauseGame();
            demoCts?.Cancel();
            demoCts?.Dispose();
            demoCts = null;
            await rootActorHub.HideAndDestroyNotesAsync(ct);
        }

        public async UniTask ExitAsync(CancellationToken ct)
        {
            await selectActorHub.HideAsync(ct);
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

        void HandleEnter()
        {
            selectActorHub.StartGame
                .Take(1)
                .Subscribe(_ => sceneChangeRequest.OnNext(SceneType.Game))
                .AddTo(disposables);
        }

        void SubscribeDemoSession()
        {
            demoDisposables.Clear();

            var noteCores = new List<NoteCoreBase>();
            noteCores.AddRange(gameSessionCore.NoteCores);
            noteCores.AddRange(gameSessionCore.MeasureLineCores);
            rootActorHub.CreateNotes(noteCores);

            gameSessionCore.CurrentOtogeTypeTransition
                .Subscribe(transition => rootActorHub.ApplyOtogeTypeTransition(transition))
                .AddTo(demoDisposables);
            gameSessionCore.OtogeEventTriggered
                .Subscribe(_ => rootActorHub.ExecuteOtogeEvent())
                .AddTo(demoDisposables);
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

            rootActorHub.LanePressed
                .Subscribe(lane => gameSessionCore.JudgePressLane(lane))
                .AddTo(demoDisposables);
            rootActorHub.LaneReleased
                .Subscribe(lane => gameSessionCore.JudgeReleaseLane(lane))
                .AddTo(demoDisposables);
            rootActorHub.AirPressed
                .Subscribe(_ => gameSessionCore.JudgePressAir())
                .AddTo(demoDisposables);
            rootActorHub.AirReleased
                .Subscribe(_ => gameSessionCore.JudgeReleaseAir())
                .AddTo(demoDisposables);
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
