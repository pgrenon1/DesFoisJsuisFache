#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

public class Poem : OdinSerializedScriptableObject
{
    public string author;
    public string title;
    [TextArea]
    public string poem;
    [FormerlySerializedAs("wordsAsWords")]
    public List<Word> words = new List<Word>();
#if UNITY_EDITOR
    [Button("SaveImages")]
    public void SaveImages()
    {
        foreach (var word in words)
        {
            SaveTextureToFile(word.texture, word.word);
        }
    }

    [Button("GetImages")]
    public void GetImages()
    {
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
        foreach (var word in words)
        {
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
                        word.texture = ((DownloadHandlerTexture)resultRequest.downloadHandler).texture;

                    EditorUtility.SetDirty(this);
                }
            }
        }
    }
#endif
}