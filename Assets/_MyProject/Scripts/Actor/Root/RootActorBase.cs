using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public abstract class RootActorBase : MonoBehaviour
    {
        public abstract void Initialize();
        public abstract UniTask TransitSceneAsync(SceneType sceneType, CancellationToken ct);
    }
}
