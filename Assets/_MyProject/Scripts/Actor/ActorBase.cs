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
        /// ゲームの最初のシーンとして表示されるときの表示処理。
        /// OverrideしなければShowAsyncと同じ処理を行う。
        /// 多くの場合、すぐに表示されることが望ましいだろう。
        /// </summary>
        public virtual UniTask InitialShowAsync(CancellationToken ct) => ShowAsync(ct);

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
