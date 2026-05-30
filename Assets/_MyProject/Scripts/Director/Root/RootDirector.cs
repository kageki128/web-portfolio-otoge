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

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            await rootActorHub.InitializeAsync(cancellationToken);

            disposables.Clear();
            
            rootActorHub.ScrollSpeedNormalizedChanged
                .Subscribe(value => playerSettingsCore.SetScrollSpeedNormalized(value))
                .AddTo(disposables);
            playerSettingsCore.ScrollSpeedNormalized
                .Subscribe(value => rootActorHub.SetScrollSpeedNormalized(value))
                .AddTo(disposables);
        }

        public void Tick()
        {

        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
