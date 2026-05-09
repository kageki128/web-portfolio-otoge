using UnityEngine;

namespace MyProject.Core
{
    public class BeatmapMetaData
    {
        public string Title { get; }
        public string Artist { get; }
        public string NoteDesigners { get; }
        public DifficultyType Difficulty { get; }

        public AudioClip Wave { get; }
        public float WaveOffset { get; }

        public float MaxBpm { get; }
        public float MinBpm { get; }

        public BeatmapMetaData
        (
            string title,
            string artist,
            string noteDesigners,
            DifficultyType difficulty,
            AudioClip wave,
            float waveOffset,
            float maxBpm,
            float minBpm
        )
        {
            Title = title;
            Artist = artist;
            NoteDesigners = noteDesigners;
            Difficulty = difficulty;
            Wave = wave;
            WaveOffset = waveOffset;
            MaxBpm = maxBpm;
            MinBpm = minBpm;
        }
    }
}
