// Spatial labels are based on Xelnath's answer on https://answers.unity.com/questions/44848/how-to-draw-debug-text-into-scene.html
// and use Handles.Label, while flat UI labels use GUI.Label as in most answers
// UI labels are inspired by UE4's visual logging system, and replace DebugScreenManager which is using the expensive Unity UI Text
// Known issue when using PrintText/DrawUIText:
// ArgumentException: Getting control N's position in a group with only N controls when doing repaint
// Aborting
//  UnityEngine.GUILayoutGroup.GetNext
//  UnityEngine.GUILayoutUtility.DoGetRect
//  UnityEngine.GUILayoutUtility.GetRect
//  UnityEngine.GUILayout.DoLabel
//  UnityEngine.GUILayout.Label
//  HyperUnityCommons.DebugLabelManager.OnGUI
//  UnityEngine.GUIUtility:ProcessEvent

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsPattern;

namespace CommonsDebug
{

	/// Manager to display debug labels
	
	/// UI labels are visible in standalone builds, but if you want to remove them from the build,
	/// just make the the DebugLabelManager game object tagged EditorOnly (UI label code will not be stripped though)

	/// Spatial labels rely on Handles, so they are Editor only. However, Handles.Label ultimately uses GUI.Label with
	/// WorldToScreenPoint, so you we could put them in standalone Build too by decompiling an inlining Handles.Label
	public class DebugLabelManager : SingletonManager<DebugLabelManager> {

		#region ProxyMethods

		public static void Print (string text, Color? color = null, float duration = 2f, int channel = -1)
		{
			if (Instance != null)
			{
				Instance.DrawUIText(text, color ?? Color.white, duration, channel);
			}
		}
		
		public static void Print (FormattableString formattableText, Color? color = null, float duration = 2f, int channel = -1)
		{
			if (Instance != null)
			{
				// Invariant allows to avoid ',' instead of '.' depending on system language (unfortunately, doesn't work with Vectors, consider custom ToString)
				Instance.DrawUIText(FormattableString.Invariant(formattableText), color ?? Color.white, duration, channel);
			}
		}
		
#if UNITY_EDITOR

		public static void Print3D (Vector3 position, string text, Color? color = null, float duration = 2f)
		{
			if (Instance != null)
			{
				Instance.DrawText(position, text, color ?? Color.white, duration);
			}
		}
		public static void Print3D (Vector3 position, FormattableString formattableText, Color? color = null, float duration = 2f)
		{
			if (Instance != null)
			{
				// Invariant allows to avoid ',' instead of '.' depending on system language (unfortunately, doesn't work with Vectors, consider custom ToString)
				Instance.DrawText(position, FormattableString.Invariant(formattableText), color ?? Color.white, duration);
			}
		}
		
#endif
		
		#endregion

		enum PooledTimedObjectState {
			ShouldStart,
			Active,
			Inactive
		}
		
		/// UI debug label, displayed on screen
		private class LabelData
		{
			public string text;
			public Color color;
			public float duration;
			
			public void SetParams (string text, Color color, float duration) {
				this.text = text;
				this.color = color;
				this.duration = duration;
			}
		}
		
#if UNITY_EDITOR
		/// Handle debug label, displayed in space
		private class SpatialLabelData : LabelData {
			public Vector3 position;

			public void SetParams (Vector3 position, string text, Color color, float duration)
			{
				base.SetParams(text, color, duration);
				this.position = position;
			}
		}
#endif
		
		private class PooledLabel<TLabelData> where TLabelData : LabelData, new() {
			/// Is the pooled object used?
			public PooledTimedObjectState state = PooledTimedObjectState.Inactive;
			public float endTime;

			public TLabelData pooledObject = new TLabelData();
		}


		/* Parameters */
		
		[Header("UI Labels")]
		
		[SerializeField, Tooltip("Size of the pool of UI labels")]
		private int uiLabelPoolSize = 10;

		[SerializeField, Tooltip("Size of the font used to draw UI labels")]
		private int uiLabelFontSize = 24;
		
		[SerializeField, Tooltip("Width of the outline of UI labels. If 0, do not draw outline. " +
								 "Caution: this quickly increases CPU usage.")]
		private int uiLabelOutlineWidth = 0;
		
		[Header("Spatial Labels")]

#if UNITY_EDITOR
		[SerializeField]
		private int spatialLabelPoolSize = 5;
#endif

		/* State vars */

		private readonly GUIStyle guiStyle = new GUIStyle();

		// REFACTOR: create an ObjectPool for non-MonoBehaviour pooled objects (not even a SingletonManager)

		/// Pool of label data (LabelData is a pure object, so we don't use PoolManager)
		List<PooledLabel<LabelData>> m_UILabelDataPool = new List<PooledLabel<LabelData>>();
		
#if UNITY_EDITOR
		/// Pool of spatial label data (LabelData is a pure object, so we don't use PoolManager)
		List<PooledLabel<SpatialLabelData>> m_SpatialLabelDataPool = new List<PooledLabel<SpatialLabelData>>();
#endif

		protected override void Init()
		{
			for (int i = 0; i < uiLabelPoolSize; i++) {
				m_UILabelDataPool.Add(new PooledLabel<LabelData>());
			}
#if UNITY_EDITOR
			for (int i = 0; i < spatialLabelPoolSize; i++) {
				m_SpatialLabelDataPool.Add(new PooledLabel<SpatialLabelData>());
			}
#endif
		}

		/// Get first free pooled label. Guaranteed to return non-null reference.
		PooledLabel<TLabelData> GetObject<TLabelData> (List<PooledLabel<TLabelData>> pool)
			where TLabelData : LabelData, new() {
			// O(n)
			for (int i = 0; i < uiLabelPoolSize; ++i) {
				PooledLabel<TLabelData> pooledLabelData = pool[i];
				if (pooledLabelData.state == PooledTimedObjectState.Inactive) {
					return pooledLabelData;
				}
			}

			// starvation -> there is no max, so create new label at will
			PooledLabel<TLabelData> newLabelData = new PooledLabel<TLabelData>();
			pool.Add(newLabelData);
			return newLabelData;
		}

		private void OnGUI()
		{
			foreach (PooledLabel<LabelData> pooledUILabelData in m_UILabelDataPool)
			{
				// 2-state FSM
				if (pooledUILabelData.state == PooledTimedObjectState.Inactive)
					continue;
				
				// always draw before checking time, so that labels drawn with time 0
				// are displayed at least 1 frame (else they would be ignored when drawn in FixedUpdate)
				LabelData uiLabelData = pooledUILabelData.pooledObject;
				guiStyle.fontSize = uiLabelFontSize;

				if (uiLabelOutlineWidth > 0)
				{
					GUILayoutLabelWithOutline(uiLabelData.text, guiStyle, null, Color.black, uiLabelData.color, uiLabelOutlineWidth);
				}
				else
				{
					guiStyle.normal.textColor = uiLabelData.color;
					GUILayout.Label(uiLabelData.text, guiStyle);
				}

				if (pooledUILabelData.endTime < Time.time)
				{
					pooledUILabelData.state = PooledTimedObjectState.Inactive;
				}
			}
		}

		/// Proxy method for GUILayoutLabelWithOutline with GUIContent, allowing to pass text directly
		void GUILayoutLabelWithOutline(string text, GUIStyle style, GUILayoutOption[] options, Color borderColor, Color innerColor, int borderWidth)
		{
			GUIContent content = new GUIContent(text);
			GUILayoutLabelWithOutline(content, style, options, borderColor, innerColor, borderWidth);
		}

		/// Bridge method between GUILayout.Label and GUILabelWithOutline to apply stamping technique to layout text
		void GUILayoutLabelWithOutline(GUIContent content, GUIStyle style, GUILayoutOption[] options, Color borderColor, Color innerColor, int borderWidth)
		{
			// I have inlined GUILayout.DoLabel in order to inject the outline version of GUI.Label
			GUILabelWithOutline(GUILayoutUtility.GetRect(content, style, options), content, style, borderColor, innerColor, borderWidth);
		}

		/// Stamp text multiple times around (with borderColor) to simulate an outline
		/// Complexity: O(borderWidth)
		/// I mixed two answers:
		/// - DrawOutline http://answers.unity.com/answers/703629/view.html by Praesidium
		/// - DrawTextWithOutline in http://answers.unity.com/answers/1386982/view.html by nateonguitar
		/// Slightly modified to receive GUIContent instead of text (but usage makes sense with GUIContent containing text anyway)
		void GUILabelWithOutline(Rect centerRect, GUIContent content, GUIStyle style, Color borderColor, Color innerColor, int borderWidth)
		{
			// performance check if case we don't need outline at all
			if (borderWidth > 0)
			{
				// assign the border color
				style.normal.textColor = borderColor;
				
				// draw the left and right edges, including in the corners
				for (int j = - borderWidth; j <= borderWidth; j++)
				{
					// left edge
					GUI.Label(new Rect(centerRect.x - borderWidth,centerRect.y + j, centerRect.width, centerRect.height), content, style);
					// right edge
					GUI.Label(new Rect(centerRect.x + borderWidth,centerRect.y + j, centerRect.width, centerRect.height), content, style);
				}
				
				// draw the top of bottom edges, but spare the corners this time as they have been done above
				for (int i = - borderWidth + 1; i <= borderWidth - 1; i++)
				{
					// top edge
					GUI.Label(new Rect(centerRect.x + i,centerRect.y - borderWidth, centerRect.width, centerRect.height), content, style);
					// bottom edge
					GUI.Label(new Rect(centerRect.x + i,centerRect.y + borderWidth, centerRect.width, centerRect.height), content, style);
				}
			}
			
			// draw the inner color version in the center (it overwrites the style color)
			style.normal.textColor = innerColor;
			GUI.Label(centerRect, content, style);
		}
		
		#region UIlabels

		public void DrawUIText(string text, Color color, float duration = 2f, int channel = -1)
		{
			PooledLabel<LabelData> pooledlabelData;
			if (channel == -1)
			{
				pooledlabelData = GetObject(m_UILabelDataPool);
			}
			else
			{
				// Limitation: does not support manual channel if not created previously
				// whereas -1 will effectively add new channels and increase the pool, which is inconsistent
				// We could add channels until reaching the passed index, but that would create 100 channels
				// when we pass 100, which UE4 can do without any problem. Prefer a mapping that supports "holes".
				Debug.AssertFormat(channel >= 0 && channel < m_UILabelDataPool.Count,
					"channel {0} is an invalid index for m_UILabelDataPool of size {1}", channel, m_UILabelDataPool.Count);
				pooledlabelData = m_UILabelDataPool[channel];
			}
			// for UI label, we experiment a more simple 2-state FSM: skip ShouldStart and set endTime now
			pooledlabelData.pooledObject.SetParams(text, color, duration);
			pooledlabelData.state = PooledTimedObjectState.Active;
			pooledlabelData.endTime = Time.time + duration;
		}
		
		#endregion


		#region SpatialLabels

#if UNITY_EDITOR
		void OnDrawGizmos()
		{
			// byref locals and returns not available in C# 4, so use for instead of foreach(ref ...) as LabelData is a struct
			// REFACTOR: track active objects
			foreach (PooledLabel<SpatialLabelData> pooledSpatialLabelData in m_SpatialLabelDataPool)
			{
				// Spatial labels use a 3-state FSM, but we'll probably switch to a 2-state FSM like UI labels
				if (pooledSpatialLabelData.state == PooledTimedObjectState.Inactive)
					continue;

				if (pooledSpatialLabelData.state == PooledTimedObjectState.Active)
				{
					if (pooledSpatialLabelData.endTime < Time.time) {
						pooledSpatialLabelData.state = PooledTimedObjectState.Inactive;
						continue;
					}
				}

				SpatialLabelData spatialLabelData = pooledSpatialLabelData.pooledObject;

				if (pooledSpatialLabelData.state == PooledTimedObjectState.ShouldStart)
				{
					// Start drawing the label and compute the end time from this frame (the 1st Update since DrawText,
					// including Editor Update).
					// This allows us not to rely on the time at which DrawText is called, which may be
					// inside FixedUpdate and therefore before the next Update, causing 0 duration labels
					// to immediately disappear.
					pooledSpatialLabelData.state = PooledTimedObjectState.Active;
					pooledSpatialLabelData.endTime = Time.time + spatialLabelData.duration;
				}

				guiStyle.normal.textColor = spatialLabelData.color;
				UnityEditor.Handles.Label(spatialLabelData.position, spatialLabelData.text, guiStyle);
			}
		}

		public void DrawText(Vector3 position, string text, Color color, float duration)
		{
			// Prepare label data and set up pooled label to start
			PooledLabel<SpatialLabelData> freeLabelData = GetObject(m_SpatialLabelDataPool);
			freeLabelData.pooledObject.SetParams(position, text, color, duration);
			freeLabelData.state = PooledTimedObjectState.ShouldStart;
		}
#endif
		
		#endregion
	}

}