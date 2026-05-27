using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
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

            public float LineWidth => lineWidth;
            [SerializeField, Min(0f)] float lineWidth = 0.05f;

            public float CurveRatePercent => curveRatePercent;
            [SerializeField, Range(0f, 100f)] float curveRatePercent;
        }

        [SerializeField] OtogeJudgeLineSettings[] otogeJudgeLineSettings;
        [SerializeField, Min(2)] int curveSegments = 128;

        LineRenderer lineRenderer;
        MotionHandle positionHandle;
        MotionHandle rotationHandle;
        MotionHandle lineShapeHandle;
        MotionHandle widthHandle;
        float currentLineLength;
        float currentCurveRatePercent;
        bool hasCurrentLineShape;

        public override void Initialize()
        {
            EnsureLineRenderer();
            CaptureCurrentLineShape();
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
            EnsureLineRenderer();

            var lineSettings = Array.Find(otogeJudgeLineSettings, x => x.Type == otogeType);
            if (lineSettings == null)
            {
                gameObject.SetActive(true);
                lineRenderer.widthMultiplier = 0f;
                return;
            }

            gameObject.SetActive(true);
            lineRenderer.widthMultiplier = lineSettings.LineWidth;
            transform.localPosition = lineSettings.LocalPosition;
            transform.localEulerAngles = lineSettings.LocalEulerAngles;
            ApplyLineShape(lineSettings.LineLength, lineSettings.CurveRatePercent);

            currentLineLength = lineSettings.LineLength;
            currentCurveRatePercent = lineSettings.CurveRatePercent;
            hasCurrentLineShape = true;
        }

        public override async UniTask SetStateAsync(OtogeType otogeType, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureLineRenderer();

            var lineSettings = Array.Find(otogeJudgeLineSettings, x => x.Type == otogeType);
            var targetPosition = lineSettings?.LocalPosition ?? transform.localPosition;
            var targetEuler = lineSettings?.LocalEulerAngles ?? transform.localEulerAngles;
            var targetWidth = lineSettings == null ? 0f : lineSettings.LineWidth;
            if (!hasCurrentLineShape)
            {
                CaptureCurrentLineShape();
            }

            var fromLineLength = currentLineLength;
            var fromCurveRatePercent = currentCurveRatePercent;
            var targetLineLength = lineSettings?.LineLength ?? fromLineLength;
            var targetCurveRatePercent = lineSettings?.CurveRatePercent ?? fromCurveRatePercent;

            positionHandle.TryCancel();
            rotationHandle.TryCancel();
            lineShapeHandle.TryCancel();
            widthHandle.TryCancel();

            gameObject.SetActive(true);

            var stateTransitionDuration = OtogeAppearance.StateTransitionDuration;
            var stateTransitionEase = OtogeAppearance.StateTransitionEase;

            positionHandle = LMotion.Create(transform.localPosition, targetPosition, stateTransitionDuration)
                .WithEase(stateTransitionEase)
                .Bind(value => transform.localPosition = value)
                .AddTo(this);

            rotationHandle = LMotion.Create(transform.localRotation, Quaternion.Euler(targetEuler), stateTransitionDuration)
                .WithEase(stateTransitionEase)
                .Bind(value => transform.localRotation = value)
                .AddTo(this);

            lineShapeHandle = LMotion.Create(0f, 1f, stateTransitionDuration)
                .WithEase(stateTransitionEase)
                .Bind(progress =>
                {
                    var lineLength = Mathf.Lerp(fromLineLength, targetLineLength, progress);
                    var curveRatePercent = Mathf.Lerp(fromCurveRatePercent, targetCurveRatePercent, progress);
                    ApplyLineShape(lineLength, curveRatePercent);
                    currentLineLength = lineLength;
                    currentCurveRatePercent = curveRatePercent;
                    hasCurrentLineShape = true;
                })
                .AddTo(this);

            widthHandle = LMotion.Create(lineRenderer.widthMultiplier, targetWidth, stateTransitionDuration)
                .WithEase(stateTransitionEase)
                .Bind(value => lineRenderer.widthMultiplier = value)
                .AddTo(this);

            await UniTask.WhenAll
            (
                positionHandle.ToUniTask(CancelBehavior.Cancel, false, cancellationToken),
                rotationHandle.ToUniTask(CancelBehavior.Cancel, false, cancellationToken),
                lineShapeHandle.ToUniTask(CancelBehavior.Cancel, false, cancellationToken),
                widthHandle.ToUniTask(CancelBehavior.Cancel, false, cancellationToken)
            );

            currentLineLength = targetLineLength;
            currentCurveRatePercent = targetCurveRatePercent;
            hasCurrentLineShape = true;
        }

        void ApplyLineShape(float lineLength, float curveRatePercent)
        {
            var curveRate = Mathf.Clamp01(curveRatePercent / 100f);
            var points = Mathf.Max(2, curveSegments);
            lineRenderer.positionCount = points + 1;
            if (curveRate == 0f)
            {
                var halfLength = lineLength / 2f;
                for (var i = 0; i <= points; i++)
                {
                    var t = i / (float)points;
                    var x = Mathf.Lerp(-halfLength, halfLength, t);
                    lineRenderer.SetPosition(i, new Vector3(x, 0f, 0f));
                }
                return;
            }

            var arcAngle = Mathf.Lerp(0.0001f, Mathf.PI * 2f, curveRate);
            var radius = lineLength / arcAngle;
            var halfArcAngle = arcAngle / 2f;

            for (var i = 0; i <= points; i++)
            {
                var t = i / (float)points;
                var angle = Mathf.Lerp(-halfArcAngle, halfArcAngle, t);
                var x = radius * Mathf.Sin(angle);
                var y = radius - radius * Mathf.Cos(angle);
                lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
            }
        }

        void CaptureCurrentLineShape()
        {
            var points = GetCurrentLinePoints();
            currentLineLength = CalculatePolylineLength(points);
            currentCurveRatePercent = EstimateCurveRatePercent(points, currentLineLength);
            hasCurrentLineShape = true;
        }

        void EnsureLineRenderer()
        {
            if (!lineRenderer)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }
        }

        Vector3[] GetCurrentLinePoints()
        {
            var count = Mathf.Max(2, lineRenderer.positionCount);
            var result = new Vector3[count];
            for (var i = 0; i < count; i++)
            {
                result[i] = i < lineRenderer.positionCount ? lineRenderer.GetPosition(i) : Vector3.zero;
            }
            return result;
        }

        static float CalculatePolylineLength(Vector3[] points)
        {
            var length = 0f;
            for (var i = 1; i < points.Length; i++)
            {
                length += Vector3.Distance(points[i - 1], points[i]);
            }
            return length;
        }

        static float EstimateCurveRatePercent(Vector3[] points, float lineLength)
        {
            if (points.Length < 2 || lineLength <= 0f)
            {
                return 0f;
            }

            var start = points[0];
            var end = points[points.Length - 1];
            var chordLength = Vector3.Distance(start, end);
            if (chordLength <= 0f)
            {
                return 100f;
            }

            var ratio = Mathf.Clamp01(chordLength / lineLength);
            if (ratio >= 0.9999f)
            {
                return 0f;
            }

            var minTheta = 0.0001f;
            var maxTheta = Mathf.PI * 2f;
            for (var i = 0; i < 20; i++)
            {
                var theta = (minTheta + maxTheta) * 0.5f;
                var thetaRatio = 2f * Mathf.Sin(theta * 0.5f) / theta;
                if (thetaRatio > ratio)
                {
                    minTheta = theta;
                }
                else
                {
                    maxTheta = theta;
                }
            }

            var solvedTheta = (minTheta + maxTheta) * 0.5f;
            return Mathf.InverseLerp(0.0001f, Mathf.PI * 2f, solvedTheta) * 100f;
        }
    }
}
