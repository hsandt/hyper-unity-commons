using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

namespace CommonsHelper.DOTween
{

	public static class DOTweenExtensions {

		public static void CutIn (this Graphic graphic) {
			graphic.DOKill();
			graphic.color = graphic.color.ToVisible();
		}

		public static void CutOut (this Graphic graphic) {
			graphic.DOKill();
			graphic.color = graphic.color.ToInvisible();
		}

		/// Fade in from invisible. graphic must start invisible, else will interpolate toward visible color.
		/// Call graphic.DOKill before calling this method if you want to remove all previous tweens immediately
		public static Tweener FadeIn (this Graphic graphic, float duration) {
			// this method should not have any immediate side-effect, only return the Tweener (no DOKill, no direct visibility change)
			return graphic.DOColor(graphic.color.ToVisible(), duration);
		}

		/// Fade out from fully visible. graphic must start visible, else will interpolate toward invisible color.
		/// Call graphic.DOKill before calling this method if you want to remove all previous tweens immediately
		public static Tweener FadeOut (this Graphic graphic, float duration) {
			// this method should not have any immediate side-effect, only return the Tweener (no DOKill, no direct visibility change)
			return graphic.DOColor(graphic.color.ToInvisible(), duration);
		}

		/// Fade to given alpha
		public static Tweener FadeTo (this Graphic graphic, float alpha, float duration) {
			return graphic.DOColor(graphic.color.ToAlpha(alpha), duration);
		}

	}

}
