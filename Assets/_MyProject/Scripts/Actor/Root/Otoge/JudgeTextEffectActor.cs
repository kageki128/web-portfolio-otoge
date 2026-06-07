using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using MyProject.Core;
using TMPro;
using UnityEngine;

namespace MyProject.Actor
{
    public class JudgeTextEffectActor : ActorBase
    {
        public const float DefaultRiseAmount = 1.5f;
        const float RiseAndFadeDuration = 0.3f;
        const float FinalFadeDuration = 0.2f;

        [SerializeField] TMP_Text mainText;
        [SerializeField] TMP_Text subText;

        [SerializeField] Color perfectColor;
        [SerializeField] Color goodColor;
        [SerializeField] Color missColor;
        [SerializeField] Color fastColor;
        [SerializeField] Color lateColor;

        MotionHandle moveHandle;
        MotionHandle fadeHandle;
        Transform mainCameraTransform;
        Action releaseAction;
        int playVersion;
        bool isReleased;

        public void SetReleaseAction(Action value)
        {
            releaseAction = value;
        }

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
            moveHandle.TryCancel();
            fadeHandle.TryCancel();
            gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }

        public void Play(JudgeType judgeType, float riseOffset, RiseAxis riseAxis, float riseAmount = DefaultRiseAmount)
        {
            playVersion++;
            isReleased = false;

            SetJudgeText(judgeType);

            var shouldShow = mainText.gameObject.activeSelf || subText.gameObject.activeSelf;
            if (!shouldShow)
            {
                ReleaseToPool();
                return;
            }

            moveHandle.TryCancel();
            fadeHandle.TryCancel();

            gameObject.SetActive(true);
            FaceMainCamera();
            SetTextAlpha(0f);

            var startLocalPosition = transform.localPosition + CreateRiseOffset(riseOffset, riseAxis);
            PlayAnimationAsync(startLocalPosition, riseAxis, riseAmount, playVersion, this.GetCancellationTokenOnDestroy()).Forget();
        }

        void SetJudgeText(JudgeType judgeType)
        {
            switch (judgeType)
            {
                case JudgeType.PerfectCriticalFast:
                case JudgeType.PerfectCriticalLate:
                    mainText.text = "PERFECT";
                    mainText.color = perfectColor;
                    mainText.gameObject.SetActive(true);
                    subText.gameObject.SetActive(false);
                    return;
                case JudgeType.PerfectFast:
                    mainText.text = "PERFECT";
                    mainText.color = perfectColor;
                    mainText.gameObject.SetActive(true);
                    subText.text = "FAST";
                    subText.color = fastColor;
                    subText.gameObject.SetActive(true);
                    return;
                case JudgeType.PerfectLate:
                    mainText.text = "PERFECT";
                    mainText.color = perfectColor;
                    mainText.gameObject.SetActive(true);
                    subText.text = "LATE";
                    subText.color = lateColor;
                    subText.gameObject.SetActive(true);
                    return;
                case JudgeType.GoodFast:
                    mainText.text = "GOOD";
                    mainText.color = goodColor;
                    mainText.gameObject.SetActive(true);
                    subText.text = "FAST";
                    subText.color = fastColor;
                    subText.gameObject.SetActive(true);
                    return;
                case JudgeType.GoodLate:
                    mainText.text = "GOOD";
                    mainText.color = goodColor;
                    mainText.gameObject.SetActive(true);
                    subText.text = "LATE";
                    subText.color = lateColor;
                    subText.gameObject.SetActive(true);
                    return;
                case JudgeType.MissFast:
                case JudgeType.MissLate:
                    mainText.text = "MISS";
                    mainText.color = missColor;
                    mainText.gameObject.SetActive(true);
                    subText.gameObject.SetActive(false);
                    return;
                default:
                    mainText.gameObject.SetActive(false);
                    subText.gameObject.SetActive(false);
                    return;
            }
        }

        async UniTaskVoid PlayAnimationAsync(
            Vector3 startLocalPosition,
            RiseAxis riseAxis,
            float riseAmount,
            int version,
            CancellationToken ct
        )
        {
            try
            {
                transform.localPosition = startLocalPosition;
                var targetLocalPosition = startLocalPosition + CreateRiseOffset(riseAmount, riseAxis);

                moveHandle = LMotion.Create(startLocalPosition, targetLocalPosition, RiseAndFadeDuration)
                    .WithEase(Ease.OutCubic)
                    .Bind(value => transform.localPosition = value)
                    .AddTo(this);

                fadeHandle = LMotion.Create(0f, 1f, RiseAndFadeDuration)
                    .WithEase(Ease.OutCubic)
                    .Bind(SetTextAlpha)
                    .AddTo(this);

                await UniTask.WhenAll(
                    moveHandle.ToUniTask(CancelBehavior.Cancel, false, ct),
                    fadeHandle.ToUniTask(CancelBehavior.Cancel, false, ct)
                );

                fadeHandle = LMotion.Create(1f, 0f, FinalFadeDuration)
                    .WithEase(Ease.Linear)
                    .Bind(SetTextAlpha)
                    .AddTo(this);

                await fadeHandle.ToUniTask(CancelBehavior.Cancel, false, ct);
            }
            catch (System.OperationCanceledException)
            {
            }

            if (playVersion != version)
            {
                return;
            }

            ReleaseToPool();
        }

        void LateUpdate()
        {
            if (!gameObject.activeSelf)
            {
                return;
            }

            FaceMainCamera();
        }

        void FaceMainCamera()
        {
            mainCameraTransform ??= Camera.main.transform;
            transform.rotation = mainCameraTransform.rotation;
        }

        static Vector3 CreateRiseOffset(float riseAmount, RiseAxis riseAxis)
        {
            var riseDirection = riseAxis == RiseAxis.Y ? Vector3.up : Vector3.back;
            return riseDirection * riseAmount;
        }

        void SetTextAlpha(float alpha)
        {
            SetAlpha(mainText, alpha);
            SetAlpha(subText, alpha);
        }

        static void SetAlpha(TMP_Text text, float alpha)
        {
            var color = text.color;
            color.a = alpha;
            text.color = color;
        }

        void ReleaseToPool()
        {
            if (isReleased)
            {
                return;
            }

            isReleased = true;
            moveHandle.TryCancel();
            fadeHandle.TryCancel();
            gameObject.SetActive(false);
            releaseAction?.Invoke();
        }
    }

    public enum RiseAxis
    {
        Y,
        Z
    }
}
