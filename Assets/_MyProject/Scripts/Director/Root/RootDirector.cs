using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Actor;
using MyProject.Core;

namespace MyProject.Director
{
    public class RootDirector
    {
        readonly IBeatmapRepository beatmapRepository;
        readonly RootActorHub rootActorHub;

        public RootDirector(IBeatmapRepository beatmapRepository, RootActorHub rootActorHub)
        {
            this.beatmapRepository = beatmapRepository;
            this.rootActorHub = rootActorHub;
        }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            await beatmapRepository.LoadAsync(cancellationToken);
            await rootActorHub.InitializeAsync(cancellationToken);
        }

        public void Tick()
        {

        }
    }
}
