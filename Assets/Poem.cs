using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ShowOdinSerializedPropertiesInInspector]
public class Poem : OdinSerializedScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField, HideInInspector]
    private SerializationData serializationData;

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        UnitySerializationUtility.DeserializeUnityObject(this, ref this.serializationData);
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        UnitySerializationUtility.SerializeUnityObject(this, ref this.serializationData);
    }

    public string author;
    public string title;
    [TextArea]
    public string poem;
    public List<KeyValuePair<string, AudioClip>> words = new List<KeyValuePair<string, AudioClip>>();
}
