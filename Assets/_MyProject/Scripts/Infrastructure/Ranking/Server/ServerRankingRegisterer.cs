using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;

namespace MyProject.Infrastructure
{
    public class ServerRankingRegisterer : IRankingRegisterer
    {
        public UniTask RegisterAsync(ResultCore result, CancellationToken ct)
        {
            throw new System.NotImplementedException();
        }
    }
}
