using System.Collections;
using System.Collections.Generic;
using HyperUnityCommons;
using UnityEngine;

/// This is simply a DontDestroyOnLoad tag that indicates that persistent managers have already been created
/// once for the game, and don't need to be created again. It is a singleton simply to allow PersistentManagersGenerator
/// to find it easily.
/// Note that its creator (PersistentManagersGenerator) will call DontDestroyOnLoad on it.
/// SEO doesn't matter, as it is created via PersistentManagersGenerator.
public class PersistentManagersCreationToken : SingletonManager<PersistentManagersCreationToken>
{
}
