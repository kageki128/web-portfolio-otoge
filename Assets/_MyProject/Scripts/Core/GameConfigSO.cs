using UnityEngine;

namespace MyProject.Core
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "MyProject/GameConfig")]
    public class GameConfigSO : ScriptableObject
    {
        [field: SerializeField]
        public SceneType InitialSceneType { get; private set; } = SceneType.Title;
    }
}
