using System.Threading;
using Cysharp.Threading.Tasks;

namespace MyProject.Actor
{
    public class MasterActor : OtogeActorBase
    {
        public override void Initialize()
        {
            gameObject.SetActive(false);
        }

        public override UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            return UniTask.CompletedTask;
        }

        public override UniTask HideAsync(CancellationToken ct)
        {
            gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }
    }
}
