using MyProject.Core;
using UnityEngine;

namespace MyProject.Infrastructure
{
    [CreateAssetMenu(fileName = "BeatmapListSO", menuName = "MyProject/BeatmapListSO")]
    public class BeatmapListSO : ScriptableObject
    {
        [SerializeField] BeatmapFilesSO demo;
        [SerializeField] BeatmapFilesSO normal;
        [SerializeField] BeatmapFilesSO hard;

        public BeatmapFilesSO Get(BeatmapType type)
        {
            return type switch
            {
                BeatmapType.Demo => demo,
                BeatmapType.Normal => normal,
                BeatmapType.Hard => hard,
                _ => throw new System.ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}
