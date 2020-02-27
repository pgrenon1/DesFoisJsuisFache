using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalData
{
    public Vector3 position;
    public Quaternion rotation;
    public string word;

    public DecalData(Vector3 position, Quaternion rotation, string word)
    {
        this.position = position;
        this.rotation = rotation;
        this.word = word;
    }
}

public class DecalCollection
{
    public List<DecalData> decalCollection;

    public DecalCollection(List<DecalData> decalData)
    {
        decalCollection = decalData;
    }
}

public class DecalManager : SingletonMonoBehaviour<DecalManager>
{
    public bool usePrettyPrint = true;
    public List<DecalData> RuntimeDecalData { get; set; } = new List<DecalData>();

    [Button("SaveToJSON", ButtonSizes.Gigantic)]
    public void SaveToJson()
    {
        //var collection = new DecalCollection(RuntimeDecalData);

        var jsonString = JsonHelper.ToJson(RuntimeDecalData.ToArray(), usePrettyPrint);

        Debug.Log(jsonString);
    }
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}