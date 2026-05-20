using UnityEngine;

namespace MyProject.Core
{
    [CreateAssetMenu(fileName = "OtogeChangesSO", menuName = "MyProject/OtogeChangesSO")]
    public class OtogeChangesSO : ScriptableObject
    {
        public OtogeChange[] OtogeChanges => otogeChanges;
        [SerializeField] OtogeChange[] otogeChanges;
    }
}
