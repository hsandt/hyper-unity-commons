// Dependency: Commons/Helper/Extensions.cs

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Commons.Debug
{
    
	// SEO: before DebugScreenManager because the pool of initial messages is created in its Awake
	// and uses references initialized in DebugText.Awake()
	// (or initialize pool in Start())
	[RequireComponent(typeof (Text))]
	public class DebugText : MonoBehaviour
	{
	    // reference to UI text
	    Text m_Text;
	    public Text text { get { return m_Text; } }

	    // // format of the string to display
	    // [SerializeField]
	    // string m_FormatString;

		/// Time it takes to fade color if the text is not updated
		const float colorChangeTime = 2f;

	    /// Channel index used by this entry
	    public int channelIndex;

	    /// Time passed since last update
	    float timeSinceLastUpdate;

	    // how much time remains before text destruction
	    float m_RemainingTime;

	    void Init () {
	        m_Text = this.GetComponentOrFail<Text>();
	        // TODO: pool the debug texts
	    }

	    void Awake () {
	        Init();
	    }

	    void Start () {
	        Setup();
	    }

	    public void Setup () {
	    }

	    void Update () {
			timeSinceLastUpdate += Time.deltaTime;
	        UpdateColor();

	        m_RemainingTime -= Time.deltaTime;
	        if (m_RemainingTime <= 0) {
	            m_RemainingTime = 0;
	            gameObject.SetActive(false);
	        }
	    }

	    /// Update the color based on the current timeSinceLastUpdate
	    void UpdateColor () {
			m_Text.color = Color.Lerp(Color.white, Color.grey, timeSinceLastUpdate / colorChangeTime);
		}

	    /// Show text or duration in sec
	    public void Show (string text, float duration = 1f) {
	        // make text visible
	        gameObject.SetActive(true);
	        // update text content
	        UpdateText(text);
	        // set timer before it disappears again
	        m_RemainingTime = duration;
	    }

	    public void UpdateText(string text)
	    {
	        m_Text.text = text;
	        timeSinceLastUpdate = 0f;
			UpdateColor();
	    }

	    public void Hide () {
	        gameObject.SetActive(false);
	        timeSinceLastUpdate = 0f;
	    }

	    /// Is the object currently used? It cannot be requested if true.
	    public bool IsInUse() {
	        return gameObject.activeSelf;
	    }
	}

}
