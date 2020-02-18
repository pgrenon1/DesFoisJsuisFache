#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;
using ch.sycoforge.Decal;

[CreateAssetMenu(fileName = "ClipCollection", menuName = "Clip Collection")]
public class WordCollection : OdinSerializedScriptableObject
{
    [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = false)]
    public List<Word> allWords = new List<Word>();
    public List<Poem> poems = new List<Poem>();

    private bool _isStopped = false;
#if UNITY_EDITOR
    [Button("STOP", ButtonSizes.Gigantic)]
    public void Stop()
    {
        _isStopped = true;
    }

    [Button("SORT")]
    public void Sort()
    {
        allWords.Sort((x, y) => string.Compare(x.word, y.word));
    }

    //[Button("GetWords")]
    //public void GetWords()
    //{
    //    foreach (var pair in allClips)
    //    {
    //        allWords.AddUnique(new Word(pair.Key, pair.Value));
    //    }

    //    EditorUtility.SetDirty(this);
    //}

    [Button("GetImages")]
    public void GetImages()
    {
        _isStopped = false;

        EditorCoroutineUtility.StartCoroutineOwnerless(GetRequest());
    }

    public void SaveTextureToFile(Texture texture, string fileName)
    {
        var texture2D = texture as Texture2D;
        var bytes = texture2D.EncodeToPNG();
        var file = File.Open("Assets/Resources/Textures" + "/" + fileName + ".png", FileMode.Create);
        var binary = new BinaryWriter(file);
        binary.Write(bytes);
        file.Close();
    }

    private IEnumerator GetRequest()
    {
        foreach (var word in allWords)
        {
            if (word.texture != null)
                continue;

            var uri = "http://api.img4me.com/?text=" + word.word + "&font=arial&fcolor=000000&size=34&bcolor=&type=png";

            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                if (webRequest.isNetworkError)
                {
                    Debug.Log(pages[page] + ": Error: " + webRequest.error);
                }
                else
                {
                    Debug.Log(webRequest.downloadHandler.text);

                    UnityWebRequest resultRequest = UnityWebRequestTexture.GetTexture(webRequest.downloadHandler.text);

                    yield return resultRequest.SendWebRequest();

                    if (resultRequest.isNetworkError || resultRequest.isHttpError)
                        Debug.Log(resultRequest.error);
                    else
                    {
                        var texture = ((DownloadHandlerTexture)resultRequest.downloadHandler).texture;

                        SaveTextureToFile(texture, word.word);
                    }

                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                    Texture textureLoaded = (Texture)Resources.Load("Textures/" + word.word, typeof(Texture));

                    word.texture = textureLoaded;

                    EditorUtility.SetDirty(this);
                }
            }

            if (_isStopped)
                break;
        }
    }
#endif
}
