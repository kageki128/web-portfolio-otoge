using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using TMPro;
using UnityEngine;

namespace MyProject.Actor
{
    [RequireComponent(typeof(StandardTransitionAnimator))]
    public class ComboTextActor : ActorBase
    {
        const float ComboPopScaleMultiplier = 1.01f;
        const float ComboPopDuration = 0.15f;

        [SerializeField] TMP_Text text;

        StandardTransitionAnimator transitionAnimator;
        int currentCombo;
        Vector3 baseScale;
        MotionHandle comboPopHandle;

        public override void Initialize()
        {
            transitionAnimator = GetComponent<StandardTransitionAnimator>();
            transitionAnimator.Initialize();

            baseScale = transform.localScale;
            SetCombo(0);
            transform.localScale = baseScale;
            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            if (!IsShow())
            {
                return;
            }

            gameObject.SetActive(true);
            await transitionAnimator.ShowAsync(ct);
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            comboPopHandle.TryCancel();
            transform.localScale = baseScale;

            if (!gameObject.activeSelf)
            {
                return;
            }

            await transitionAnimator.HideAsync(ct);
            gameObject.SetActive(false);
        }

        public void SetCombo(int combo)
        {
            var previousCombo = currentCombo;
            currentCombo = combo;
            text.text = $"{combo}";

            var show = IsShow();
            gameObject.SetActive(show);

            if (!show)
            {
                comboPopHandle.TryCancel();
                transform.localScale = baseScale;
                return;
            }

            if (combo > previousCombo)
            {
                PlayComboPopAnimation();
            }
        }

        bool IsShow()
        {
            return currentCombo >= 5;
        }

        void PlayComboPopAnimation()
        {
            comboPopHandle.TryCancel();

            var targetScale = baseScale * ComboPopScaleMultiplier;
            var halfDuration = ComboPopDuration * 0.5f;

            comboPopHandle = LMotion.Create(transform.localScale, targetScale, halfDuration)
                .WithEase(Ease.OutCubic)
                .WithOnComplete(() =>
                {
                    comboPopHandle = LMotion.Create(targetScale, baseScale, halfDuration)
                        .WithEase(Ease.OutCubic)
                        .Bind(value => transform.localScale = value)
                        .AddTo(this);
                })
                .Bind(value => transform.localScale = value)
                .AddTo(this);
        }
    }
}
