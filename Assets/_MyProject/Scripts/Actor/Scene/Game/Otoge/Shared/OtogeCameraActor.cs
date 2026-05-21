using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class OtogeCameraActor : OtogeSharedActorBase
    {
        const float StateTransitionDuration = 0.5f;
        const Ease StateTransitionEase = Ease.OutCubic;

        [Serializable]
        class OtogeCameraSettings
        {
            public OtogeType Type => type;
            [SerializeField] OtogeType type;

            public Vector3 LocalPosition => localPosition;
            [SerializeField] Vector3 localPosition;

            public Vector3 LocalEulerAngles => localEulerAngles;
            [SerializeField] Vector3 localEulerAngles;
        }

        [Serializable]
        class DefaultCameraSettings
        {
            public Vector3 LocalPosition => localPosition;
            [SerializeField] Vector3 localPosition;

            public Vector3 LocalEulerAngles => localEulerAngles;
            [SerializeField] Vector3 localEulerAngles;
        }

        [SerializeField] OtogeCameraSettings[] otogeCameraSettings;
        [SerializeField] DefaultCameraSettings defaultSettings;

        MotionHandle positionHandle;
        MotionHandle eulerHandle;

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

        public override void SetState(OtogeType otogeType)
        {
            var settings = Array.Find(otogeCameraSettings, x => x.Type == otogeType);

            transform.localPosition = settings?.LocalPosition ?? defaultSettings.LocalPosition;
            transform.localEulerAngles = settings?.LocalEulerAngles ?? defaultSettings.LocalEulerAngles;
        }

        public override async UniTask SetStateAsync(OtogeType otogeType, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var settings = Array.Find(otogeCameraSettings, x => x.Type == otogeType);
            var targetPosition = settings?.LocalPosition ?? defaultSettings.LocalPosition;
            var targetEuler = settings?.LocalEulerAngles ?? defaultSettings.LocalEulerAngles;

            positionHandle.TryCancel();
            eulerHandle.TryCancel();

            positionHandle = LMotion.Create(transform.localPosition, targetPosition, StateTransitionDuration)
                .WithEase(StateTransitionEase)
                .Bind(value => transform.localPosition = value)
                .AddTo(this);

            eulerHandle = LMotion.Create(transform.localEulerAngles, targetEuler, StateTransitionDuration)
                .WithEase(StateTransitionEase)
                .Bind(value => transform.localEulerAngles = value)
                .AddTo(this);

            await UniTask.WhenAll
            (
                positionHandle.ToUniTask(CancelBehavior.Cancel, false, cancellationToken),
                eulerHandle.ToUniTask(CancelBehavior.Cancel, false, cancellationToken)
            );
        }
    }
}
