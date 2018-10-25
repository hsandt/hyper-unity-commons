// Xelnath on https://answers.unity.com/questions/44848/how-to-draw-debug-text-into-scene.html

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsPattern;

namespace CommonsDebug
{

	/// Manager to display debug labels
	/// Tag as EditorOnly
	public class DebugLabelManager : SingletonManager<DebugLabelManager> {
	#if UNITY_EDITOR

	    // REFACTOR: create a PureObjectPoolDataManager similar to PoolManager, and the corresponding PooledPureObject<T>

	    enum PooledTimedObjectState {
	        ShouldStart,
	        Active,
	        Inactive
	    }

	    class PooledLabel {
	        /// Is the pooled object used?
	        public PooledTimedObjectState state = PooledTimedObjectState.Inactive;
	        public float endTime;

	        public LabelData pooledObject;
	    }

	    struct LabelData {
	        public Vector3 position;
	        public string text;
	        public Color color;
	        public float duration;

	        public LabelData (Vector3 position, string text, Color color, float duration) {
	            this.position = position;
	            this.text = text;
	            this.color = color;
	            this.duration = duration;
	        }
	    }


	    /* Parameters */

	    [SerializeField]
	    int poolSize = 5;


	    /* State vars */

		private readonly GUIStyle guiStyle = new GUIStyle();

	    /// Pool of label data (LabelData is a pure object, so we don't use PoolManager)
	    List<PooledLabel> m_LabelDataPool = new List<PooledLabel>();


	    void Awake () {
	        SetInstanceOrSelfDestruct(this);
	        Init();
	    }

	    void Init () {
	        for (int i = 0; i < poolSize; i++) {
	            m_LabelDataPool.Add(new PooledLabel());
	        }
	    }

	    PooledLabel GetObject () {
	        // O(n)
	        for (int i = 0; i < poolSize; ++i) {
	            PooledLabel pooledLabelData = m_LabelDataPool[i];
	            if (pooledLabelData.state == PooledTimedObjectState.Inactive) {
	                return pooledLabelData;
	            }
	        }

	        // check for old objects here?

	        // starvation
	        PooledLabel newLabelData = new PooledLabel();
	        m_LabelDataPool.Add(newLabelData);
	        return newLabelData;
	    }

	    void OnDrawGizmos()
	    {
	        // byref locals and returns not available in C# 4, so use for instead of foreach(ref ...) as LabelData is a struct
	        // REFACTOR: track active objects
	        foreach (PooledLabel pooledLabelData in m_LabelDataPool)
	        {
	            if (pooledLabelData.state == PooledTimedObjectState.Inactive)
	                continue;

	            if (pooledLabelData.state == PooledTimedObjectState.Active)
	            {
	                if (pooledLabelData.endTime < Time.time) {
	                    pooledLabelData.state = PooledTimedObjectState.Inactive;
	                    continue;
	                }
	            }

	            LabelData labelData = pooledLabelData.pooledObject;

	            if (pooledLabelData.state == PooledTimedObjectState.ShouldStart)
	            {
	                // Start drawing the label and compute the end time from this frame (the 1st Update since DrawText,
	                // including Editor Update).
	                // This allows us not to rely on the time at which DrawText is called, which may be
	                // inside FixedUpdate and therefore before the next Update, causing 0 duration labels
	                // to immediately disappear.
	                pooledLabelData.state = PooledTimedObjectState.Active;
	                pooledLabelData.endTime = Time.time + labelData.duration;
	            }

	            guiStyle.normal.textColor = labelData.color;
	            UnityEditor.Handles.Label(labelData.position, labelData.text, guiStyle);
	        }
	    }

	    public void DrawText(Vector3 position, string text, Color color, float duration)
	    {
	        // Prepare label data and set up pooled label to start
	        PooledLabel freeLabelData = GetObject();
	        freeLabelData.pooledObject = new LabelData(position, text, color, duration);
	        freeLabelData.state = PooledTimedObjectState.ShouldStart;
	    }

	#endif
	}

}
