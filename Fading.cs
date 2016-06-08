using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using DG.Tweening;

[RequireComponent(typeof(Image))]
public class Fading : MonoBehaviour {

	Image image;

	void Awake () {
		image = GetComponent<Image>();
	}

	public void CutIn () {
		image.DOKill();
		image.color = image.color.ToVisible();
	}

	public void CutOut () {
		image.DOKill();
		image.color = image.color.ToInvisible();
	}

	/// Fade in from invisible
	public Tweener FadeIn (float duration) {
		// cancel any previous tween
		image.DOKill();

		// set black screen to be fully invisible (in case it was not in this state already)
		image.color = image.color.ToInvisible();
		return image.DOColor(image.color.ToVisible(), duration);
	}

	/// Fade out from fully visible
	public Tweener FadeOut (float duration) {
		// cancel any previous tween
		image.DOKill();

		// set black screen to be fully invisible (in case it was not in this state already)
		image.color = image.color.ToVisible();
		return image.DOColor(image.color.ToInvisible(), duration);
	}

}
