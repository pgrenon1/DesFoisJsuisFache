using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poem : OdinSerializedScriptableObject
{
    public string author;
    public string title;
    public string poem;
    public List<KeyValuePair<string, AudioClip>> words = new List<KeyValuePair<string, AudioClip>>();
}
