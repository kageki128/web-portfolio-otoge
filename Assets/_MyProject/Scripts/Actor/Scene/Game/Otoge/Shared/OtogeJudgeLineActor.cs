using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    [RequireComponent(typeof(LineRenderer))]
    public class OtogeJudgeLineActor : OtogeSharedActorBase
    {
        [Serializable]
        class OtogeJudgeLineSettings
        {
            public OtogeType Type => type;
            [SerializeField] OtogeType type;

            public Vector3 LocalPosition => localPosition;
            [SerializeField] Vector3 localPosition;

            public Vector3 LocalEulerAngles => localEulerAngles;
            [SerializeField] Vector3 localEulerAngles;

            public float LineLength => lineLength;
            [SerializeField] float lineLength;

            public float CurveRatePercent => curveRatePercent;
            [SerializeField, Range(0f, 100f)] float curveRatePercent;
        }

        [SerializeField] OtogeJudgeLineSettings[] otogeJudgeLineSettings;
        [SerializeField, Min(2)] int curveSegments = 128;

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
            var lineSettings = Array.Find(otogeJudgeLineSettings, x => x.Type == otogeType);
            var lineRenderer = GetComponent<LineRenderer>();
            if (lineSettings == null)
            {
                gameObject.SetActive(true);
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, Vector3.zero);
                lineRenderer.SetPosition(1, Vector3.zero);
                return;
            }

            gameObject.SetActive(true);
            transform.localPosition = lineSettings.LocalPosition;
            transform.localEulerAngles = lineSettings.LocalEulerAngles;

            var curveRate = Mathf.Clamp01(lineSettings.CurveRatePercent / 100f);
            if (curveRate == 0f)
            {
                lineRenderer.positionCount = 2;
                var halfLength = lineSettings.LineLength / 2f;
                lineRenderer.SetPosition(0, new Vector3(-halfLength, 0f, 0f));
                lineRenderer.SetPosition(1, new Vector3(halfLength, 0f, 0f));
                return;
            }

            var arcAngle = Mathf.Lerp(0.0001f, Mathf.PI * 2f, curveRate);
            var radius = lineSettings.LineLength / arcAngle;
            var halfArcAngle = arcAngle / 2f;

            var points = Mathf.Max(2, curveSegments);
            lineRenderer.positionCount = points + 1;

            for (var i = 0; i <= points; i++)
            {
                var t = i / (float)points;
                var angle = Mathf.Lerp(-halfArcAngle, halfArcAngle, t);
                var x = radius * Mathf.Sin(angle);
                var y = radius - radius * Mathf.Cos(angle);
                lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
            }
        }
    }
}
