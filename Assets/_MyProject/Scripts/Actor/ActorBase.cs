using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MyProject.Actor
{
    /// <summary>
    /// Actorの基底クラス。
    /// 全てのアクターは基本的にこのクラスを継承する。
    /// </summary>
    public abstract class ActorBase : MonoBehaviour
    {
        /// <summary>
        /// 初期化処理を行う。
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// 表示処理。
        /// </summary>
        public abstract UniTask ShowAsync(CancellationToken ct);

        /// <summary>
        /// 非表示処理。
        /// </summary>
        public abstract UniTask HideAsync(CancellationToken ct);
    }
}
