using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ClipCollection", menuName = "Clip Collection")]
public class ClipCollection : OdinSerializedScriptableObject
{
    public Dictionary<string, AudioClip> allClips = new Dictionary<string, AudioClip>();
}
