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
            await UniTask.CompletedTask;
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
            gameSessionCore.ProceedGame();
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

            // ノーツを生成してスクロールを購読
            var noteCores = gameSessionCore.NoteCores;
            var timelineToCurrentScroll = gameSessionCore.TimelineToCurrentScroll;
            gameActorHub.CreateNotes(noteCores);
            foreach (var kvp in timelineToCurrentScroll)
            {
                int timeline = kvp.Key;
                var currentScroll = kvp.Value;
                currentScroll.Subscribe(scroll => gameActorHub.UpdateNotesByTimeline(timeline, scroll, 8f)).AddTo(disposables);
            }

            // Coreを購読
            gameActorHub.SetMetaData(gameSessionCore.MetaData);
            gameSessionCore.Score
                .Subscribe(score => gameActorHub.SetScore(score))
                .AddTo(disposables);
            gameSessionCore.Combo
                .Subscribe(combo => gameActorHub.SetCombo(combo))
                .AddTo(disposables);
            foreach (var noteCore in noteCores)
            {
                noteCore.Judge
                    .Skip(1)
                    .Subscribe(_ => gameActorHub.SetJudgeCounts(gameSessionCore.JudgeCounts))
                    .AddTo(disposables);
            }

            // Actorを購読
            gameActorHub.Quit
                .Take(1)
                .Subscribe(_ => sceneChangeRequest.OnNext(SceneType.Select))
                .AddTo(disposables);
            gameActorHub.LanePressed
                .Subscribe(lane => gameSessionCore.JudgePressLane(lane))
                .AddTo(disposables);
            gameActorHub.LaneReleased
                .Subscribe(lane => gameSessionCore.JudgeReleaseLane(lane))
                .AddTo(disposables);
            gameActorHub.AirPressed
                .Subscribe(_ => gameSessionCore.JudgePressAir())
                .AddTo(disposables);
            gameActorHub.AirReleased
                .Subscribe(_ => gameSessionCore.JudgeReleaseAir())
                .AddTo(disposables);

            // ゲーム開始
            var startDspTime = gameSessionCore.StartGame();
            var wave = gameSessionCore.MetaData.Wave;
            gameActorHub.PlayWave(wave, startDspTime);
        }
    }
}
