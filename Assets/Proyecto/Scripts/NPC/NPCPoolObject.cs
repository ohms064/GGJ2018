using UnityEngine;
using System.Collections;

public class NPCPoolObject : PoolObject {

    private bool isBeignConverted;
    private NPCPlayerInteraction npcPlayerInteraction;

    public bool IsBeignConverted
    {
        get
        {
            return isBeignConverted;
        }

        set
        {
            isBeignConverted = value;
        }
    }

    private void OnEnable() {
        npcPlayerInteraction = GetComponent<NPCPlayerInteraction>();
    }

}
