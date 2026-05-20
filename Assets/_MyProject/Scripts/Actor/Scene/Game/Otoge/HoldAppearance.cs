using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    internal static class HoldAppearance
    {
        const float BeforeBrightness = 0.75f;
        const float HoldingBrightness = 1f;
        const float MissedBrightness = 0.5f;

        public static Color ApplyStateBrightness(Color baseColor, NoteState state)
        {
            var brightness = state switch
            {
                NoteState.BeforeJudge => BeforeBrightness,
                NoteState.Holding => HoldingBrightness,
                NoteState.Missed => MissedBrightness,
                NoteState.Released => MissedBrightness,
                _ => BeforeBrightness
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
