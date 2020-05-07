#if UNITY_EDITOR
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

public class TTSWriter : OdinSerializedBaseBehaviour
{
    public string fileName;
    public WordCollection clipCollection;

    private SpeechManager _speechManager;

    private void Start()
    {
        _speechManager = GetComponent<SpeechManager>();
        var jsonString = Resources.Load<TextAsset>(fileName);
        var data = JsonUtility.FromJson<PoemDataCollection>(jsonString.text);

        StartCoroutine(CreateAssets(data));
    }

    private IEnumerator CreateAssets(PoemDataCollection data)
    {
        yield return new WaitForSeconds(3f);

        foreach (var poemData in data.collection)
        {
            if (clipCollection.poems.Any(p => p.title == poemData.title))
                continue;

            Debug.Log("Starting to process poem: " + poemData.title);

            var poemAsset = new Poem();
            poemAsset.author = poemData.author;
            poemAsset.title = poemData.title;
            poemAsset.poem = poemData.poem;

            var noPunc = new string(poemAsset.poem.Where(c => c == '-' || c == '\'' || c == '’' || !char.IsPunctuation(c)).ToArray());
            var noReturn = noPunc.Replace("\n", " ");
            var split = noReturn.Split(' ');
            var words = split.ToList().Where(w => w != string.Empty && w != "" && w != " ");

            foreach (var word in words)
            {
                var lowerWord = word.ToLower();

                // if not in collection, request clip and add it to the global collection AND the poem asset
                var wordInCollection = clipCollection.allWords.Find(x => x.word == lowerWord);
                if (wordInCollection == null)
                {
                    Debug.Log(word + " was not found in the collection, trying to process it.");

                    yield return StartCoroutine(RequestAndAddClip(lowerWord));
                }

                AudioClip clip = null;
                foreach (var wordData in clipCollection.allWords)
                {
                    if (wordData != null && wordData.word != null)
                    {
                        if (wordData.word == lowerWord)
                        {
                            clip = wordData.clip;
                            break;
                        }
                    }
                }

                if (clip != null)
                {
                    poemAsset.words.Add(new Word(word, clip));
                }
                else
                {
                    Debug.Log("Did not manage to get requested clip for word : " + word);
                    poemAsset.words.Add(new Word(word, null));
                }
            }

            Debug.Log("Finished processing : " + poemAsset.title);

            clipCollection.poems.Add(poemAsset);
            EditorUtility.SetDirty(clipCollection);

            AssetDatabase.CreateAsset(poemAsset, "Assets/Resources/Poems/" + poemAsset.title + ".asset");
            EditorUtility.SetDirty(poemAsset);

            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = poemAsset;
        }
    }

    private IEnumerator RequestAndAddClip(string word)
    {
        Debug.Log("Processing word: " + word);

        _speechManager.SpeakWithRESTAPI(word);

        yield return new WaitForSeconds(5);
    }
}

//public static class Extensions
//{
//    public static string RemoveDiacritics(this string text)
//    {
//        if (string.IsNullOrWhiteSpace(text))
//            return text;

//        text = text.Normalize(NormalizationForm.FormD);
//        var chars = text.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
//        var noApostrphe = new string(chars).ToString().Replace("'", "");
//        return noApostrphe.Normalize(NormalizationForm.FormC);
//    }

//    public static void AddUnique<T>(this List<T> list, T element)
//    {
//        if (!list.Contains(element))
//            list.Add(element);
//    }

//    public static void Play(AudioClip clip)
//    {
//        if (clip == null)
//            return;

//        Debug.Log("Playing " + clip);

//        var source = new GameObject().AddComponent<AudioSource>();

//        source.PlayOneShot(clip);

//        GameObject.Destroy(source.gameObject, clip.length);
//    }

//    public static List<List<string>> GetTokenizedLines(string poem)
//    {
//        var tokenized = new List<List<string>>();

//        var lines = poem.Split('\n');

//        foreach (var line in lines)
//        {
//            var noPunc = new string(line.Where(c => c == '-' || c == '\'' || c == '’' || !char.IsPunctuation(c)).ToArray());
//            var noReturn = noPunc.Replace("\n", " ");
//            var split = noReturn.Split(' ');
//            var words = split.Where(w => w != string.Empty && w != "" && w != " ").ToList();

//            tokenized.Add(words);
//        }

//        return tokenized;
//    }
//}
#endif