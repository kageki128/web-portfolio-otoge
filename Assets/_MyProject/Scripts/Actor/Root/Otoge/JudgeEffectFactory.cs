using MyProject.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace MyProject.Actor
{
    public class JudgeEffectFactory : MonoBehaviour
    {
        const int MaxPoolSize = 128;

        [SerializeField] JudgeLaneEffectActor judgeLaneEffectActorPrefab;
        [SerializeField] Transform judgeLaneEffectParent;
        [SerializeField] JudgeTextEffectActor judgeTextEffectActorPrefab;
        [SerializeField] Transform judgeTextEffectParent;
        [SerializeField, Min(1)] int defaultCapacity = 16;

        ObjectPool<JudgeLaneEffectActor> judgeLaneEffectPool;
        ObjectPool<JudgeTextEffectActor> judgeTextEffectPool;

        public void Initialize()
        {
            judgeLaneEffectPool = new ObjectPool<JudgeLaneEffectActor>(
                CreateJudgeLaneEffect,
                actionOnGet: actor => actor.gameObject.SetActive(true),
                actionOnRelease: actor => actor.gameObject.SetActive(false),
                actionOnDestroy: actor => Destroy(actor.gameObject),
                collectionCheck: true,
                defaultCapacity: defaultCapacity,
                maxSize: MaxPoolSize
            );
            judgeTextEffectPool = new ObjectPool<JudgeTextEffectActor>(
                CreateJudgeTextEffect,
                actionOnGet: actor => actor.gameObject.SetActive(true),
                actionOnRelease: actor => actor.gameObject.SetActive(false),
                actionOnDestroy: actor => Destroy(actor.gameObject),
                collectionCheck: true,
                defaultCapacity: defaultCapacity,
                maxSize: MaxPoolSize
            );
        }

        public void PlayEffect(
            JudgeType judgeType,
            float riseOffset,
            RiseAxis riseAxis,
            Vector3 position,
            float riseAmount = JudgeTextEffectActor.DefaultRiseAmount
        )
        {
            var laneEffectActor = judgeLaneEffectPool.Get();
            laneEffectActor.transform.localPosition = position;
            laneEffectActor.Play(judgeType);

            var textEffectActor = judgeTextEffectPool.Get();
            textEffectActor.transform.localPosition = position;
            textEffectActor.Play(judgeType, riseOffset, riseAxis, riseAmount);
        }

        void OnDestroy()
        {
            judgeLaneEffectPool?.Clear();
            judgeTextEffectPool?.Clear();
        }

        JudgeLaneEffectActor CreateJudgeLaneEffect()
        {
            var actor = Instantiate(judgeLaneEffectActorPrefab, judgeLaneEffectParent);
            actor.Initialize();
            actor.SetReleaseAction(() => judgeLaneEffectPool.Release(actor));
            return actor;
        }

        JudgeTextEffectActor CreateJudgeTextEffect()
        {
            var actor = Instantiate(judgeTextEffectActorPrefab, judgeTextEffectParent);
            actor.Initialize();
            actor.SetReleaseAction(() => judgeTextEffectPool.Release(actor));
            return actor;
        }
    }
}
