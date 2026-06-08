using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Actor;
using MyProject.Core;
using R3;

namespace MyProject.Director
{
    public class RootDirector : System.IDisposable
    {
        readonly RootActorHub rootActorHub;
        readonly PlayerSettingsCore playerSettingsCore;
        readonly CompositeDisposable disposables = new();

        public RootDirector(RootActorHub rootActorHub, PlayerSettingsCore playerSettingsCore)
        {
            this.rootActorHub = rootActorHub;
            this.playerSettingsCore = playerSettingsCore;
        }

        public async UniTask InitializeAsync(CancellationToken ct)
        {
            rootActorHub.Initialize();

            disposables.Clear();

            rootActorHub.ScrollSpeedNormalizedChanged
                .Subscribe(value => playerSettingsCore.SetScrollSpeedNormalized(value))
                .AddTo(disposables);
            rootActorHub.ScrollSpeedResetRequested
                .Subscribe(_ => playerSettingsCore.ResetScrollSpeed())
                .AddTo(disposables);
            playerSettingsCore.ScrollSpeedNormalized
                .Subscribe(value => rootActorHub.SetScrollSpeedNormalized(value))
                .AddTo(disposables);
            playerSettingsCore.ScrollSpeed
                .Subscribe(value => rootActorHub.SetScrollSpeed(value))
                .AddTo(disposables);

            rootActorHub.NoteOffsetNormalizedChanged
                .Subscribe(value => playerSettingsCore.SetNoteOffsetNormalized(value))
                .AddTo(disposables);
            rootActorHub.NoteOffsetResetRequested
                .Subscribe(_ => playerSettingsCore.ResetNoteOffset())
                .AddTo(disposables);
            playerSettingsCore.NoteOffsetNormalized
                .Subscribe(value => rootActorHub.SetNoteOffsetNormalized(value))
                .AddTo(disposables);
            playerSettingsCore.NoteOffset
                .Subscribe(value => rootActorHub.SetNoteOffset(value))
                .AddTo(disposables);

            await UniTask.CompletedTask;
        }

        public void Tick()
        {

        }

        public async UniTask TransitSceneAsync(SceneType sceneType, CancellationToken ct)
        {
            await rootActorHub.TransitSceneAsync(sceneType, ct);
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
