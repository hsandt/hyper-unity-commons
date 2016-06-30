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

	/// Fade in from invisible. Image must start visible, else will interpolate toward invisible color.
	/// Call image.DOKill before calling this method if you want to remove all previous tweens immediately
	public Tweener FadeIn (float duration) {
		// this method should not have any immediate side-effect, only return the Tweener (no DOKill, no direct visibility change)
		return image.DOColor(image.color.ToVisible(), duration);
	}

	/// Fade out from fully visible (call DOKill manually, since this function will be called immediately even in a sequence)
	public Tweener FadeOut (float duration) {
		// this method should not have any immediate side-effect, only return the Tweener (no DOKill, no direct visibility change)
		return image.DOColor(image.color.ToInvisible(), duration);
	}

	/// Fade to given alpha
	public Tweener FadeTo (float alpha, float duration) {
		return image.DOColor(image.color.ToAlpha(alpha), duration);
	}

}
