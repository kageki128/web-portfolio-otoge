using UnityEngine;

namespace MyProject.Core
{
    [CreateAssetMenu(fileName = "MusicInfoSO", menuName = "MyProject/MusicInfoSO")]
    public class MusicInfoSO : ScriptableObject
    {
        [field: SerializeField]
        public AudioClip Wave { get; private set; }

        [field: SerializeField]
        public TextAsset Beatmap { get; private set; }
    }
}
