using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class PlaySound : MonoBehaviour {

	AudioSource audioSource;

	/* State vars */

	/// Should the audio source resume play on enable?
	bool wasPlaying;

	void Awake () {
		audioSource = GetComponent<AudioSource>();
	}

	void Start () {
		Setup();
	}

	public void Setup () {

	}

	public void Clear () {
		audioSource.Stop();
		wasPlaying = false;
	}

	/// Play clip from start, overriding any other clip playing
	public void PlayClip (AudioClip clip) {
		audioSource.Stop();
		audioSource.clip = clip;
		audioSource.Play();
	}

	void OnEnable () {
		if (wasPlaying) {
			audioSource.Play();
			wasPlaying = false;
		}
	}

	void OnDisable () {
		// FIXME? maybe audio source is not considered to play at t=0, so pause the game just when the sound starts to test
		// if it does not work, track should playing with a bool (same remark for PFX)
		if (audioSource.isPlaying) {
			audioSource.Pause();
			wasPlaying = true;
		}
	}

}
