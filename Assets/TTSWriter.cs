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
        var data = JsonUtility.FromJson<PoemDataCollection>(jsonString.text);

        StartCoroutine(CreateAssets(data));
        return;

        //AudioClip[] clips = Resources.LoadAll<AudioClip>("Clips");
        //foreach (var clip in clips)
        //{
        //    if (clipCollection.allClips.ContainsKey(clip.name))
        //    {
        //        clipCollection.allClips[clip.name] = clip;
        //    }
        //    else
        //    {
        //        clipCollection.allClips.Add(clip.name, clip);
        //    }
        //}
        //EditorUtility.SetDirty(clipCollection);
    }

    private IEnumerator CreateAssets(PoemDataCollection data)
    {
        yield return new WaitForSeconds(3f);

        foreach (var poemData in data.collection)
        {
            if (clipCollection.poems.Any(p => p.title == poemData.title))
                continue;

            Debug.Log("Starting to process poem: " + poemData.title);

            var asset = new Poem();
            asset.author = poemData.author;
            asset.title = poemData.title;
            asset.poem = poemData.poem;

            var noPunc = new string(asset.poem.Where(c => c == '-' || c == '\'' || c == '’' || !char.IsPunctuation(c)).ToArray());
            var noReturn = noPunc.Replace("\n", " ");
            var split = noReturn.Split(' ');
            var words = split.ToList().Where(w => w != string.Empty && w != "" && w != " ");

            foreach (var word in words)
            {
                var lowerWord = word.ToLower();

                // if not in collection, request clip and add it to the global collection AND the poem asset
                if (!clipCollection.allClips.ContainsKey(lowerWord))
                {
                    Debug.Log(word + " was not found in the collection, trying to process it.");

                    yield return StartCoroutine(RequestAndAddClip(lowerWord));
                }

                AudioClip clip;
                if (clipCollection.allClips.TryGetValue(lowerWord, out clip))
                {
                    asset.words.Add(new KeyValuePair<string, AudioClip>(word, clip));
                }
                else
                {
                    Debug.Log("Did not manage to get requested clip for word : " + word);
                    asset.words.Add(new KeyValuePair<string, AudioClip>(word, null));
                }
            }

            Debug.Log("Finished processing : " + asset.title);

            clipCollection.poems.Add(asset);
            EditorUtility.SetDirty(clipCollection);

            AssetDatabase.CreateAsset(asset, "Assets/Resources/Poems/" + asset.title + ".asset");
            EditorUtility.SetDirty(asset);

            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;

            SpeechManager.voiceName = SpeechManager.voiceName == CognitiveServicesTTS.VoiceName.frCHGuillaume ? CognitiveServicesTTS.VoiceName.frCACaroline : CognitiveServicesTTS.VoiceName.frCHGuillaume;
            SpeechManager.gender = SpeechManager.voiceName == CognitiveServicesTTS.VoiceName.frCHGuillaume ? CognitiveServicesTTS.Gender.Male : CognitiveServicesTTS.Gender.Female;
        }
    }

    private IEnumerator RequestAndAddClip(string word)
    {
        Debug.Log("Processing word: " + word);

        _speechManager.GetSpeech(word);

        yield return new WaitForSeconds(5);
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