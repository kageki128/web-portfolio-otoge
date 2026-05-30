using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class JudgeEffectFactory : MonoBehaviour
    {
        [SerializeField] JudgeTextEffectActor judgeTextEffectActorPrefab;
        [SerializeField] Transform judgeTextEffectParent;

        public void PlayEffect(JudgeType judgeType, float riseAmount, RiseAxis riseAxis, Vector3 position)
        {
            var effectActor = Instantiate(judgeTextEffectActorPrefab, judgeTextEffectParent);
            effectActor.transform.localPosition = position;
            effectActor.Play(judgeType, riseAmount, riseAxis);
        }
    }
}
