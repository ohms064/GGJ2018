using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MixLevels : MonoBehaviour {

    public AudioMixer masterMixer;

    public void SetSFXVol(float sfxVol) {
        masterMixer.SetFloat("sfxVol", sfxVol);
    }

    public void SetMusicVol(float musicVol) {
        masterMixer.SetFloat("musicVol", musicVol);
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
