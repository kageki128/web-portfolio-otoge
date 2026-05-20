using UnityEngine;

namespace MyProject.Infrastructure
{
    [CreateAssetMenu(fileName = "BeatmapFilesSO", menuName = "MyProject/BeatmapFilesSO")]
    public class BeatmapFilesSO : ScriptableObject
    {
        [field: SerializeField]
        public AudioClip Wave { get; private set; }

        [field: SerializeField]
        public TextAsset Beatmap { get; private set; }

        [field: SerializeField]
        public OtogeChangesSO OtogeChanges { get; private set; }
    }
}
