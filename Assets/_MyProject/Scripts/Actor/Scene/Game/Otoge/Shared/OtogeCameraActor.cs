using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class OtogeCameraActor : OtogeSharedActorBase
    {
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
    }
}
