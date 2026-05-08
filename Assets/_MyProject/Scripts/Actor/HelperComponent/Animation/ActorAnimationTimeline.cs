using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MyProject.Actor
{
    /// <summary>
    /// 対象のActorとアニメーション再生タイミングの組の集合から、アニメーションのタイムラインを作成するクラス。
    /// </summary>
    public class ActorAnimationTimeline : ActorBase
    {
        [Serializable]
        class TimedActorAnimation
        {
            [field: SerializeField]
            public ActorBase Actor { get; private set; }

            [field: SerializeField, Min(0f)]
            public float InitialShowStartSeconds { get; private set; }

            [field: SerializeField, Min(0f)]
            public float ShowStartSeconds { get; private set; }

            [field: SerializeField, Min(0f)]
            public float HideStartSeconds { get; private set; }
        }

        [SerializeField] bool useInitialShow = false;

        [Header("Actor Timelines")]
        [SerializeField] List<TimedActorAnimation> actorAnimations = new();

        Func<CancellationToken, UniTask> playInitialShowTimelineAsync = _ => UniTask.CompletedTask;
        Func<CancellationToken, UniTask> playShowTimelineAsync = _ => UniTask.CompletedTask;
        Func<CancellationToken, UniTask> playHideTimelineAsync = _ => UniTask.CompletedTask;

        public override void Initialize()
        {
            var validAnimations = new List<TimedActorAnimation>(actorAnimations.Count);

            foreach (var timedAnimation in actorAnimations)
            {
                if (timedAnimation == null)
                {
                    Debug.LogWarning($"[{nameof(ActorAnimationTimeline)}] TimedActorAnimation is null.", this);
                    continue;
                }

                if (timedAnimation.Actor == null)
                {
                    Debug.LogWarning($"[{nameof(ActorAnimationTimeline)}] Actor is not assigned.", this);
                    continue;
                }

                timedAnimation.Actor.Initialize();
                validAnimations.Add(timedAnimation);
            }

            playInitialShowTimelineAsync = BuildTimelineTask(validAnimations, timed => timed.InitialShowStartSeconds, PlayInitialShowAsync);
            playShowTimelineAsync = BuildTimelineTask(validAnimations, timed => timed.ShowStartSeconds, PlayShowAsync);
            playHideTimelineAsync = BuildTimelineTask(validAnimations, timed => timed.HideStartSeconds, PlayHideAsync);
        }

        /// <summary>
        /// Initial Show Timelineを再生する。
        /// </summary>
        public override UniTask InitialShowAsync(CancellationToken ct)
        {
            return useInitialShow
                ? playInitialShowTimelineAsync(ct)
                : playShowTimelineAsync(ct);
        }

        /// <summary>
        /// Show Timelineを再生する。
        /// </summary>
        public override UniTask ShowAsync(CancellationToken ct)
        {
            return playShowTimelineAsync(ct);
        }

        /// <summary>
        /// Hide Timelineを再生する。
        /// </summary>
        public override UniTask HideAsync(CancellationToken ct)
        {
            return playHideTimelineAsync(ct);
        }

        Func<CancellationToken, UniTask> BuildTimelineTask
        (
            List<TimedActorAnimation> timeline,
            Func<TimedActorAnimation, float> getStartSeconds,
            Func<ActorBase, CancellationToken, UniTask> playAsync
        )
        {
            if (timeline.Count == 0)
            {
                return _ => UniTask.CompletedTask;
            }

            var timedPlays = new List<Func<CancellationToken, UniTask>>(timeline.Count);

            foreach (var timedAnimation in timeline)
            {
                var actor = timedAnimation.Actor;
                var startSeconds = Mathf.Max(0f, getStartSeconds(timedAnimation));

                timedPlays.Add(ct => PlayActorAtAsync(actor, startSeconds, playAsync, ct));
            }

            return ct =>
            {
                var playTasks = new UniTask[timedPlays.Count];
                for (var i = 0; i < timedPlays.Count; i++)
                {
                    playTasks[i] = timedPlays[i](ct);
                }

                return UniTask.WhenAll(playTasks);
            };
        }

        static async UniTask PlayActorAtAsync
        (
            ActorBase actor,
            float startSeconds,
            Func<ActorBase, CancellationToken, UniTask> playAsync,
            CancellationToken ct
        )
        {
            ct.ThrowIfCancellationRequested();

            if (startSeconds > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(startSeconds), cancellationToken: ct);
            }

            await playAsync(actor, ct);
        }

        static UniTask PlayInitialShowAsync(ActorBase actor, CancellationToken ct)
        {
            return actor.InitialShowAsync(ct);
        }

        static UniTask PlayShowAsync(ActorBase actor, CancellationToken ct)
        {
            return actor.ShowAsync(ct);
        }

        static UniTask PlayHideAsync(ActorBase actor, CancellationToken ct)
        {
            return actor.HideAsync(ct);
        }
    }
}
