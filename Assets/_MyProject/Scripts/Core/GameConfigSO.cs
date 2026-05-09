using UnityEngine;

namespace MyProject.Core
{
    [CreateAssetMenu(fileName = "GameConfigSO", menuName = "MyProject/GameConfigSO")]
    public class GameConfigSO : ScriptableObject
    {
        [field: SerializeField]
        public SceneType InitialSceneType { get; private set; } = SceneType.Select;
    }
}
