using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ObjectsPool<T> : MonoBehaviour where T : PoolObject {

    private T PoolObject {
        get {
            return poolObjects[Random.Range( 0, poolObjects.Length )];
        }
    }
    [AssetsOnly]
    public T[] poolObjects;

    [ValidateInput( "ValidatePoolSize" )]
    public int poolSize = 1;
    private T[] pool;
    private int iterator = 0;

#if UNITY_EDITOR
    private bool ValidatePoolSize (int i) {
        return i > 0;
    }
#endif


    public IEnumerable<T> GetUnavailableObjects () {
        for ( int i = 0; i < pool.Length; i++ ) {
            if ( !pool[i].Available )
                yield return pool[i];
        }
    }

    public IEnumerable<T> GetAvailableObjects () {
        for ( int i = 0; i < pool.Length; i++ ) {
            if ( pool[i].Available )
                yield return pool[i];
        }
    }

    private void Awake () {
        pool = new T[poolSize];
        for ( int i = 0; i < poolSize; i++ ) {
            pool[i] = Instantiate( PoolObject );
            pool[i].gameObject.SetActive( false );
        }
    }

    public bool RequestPoolObject (out T poolObject) {
        if ( iterator >= pool.Length ) {
            iterator = 0;
        }
        poolObject = pool[iterator];
        iterator++;
        return poolObject.Available;
    }
}

public abstract class PoolObject : MonoBehaviour {
    public virtual bool Available {
        get {
            return !gameObject.activeSelf;
        }
    }
    public Vector2 position {
        get {
            return transform.position;
        }
    }

    public virtual void Spawn (Vector3 position) {
        gameObject.SetActive( true );
        transform.position = position;
    }

    public virtual void Despawn () {
        gameObject.SetActive( false );
    }
}
