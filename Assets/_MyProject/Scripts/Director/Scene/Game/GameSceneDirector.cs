using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Actor;
using MyProject.Core;
using ObservableCollections;
using R3;

namespace MyProject.Director
{
    public class GameSceneDirector : ISceneDirector, IDisposable
    {
        public Observable<SceneType> SceneChangeRequest => sceneChangeRequest;
        readonly Subject<SceneType> sceneChangeRequest = new();

        public Observable<Unit> SceneReloadRequest => sceneReloadRequest;
        readonly Subject<Unit> sceneReloadRequest = new();

        readonly PlayerSettingsCore playerSettingsCore;
        readonly GameSessionCore gameSessionCore;
        readonly RootActorHub rootActorHub;
        readonly GameActorHub gameActorHub;

        readonly CompositeDisposable disposables = new();

        public GameSceneDirector(PlayerSettingsCore playerSettingsCore, GameSessionCore gameSessionCore, RootActorHub rootActorHub, GameActorHub gameActorHub)
        {
            this.playerSettingsCore = playerSettingsCore;
            this.gameSessionCore = gameSessionCore;
            this.rootActorHub = rootActorHub;
            this.gameActorHub = gameActorHub;
        }

        public async UniTask InitializeAsync(CancellationToken ct)
        {
            gameActorHub.Initialize();
            await UniTask.CompletedTask;
        }

        public async UniTask BeforeEnterAsync(CancellationToken ct)
        {
            await gameSessionCore.InitializeAsync(playerSettingsCore.SelectedBeatmapType.CurrentValue, ct);

            disposables.Clear();
            SubscribeCoreForActor();
        }

        public async UniTask EnterAsync(CancellationToken ct)
        {
            await gameActorHub.ShowAsync(ct);
        }

        public async UniTask AfterEnterAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            SubscribeNotes();
            SubscribeActorForCore();
            rootActorHub.SetOtogeInputEnabled(true);
            StartGame();
            await UniTask.CompletedTask;
        }

        public void Tick()
        {
            gameSessionCore.ProceedGame(playerSettingsCore.NoteOffset.CurrentValue, playerSettingsCore.IsAutoPlay.CurrentValue);
        }

        public async UniTask BeforeExitAsync(CancellationToken ct)
        {
            rootActorHub.SetOtogeInputEnabled(false);
            await playerSettingsCore.SaveCurrentSettingsAsync(ct);
            disposables.Clear();
            if (gameSessionCore.State.CurrentValue is GameState.Finished)
            {
                await gameSessionCore.SaveCurrentScoreAsync(ct);
            }
            else
            {
                gameSessionCore.PauseGame();
            }
            await UniTask.CompletedTask;
        }

        public async UniTask ExitAsync(CancellationToken ct)
        {
            await UniTask.WhenAll(
                gameActorHub.HideAsync(ct),
                rootActorHub.HideAndDestroyNotesAsync(ct)
            );
        }

        public void Dispose()
        {
            disposables.Dispose();
            sceneChangeRequest.Dispose();
            sceneReloadRequest.Dispose();
        }

        void SubscribeCoreForActor()
        {
            // Coreを購読
            gameActorHub.SetMetaData(gameSessionCore.MetaData);

            gameSessionCore.Score
                .Subscribe(score => gameActorHub.SetScore(score))
                .AddTo(disposables);
            gameSessionCore.Combo
                .Subscribe(combo => gameActorHub.SetCombo(combo))
                .AddTo(disposables);
            gameSessionCore.CurrentOtogeTypeTransition
                .Subscribe(ApplyOtogeTypeTransition)
                .AddTo(disposables);
            gameSessionCore.OtogeEventTriggered
                .Subscribe(_ => rootActorHub.ExecuteOtogeEvent())
                .AddTo(disposables);
            gameActorHub.SetJudgeCounts(gameSessionCore.JudgeCounts);
            gameSessionCore.JudgeCounts
                .ObserveDictionaryReplace()
                .Subscribe(_ => gameActorHub.SetJudgeCounts(gameSessionCore.JudgeCounts))
                .AddTo(disposables);

        }

        void SubscribeNotes()
        {
            var noteCores = new List<NoteCoreBase>();
            noteCores.AddRange(gameSessionCore.NoteCores);
            noteCores.AddRange(gameSessionCore.MeasureLineCores);
            var timelineToCurrentScroll = gameSessionCore.TimelineToCurrentScroll;
            rootActorHub.CreateNotes(noteCores);
            foreach (var kvp in timelineToCurrentScroll)
            {
                int timeline = kvp.Key;
                var currentScroll = kvp.Value;
                currentScroll
                    .Subscribe(scroll => rootActorHub.UpdateNotesByTimeline(timeline, gameSessionCore.CurrentBeat.CurrentValue, scroll, playerSettingsCore.ScrollSpeed.CurrentValue))
                    .AddTo(disposables);
            }
        }

        void SubscribeActorForCore()
        {
            gameSessionCore.EndReached
                .Take(1)
                .Subscribe(_ =>
                {
                    gameSessionCore.FinishGame();
                    sceneChangeRequest.OnNext(SceneType.Result);
                })
                .AddTo(disposables);
            gameActorHub.Quit
                .Take(1)
                .Subscribe(_ => sceneChangeRequest.OnNext(SceneType.Select))
                .AddTo(disposables);
            gameActorHub.ChangeAuto
                .Subscribe(_ => playerSettingsCore.ChangeAutoPlay())
                .AddTo(disposables);
            rootActorHub.LanePressed
                .Subscribe(lane => gameSessionCore.JudgePressLane(lane))
                .AddTo(disposables);
            rootActorHub.LaneReleased
                .Subscribe(lane => gameSessionCore.JudgeReleaseLane(lane))
                .AddTo(disposables);
            rootActorHub.AirPressed
                .Subscribe(_ => gameSessionCore.JudgePressAir())
                .AddTo(disposables);
            rootActorHub.AirReleased
                .Subscribe(_ => gameSessionCore.JudgeReleaseAir())
                .AddTo(disposables);
        }

        void ApplyOtogeTypeTransition(OtogeTypeTransition transition)
        {
            gameActorHub.ApplyOtogeTypeGaugeTransition(transition);
            rootActorHub.ApplyOtogeTypeTransition(transition);
        }

        void StartGame()
        {
            // ゲーム開始
            var startDspTime = gameSessionCore.StartGame();
            var wave = gameSessionCore.MetaData.Wave;
            var waveOffset = gameSessionCore.MetaData.WaveOffset;
            gameActorHub.PlayWave(wave, startDspTime + waveOffset);
        }
    }
}
