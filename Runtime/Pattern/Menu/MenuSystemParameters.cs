using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

using HyperUnityCommons;

/// Main Menu parameters
[CreateAssetMenu(fileName = "MenuSystemParameters", menuName = "Menu/Menu System Parameters")]
public class MenuSystemParameters : ScriptableObject
{
    [Header("Audio assets")]

    [Tooltip("BGM played over all menus in this system")]
    public AudioAssetWrapper bgmWrapper;

    [Tooltip("OLD BGM, use bgmWrapper instead")]
    [FormerlySerializedAs("bgm")]
    public AudioClip OLD_bgm;

    [Tooltip("SFX played when changing selection")]
    public AudioClip sfxUISelect;

    [Tooltip("SFX played on non-Back button confirm (submit or click)")]
    public AudioClip sfxUIConfirm;

    [Tooltip("SFX played on Back button confirm or Cancel input")]
    public AudioClip sfxUICancel;
}
