using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    [RequireComponent(typeof(LineRenderer))]
    public class OtogeLineActor : OtogeSharedActorBase
    {
        [Serializable]
        class OtogeLineSettings
        {
            public OtogeType Type => type;
            [SerializeField] OtogeType type;

            public Vector3 LocalPosition => localPosition;
            [SerializeField] Vector3 localPosition;

            public Vector3 LocalEulerAngles => localEulerAngles;
            [SerializeField] Vector3 localEulerAngles;

            public float LineLength => lineLength;
            [SerializeField] float lineLength;
        }

        [SerializeField] OtogeLineSettings[] otogeLineSettings;

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
            var lineSettings = Array.Find(otogeLineSettings, x => x.Type == otogeType);
            if (lineSettings == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            var lineRenderer = GetComponent<LineRenderer>();
            transform.localPosition = lineSettings.LocalPosition;
            transform.localEulerAngles = lineSettings.LocalEulerAngles;

            var direction = lineRenderer.positionCount >= 2
                ? lineRenderer.GetPosition(1) - lineRenderer.GetPosition(0)
                : Vector3.up;
            if (direction == Vector3.zero) direction = Vector3.up;

            lineRenderer.positionCount = 2;
            var half = direction.normalized * (lineSettings.LineLength / 2f);
            lineRenderer.SetPosition(0, -half);
            lineRenderer.SetPosition(1, half);
        }
    }
}
