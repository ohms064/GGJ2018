using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class NPCPool : ObjectsPool<NPCPoolObject> {

    private bool isSomeoneBeingConverted = false;
    public AudioMixer attackinMixer;

    public void Update() {
        /*isSomeoneBeingConverted = false;
        foreach(var npc in GetUnavailableObjects()) {
            Debug.Log("av" + npc.IsBeignConverted);
            if (npc.IsBeignConverted) {
                isSomeoneBeingConverted = true;
                break;
            }
        }*/

        //Debug.Log("con" + isSomeoneBeingConverted);
        isSomeoneBeingConverted = false;
        foreach (var npc in GetUnavailableObjects()) {
            Debug.Log("estoyenforeach" + npc.GetComponent<NPCPlayerInteraction>().IsConverting());
            if (npc.GetComponent<NPCPlayerInteraction>().IsConverting()) {
                Debug.Log("enelif" + isSomeoneBeingConverted);
                isSomeoneBeingConverted = true;
                break;
            } 
        }
        Debug.Log("con" + isSomeoneBeingConverted);
        if (isSomeoneBeingConverted) {
            float vol;
            bool x = attackinMixer.GetFloat("attacking", out vol);
            if (vol < 0.0f)
                attackinMixer.SetFloat("attacking", vol + (Time.deltaTime * 10));
        } else {
            float vol;
            bool x = attackinMixer.GetFloat("attacking", out vol);
            if (vol > -40.0f)
                attackinMixer.SetFloat("attacking", vol - (Time.deltaTime * 10));
        }

    }
}
