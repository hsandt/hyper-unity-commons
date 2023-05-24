using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsPattern;

/// Convenience component to create an ordered pool container of standard pooled object,
/// when the pooled object prefab has no particular component and just uses the StandardPooledObject
public class StandardOrderedPoolContainer : OrderedPoolContainer<StandardPooledObject>
{
}
