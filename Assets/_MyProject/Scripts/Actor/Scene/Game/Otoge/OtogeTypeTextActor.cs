using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using TMPro;
using UnityEngine;

namespace MyProject.Actor
{
    public class OtogeTypeTextActor : ActorBase
    {
        const float ShowRemainingBeatThreshold = 4f;

        [SerializeField] TMP_Text currentText;
        [SerializeField] TMP_Text nextText;

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

        public void ApplyTransition(OtogeTypeTransition transition)
        {
            if (!CanShow(transition))
            {
                gameObject.SetActive(false);
                return;
            }

            currentText.text = GetOtogeTypeText(transition.CurrentType);
            nextText.text = GetOtogeTypeText(transition.NextType);
            gameObject.SetActive(true);
        }

        static bool CanShow(OtogeTypeTransition transition)
        {
            return transition.CurrentType != transition.NextType
                && transition.RemainingBeat > 0f
                && transition.RemainingBeat <= ShowRemainingBeatThreshold;
        }

        string GetOtogeTypeText(OtogeType type)
        {
            return type switch
            {
                OtogeType.Tetra => "TETRA",
                OtogeType.Octa => "OCTA",
                OtogeType.Air => "AIR",
                OtogeType.Laundry => "LAUNDRY",
                OtogeType.Idol => "IDOL",
                OtogeType.Effect => "EFFECT",
                OtogeType.Master => "MASTER",
                OtogeType.Run => "RUN",
                OtogeType.Scan => "SCAN",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}
