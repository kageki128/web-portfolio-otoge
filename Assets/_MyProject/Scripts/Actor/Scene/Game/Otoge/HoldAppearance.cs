using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    internal static class HoldAppearance
    {
        const float BeforeAlpha = 0.5f;
        const float HoldingAlpha = 1f;
        const float MissedAlpha = 0.25f;

        public static Color ApplyStateAlpha(Color baseColor, NoteState state)
        {
            baseColor.a = state switch
            {
                NoteState.BeforeJudge => BeforeAlpha,
                NoteState.Holding => HoldingAlpha,
                NoteState.Missed => MissedAlpha,
                NoteState.Released => MissedAlpha,
                _ => BeforeAlpha
            };
            return baseColor;
        }
    }
}
