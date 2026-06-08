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
        readonly PlayerSettingsCore playerSettingsCore;
        readonly IBeatmapRepository beatmapRepository;

        readonly CompositeDisposable disposables = new();
        BeatmapCore demoBeatmapCore;

        public SelectSceneDirector
        (
            SelectActorHub selectActorHub,
            PlayerSettingsCore playerSettingsCore,
            IBeatmapRepository beatmapRepository
        )
        {
            this.selectActorHub = selectActorHub;
            this.playerSettingsCore = playerSettingsCore;
            this.beatmapRepository = beatmapRepository;
        }

        public async UniTask InitializeAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            selectActorHub.Initialize();
            await UniTask.CompletedTask;
        }

        public async UniTask BeforeEnterAsync(CancellationToken ct)
        {
            demoBeatmapCore = await beatmapRepository.GetAsync(BeatmapType.Demo, ct);
        }

        public async UniTask EnterAsync(CancellationToken ct)
        {
            await selectActorHub.ShowAsync(ct);
            SubscribeDemoBeatmap();
            StartDemo();
            HandleEnter();
        }

        public void Tick()
        {
            demoBeatmapCore?.AdvanceTime(0f);
        }

        public async UniTask BeforeExitAsync(CancellationToken ct)
        {
            disposables.Clear();
            demoBeatmapCore = null;
            await selectActorHub.HideAndDestroyNotesAsync(ct);
        }

        public async UniTask ExitAsync(CancellationToken ct)
        {
            await selectActorHub.HideAsync(ct);
        }

        public void Dispose()
        {
            disposables.Dispose();
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

        void SubscribeDemoBeatmap()
        {
            disposables.Clear();

            var noteCores = new List<NoteCoreBase>();
            noteCores.AddRange(demoBeatmapCore.NoteCores);
            noteCores.AddRange(demoBeatmapCore.MeasureLineCores);
            selectActorHub.CreateNotes(noteCores);

            demoBeatmapCore.CurrentOtogeTypeTransition
                .Subscribe(transition => selectActorHub.ApplyOtogeTypeTransition(transition))
                .AddTo(disposables);
            demoBeatmapCore.OtogeEvent
                .Subscribe(_ => selectActorHub.ExecuteOtogeEvent())
                .AddTo(disposables);
            demoBeatmapCore.EndReached
                .Subscribe(_ => StartDemo())
                .AddTo(disposables);

            foreach (var kvp in demoBeatmapCore.TimelineToCurrentScroll)
            {
                var timeline = kvp.Key;
                kvp.Value
                    .Subscribe(scroll => selectActorHub.UpdateNotesByTimeline(timeline, demoBeatmapCore.CurrentBeat.CurrentValue, scroll, playerSettingsCore.ScrollSpeed.CurrentValue))
                    .AddTo(disposables);
            }
        }

        void StartDemo()
        {
            demoBeatmapCore.Start(0);
        }
    }
}
