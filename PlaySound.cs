using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class PlaySound : MonoBehaviour {

	AudioSource audioSource;

	void Awake () {
		audioSource = GetComponent<AudioSource>();
	}

	/// Play clip from start, even if it was already playing
	public void PlayClip (AudioClip clip) {
		audioSource.Stop();
		audioSource.clip = clip;
		audioSource.Play();
	}

}
