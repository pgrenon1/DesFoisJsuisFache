using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

[Serializable]
public class PoemData
{
    public string author;
    public string title;
    public string poem;
}

[Serializable]
public class PoemDataCollection
{
    public PoemData[] collection;
}

[Serializable]
public class TTSWriter : OdinSerializedBaseBehaviour
{
    public string fileName;
    public ClipCollection clipCollection;
    private SpeechManager _speechManager;

    private void Start()
    {
        _speechManager = GetComponent<SpeechManager>();
        var jsonString = Resources.Load<TextAsset>(fileName);
        Debug.Log(jsonString.text);
        var data = JsonUtility.FromJson<PoemDataCollection>(jsonString.text);

        StartCoroutine(CreateAssets(data));
    }

    private IEnumerator CreateAssets(PoemDataCollection data)
    {
        foreach (var poemData in data.collection)
        {
            var asset = new Poem();
            asset.author = poemData.author;
            asset.title = poemData.title;
            asset.poem = poemData.poem;

            var noPunc = new string(asset.poem.Where(c => !char.IsPunctuation(c)).ToArray());
            var noReturn = noPunc.Replace("\n", " ");
            var words = noReturn.Split(' ');
            var noEmpty = words.ToList().Where(w => w != string.Empty && w != "" && w != " ");

            foreach (var word in noEmpty)
            {
                // if not in collection, request clip and add it to the global collection
                if (!clipCollection.allClips.ContainsKey(word.ToLower()))
                {
                    Debug.Log(word + " was not found in the collection, trying to process it.");
                    yield return StartCoroutine(RequestAndAddClip(word.ToLower()));
                }

                //// add word and clip to the scriptable object's word dictionary
                //AudioClip clip;
                //if (clipCollection.allClips.TryGetValue(word.ToLower(), out clip))
                //{
                //    asset.words.Add(new KeyValuePair<string, AudioClip>(word, clip));
                //}

                AssetDatabase.SaveAssets();
            }

            AssetDatabase.CreateAsset(asset, "Assets/Resources/Poems/" + poemData.title.Replace(" ", "") + ".asset");
            break;
        }

        AssetDatabase.SaveAssets();
    }

    private IEnumerator RequestAndAddClip(string word)
    {
        _speechManager.Speak(word);

        Debug.Log("Processing word: " + word);

        yield return new WaitForSeconds(5);

        //var clip = Resources.Load<AudioClip>("Assets/Resources/Clips/" + word.RemoveDiacritics() + ".wav");

        //Extensions.Play(clip);

        //if (clip)
        //{
        //    Debug.Log(clip + " was generated.");
        //    clipCollection.allClips.Add(word, clip);
        //}
    }
}

public static class Extensions
{
    public static string RemoveDiacritics(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        text = text.Normalize(NormalizationForm.FormD);
        var chars = text.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
        var noApostrphe = new string(chars).ToString().Replace("'", "");
        return noApostrphe.Normalize(NormalizationForm.FormC);
    }

    public static void Play(AudioClip clip)
    {
        if (clip == null)
            return;

        Debug.Log("Playing " + clip);

        var source = new GameObject().AddComponent<AudioSource>();

        source.PlayOneShot(clip);

        GameObject.Destroy(source.gameObject, clip.length);
    }
}