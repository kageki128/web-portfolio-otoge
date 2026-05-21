using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
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

            public float LineWidth => lineWidth;
            [SerializeField, Min(0f)] float lineWidth = 0.02f;
        }

        [SerializeField] OtogeLineSettings[] otogeLineSettings;

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

            var lineSettings = Array.Find(otogeLineSettings, x => x.Type == otogeType);
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

            var direction = lineRenderer.positionCount >= 2
                ? lineRenderer.GetPosition(1) - lineRenderer.GetPosition(0)
                : Vector3.up;
            if (direction == Vector3.zero) direction = Vector3.up;

            lineRenderer.positionCount = 2;
            var half = direction.normalized * (lineSettings.LineLength / 2f);
            lineRenderer.SetPosition(0, -half);
            lineRenderer.SetPosition(1, half);
        }

        public override async UniTask SetStateAsync(OtogeType otogeType, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lineRenderer ??= GetComponent<LineRenderer>();

            var lineSettings = Array.Find(otogeLineSettings, x => x.Type == otogeType);
            var targetPosition = lineSettings?.LocalPosition ?? transform.localPosition;
            var targetEuler = lineSettings?.LocalEulerAngles ?? transform.localEulerAngles;
            var currentLinePoints = GetCurrentLinePoints();
            var targetLinePoints = lineSettings == null
                ? currentLinePoints
                : CreateTargetLinePoints(lineSettings);
            var targetWidth = lineSettings == null ? 0f : lineSettings.LineWidth;

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
                .Bind(progress => ApplyLineShape(currentLinePoints, targetLinePoints, progress))
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
        }

        Vector3[] CreateTargetLinePoints(OtogeLineSettings lineSettings)
        {
            if (lineSettings == null)
            {
                return new[] { Vector3.zero, Vector3.zero };
            }

            var direction = lineRenderer.positionCount >= 2
                ? lineRenderer.GetPosition(1) - lineRenderer.GetPosition(0)
                : Vector3.up;
            if (direction == Vector3.zero) direction = Vector3.up;

            var half = direction.normalized * (lineSettings.LineLength / 2f);
            return new[] { -half, half };
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
