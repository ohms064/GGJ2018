using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

public class Spawner : MonoBehaviour {
    public SpawnPoint[] spawnPositions;
    public NPCPool pool;
    [MinMaxSlider( 1f, 100f, true )]
    public Vector2 randomSpawnTime;
    public float SpawnTime {
        get {
            return Random.Range( randomSpawnTime.x, randomSpawnTime.y );
        }
    }
    public SpawnPoint SpawnPosition {
        get {
            return spawnPositions[Random.Range( 0, spawnPositions.Length )];
        }
    }

#if UNITY_EDITOR
    private void Reset () {
        pool = GetComponent<NPCPool>();
        spawnPositions = GetComponentsInChildren<SpawnPoint>();
    }
#endif

    private IEnumerator Start () {
        NPCPoolObject poolObject;
        for ( int i = 0; i < spawnPositions.Length; i++ ) {
            if ( pool.RequestPoolObject( out poolObject ) ) {
                poolObject.Spawn( spawnPositions[i].Position );
            }
        }

        while ( Application.isPlaying ) {
            yield return new WaitForSeconds( SpawnTime );
            if ( pool.RequestPoolObject( out poolObject ) ) {
                SpawnPosition.Spawn( poolObject );
            }
        }
    }

}

public enum SpawnType {
    ONE_BY_ONE, BURST
}
