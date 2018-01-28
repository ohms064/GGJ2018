using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

public class SpawnPoint : MonoBehaviour {
    public SpawnType spawnType;
    [ShowIf( "BurstIntervalCondition" )]
    public float burstIntervalTime;
    [ShowIf( "BurstIntervalCondition" )]
    public int burstCount = 5;
#if UNITY_EDITOR
    public bool BurstIntervalCondition {
        get {
            return spawnType == SpawnType.BURST;
        }
    }
#endif

    public Vector3 Position {
        get {
            return transform.localPosition;
        }
    }

    public void Spawn<T>(T poolObject) where T : PoolObject {
        switch ( spawnType ) {
            case SpawnType.ONE_BY_ONE:
                poolObject.Spawn( Position );
                break;
            case SpawnType.BURST:
                for ( int i = 0; i < burstCount; i++ ) {
                    poolObject.Spawn( Position );
                }
                break;
        }

    }
}
