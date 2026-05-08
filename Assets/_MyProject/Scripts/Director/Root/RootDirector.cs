using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Actor;

namespace MyProject.Director
{
    public class RootDirector
    {
        readonly RootActorHub rootActorHub;

        public RootDirector(RootActorHub rootActorHub)
        {
            this.rootActorHub = rootActorHub;
        }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            await rootActorHub.InitializeAsync(cancellationToken);
        }

        public void Tick()
        {

        }
    }
}
