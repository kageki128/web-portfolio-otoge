using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class JudgeEffectFactory : MonoBehaviour
    {
        [SerializeField] JudgeLaneEffectActor judgeLaneEffectActorPrefab;
        [SerializeField] Transform judgeLaneEffectParent;
        [SerializeField] JudgeTextEffectActor judgeTextEffectActorPrefab;
        [SerializeField] Transform judgeTextEffectParent;

        public void PlayEffect(
            JudgeType judgeType,
            float riseOffset,
            RiseAxis riseAxis,
            Vector3 position,
            float riseAmount = JudgeTextEffectActor.DefaultRiseAmount
        )
        {
            var laneEffectActor = Instantiate(judgeLaneEffectActorPrefab, judgeLaneEffectParent);
            laneEffectActor.transform.localPosition = position;
            laneEffectActor.Play(judgeType);

            var textEffectActor = Instantiate(judgeTextEffectActorPrefab, judgeTextEffectParent);
            textEffectActor.transform.localPosition = position;
            textEffectActor.Play(judgeType, riseOffset, riseAxis, riseAmount);
        }
    }
}
