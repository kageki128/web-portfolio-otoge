using System;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Infrastructure
{
    [CreateAssetMenu(fileName = "OtogeChangesSO", menuName = "MyProject/OtogeChangesSO")]
    public class OtogeChangesSO : ScriptableObject
    {
        [Serializable]
        class OtogeChange
        {
            public float Beat => beat;
            [SerializeField] float beat;

            public OtogeType Type => type;
            [SerializeField] OtogeType type;
        }

        public MyProject.Core.OtogeChange[] OtogeChanges => Array.ConvertAll(otogeChanges, x => new MyProject.Core.OtogeChange(x.Beat, x.Type));

        [SerializeField] OtogeChange[] otogeChanges;
    }
}
