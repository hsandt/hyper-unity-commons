// Dependency: Commons/Helper/Extensions.cs

using System;
using UnityEngine;
using UnityEngine.UI;

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

    // display channel: printing on an existing channel replaces the previous text
    string channel;

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
        m_RemainingTime -= Time.deltaTime;
        if (m_RemainingTime <= 0) {
            m_RemainingTime = 0;
            gameObject.SetActive(false);
        }
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
    }

    public void Hide () {
        gameObject.SetActive(false);
    }

    /// Is the object currently used? It cannot be requested if true.
    public bool IsInUse() {
        return gameObject.activeSelf;
    }
}
