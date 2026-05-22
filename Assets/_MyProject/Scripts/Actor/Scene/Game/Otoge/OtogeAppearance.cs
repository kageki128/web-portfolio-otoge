using LitMotion;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    internal static class OtogeAppearance
    {
        public const float StateTransitionDuration = 0.4f;
        public const Ease StateTransitionEase = Ease.OutCubic;

        const float HoldBeforeBrightness = 0.75f;
        const float HoldHoldingBrightness = 1f;
        const float HoldMissedBrightness = 0.5f;

        public static Color GetHoldColor(Color baseColor, NoteState state)
        {
            var brightness = state switch
            {
                NoteState.BeforeJudge => HoldBeforeBrightness,
                NoteState.Holding => HoldHoldingBrightness,
                NoteState.Missed => HoldMissedBrightness,
                NoteState.Released => HoldMissedBrightness,
                _ => HoldBeforeBrightness
            };

            return new Color(
                baseColor.r * brightness,
                baseColor.g * brightness,
                baseColor.b * brightness,
                baseColor.a
            );
        }
    }
}
