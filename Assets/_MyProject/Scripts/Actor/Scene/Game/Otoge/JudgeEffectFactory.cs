using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class JudgeEffectFactory : MonoBehaviour
    {
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
            var effectActor = Instantiate(judgeTextEffectActorPrefab, judgeTextEffectParent);
            effectActor.transform.localPosition = position;
            effectActor.Play(judgeType, riseOffset, riseAxis, riseAmount);
        }
    }
}
