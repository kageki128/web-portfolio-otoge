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

        public override void Initialize()
        {
            lineRenderer = GetComponent<LineRenderer>();
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
            lineRenderer ??= GetComponent<LineRenderer>();

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

        public override async UniTask SetStateAsync(OtogeType otogeType, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lineRenderer ??= GetComponent<LineRenderer>();

            var lineSettings = Array.Find(otogeJudgeLineSettings, x => x.Type == otogeType);
            var targetPosition = lineSettings?.LocalPosition ?? transform.localPosition;
            var targetEuler = lineSettings?.LocalEulerAngles ?? transform.localEulerAngles;
            var currentPoints = GetCurrentLinePoints();
            var targetPoints = lineSettings == null
                ? currentPoints
                : CreateTargetPoints(lineSettings);
            var targetWidth = lineSettings == null ? 0f : lineSettings.LineWidth;

            positionHandle.TryCancel();
            rotationHandle.TryCancel();
            lineShapeHandle.TryCancel();
            widthHandle.TryCancel();

            gameObject.SetActive(true);

            positionHandle = LMotion.Create(transform.localPosition, targetPosition, StateTransitionDuration)
                .WithEase(StateTransitionEase)
                .Bind(value => transform.localPosition = value)
                .AddTo(this);

            rotationHandle = LMotion.Create(transform.localRotation, Quaternion.Euler(targetEuler), StateTransitionDuration)
                .WithEase(StateTransitionEase)
                .Bind(value => transform.localRotation = value)
                .AddTo(this);

            lineShapeHandle = LMotion.Create(0f, 1f, StateTransitionDuration)
                .WithEase(StateTransitionEase)
                .Bind(progress => ApplyLineShape(currentPoints, targetPoints, progress))
                .AddTo(this);

            widthHandle = LMotion.Create(lineRenderer.widthMultiplier, targetWidth, StateTransitionDuration)
                .WithEase(StateTransitionEase)
                .Bind(value => lineRenderer.widthMultiplier = value)
                .AddTo(this);

            await UniTask.WhenAll
            (
                positionHandle.ToUniTask(CancelBehavior.Cancel, false, cancellationToken),
                rotationHandle.ToUniTask(CancelBehavior.Cancel, false, cancellationToken),
                lineShapeHandle.ToUniTask(CancelBehavior.Cancel, false, cancellationToken),
                widthHandle.ToUniTask(CancelBehavior.Cancel, false, cancellationToken)
            );
        }

        Vector3[] CreateTargetPoints(OtogeJudgeLineSettings lineSettings)
        {
            if (lineSettings == null)
            {
                return new[] { Vector3.zero, Vector3.zero };
            }

            var curveRate = Mathf.Clamp01(lineSettings.CurveRatePercent / 100f);
            if (curveRate == 0f)
            {
                var halfLength = lineSettings.LineLength / 2f;
                return new[]
                {
                    new Vector3(-halfLength, 0f, 0f),
                    new Vector3(halfLength, 0f, 0f)
                };
            }

            var arcAngle = Mathf.Lerp(0.0001f, Mathf.PI * 2f, curveRate);
            var radius = lineSettings.LineLength / arcAngle;
            var halfArcAngle = arcAngle / 2f;
            var points = Mathf.Max(2, curveSegments);
            var results = new Vector3[points + 1];

            for (var i = 0; i <= points; i++)
            {
                var t = i / (float)points;
                var angle = Mathf.Lerp(-halfArcAngle, halfArcAngle, t);
                var x = radius * Mathf.Sin(angle);
                var y = radius - radius * Mathf.Cos(angle);
                results[i] = new Vector3(x, y, 0f);
            }

            return results;
        }

        Vector3[] GetCurrentLinePoints()
        {
            var count = Mathf.Max(2, lineRenderer.positionCount);
            var points = new Vector3[count];
            for (var i = 0; i < count; i++)
            {
                points[i] = i < lineRenderer.positionCount ? lineRenderer.GetPosition(i) : Vector3.zero;
            }
            return points;
        }

        void ApplyLineShape(Vector3[] fromPoints, Vector3[] toPoints, float progress)
        {
            var count = Mathf.Max(fromPoints.Length, toPoints.Length);
            lineRenderer.positionCount = count;

            for (var i = 0; i < count; i++)
            {
                var from = SamplePoint(fromPoints, i, count);
                var to = SamplePoint(toPoints, i, count);
                lineRenderer.SetPosition(i, Vector3.Lerp(from, to, progress));
            }
        }

        static Vector3 SamplePoint(Vector3[] points, int index, int targetCount)
        {
            if (points.Length == 0)
            {
                return Vector3.zero;
            }

            if (points.Length == 1 || targetCount <= 1)
            {
                return points[0];
            }

            var normalizedIndex = index / (float)(targetCount - 1);
            var sourcePosition = normalizedIndex * (points.Length - 1);
            var fromIndex = Mathf.FloorToInt(sourcePosition);
            var toIndex = Mathf.Min(fromIndex + 1, points.Length - 1);
            var t = sourcePosition - fromIndex;
            return Vector3.Lerp(points[fromIndex], points[toIndex], t);
        }
    }
}
