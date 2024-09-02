using Photon.Deterministic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Quantum
{
    public class ProjectileConfig : AssetObject
    {
        [FormerlySerializedAs("ProjectileInitialSpeed")] 
        public FP projectileInitialSpeed = 7;

        [FormerlySerializedAs("ProjectileTTL")] 
        public FP projectileTtl = 1;
    }
}
