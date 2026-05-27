using System;
using UnityEngine;

namespace MyProject.Infrastructure
{
    [CreateAssetMenu(fileName = "OtogeEventsSO", menuName = "MyProject/OtogeEventsSO")]
    public class OtogeEventsSO : ScriptableObject
    {
        public float[] OtogeEventBeats
        {
            get
            {
                if (otogeEventBeats == null)
                {
                    throw new InvalidOperationException("OtogeEventsSO.otogeEventBeats is not assigned.");
                }

                return otogeEventBeats;
            }
        }

        [SerializeField] float[] otogeEventBeats;
    }
}
