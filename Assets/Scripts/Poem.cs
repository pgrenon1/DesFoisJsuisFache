#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
//using SimpleSpritePackerEditor;
#endif
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

using System.Linq;
using ch.sycoforge.Decal;

public class Line
{
    [LabelText("@LineTitle()")]
    public List<Word> words = new List<Word>();

    private string LineTitle()
    {
        return String.Join(" ", words.Select(x => x.word));
    }
}

public class Poem : OdinSerializedScriptableObject
{
    public string author;
    public string title;
    [TextArea]
    public string poem;
    public List<Word> words = new List<Word>();
    public List<Line> lines = new List<Line>();
    public DecalTextureAtlas decalTextureAtlas;

#if UNITY_EDITOR
    //[Button("SaveImages")]
    //public void SaveImages()
    //{
    //    foreach (var word in words)
    //    {
    //        SaveTextureToFile(word.texture, word.word);
    //    }
    //}

    [Button("Seperate In Lines")]
    public void GetLines()
    {
        var lineStrings = Extensions.GetTokenizedLines(poem);

        foreach (var lineString in lineStrings)
        {
            var newLine = new Line();
            foreach (var wordString in lineString)
            {
                var wordWord = words.Find(x => x.word == wordString);
                newLine.words.Add(wordWord);
            }
            lines.Add(newLine);
        }

        EditorUtility.SetDirty(this);
    }

    //[Button("CreateDirectoryAndAtlases")]
    //public void CreateDirectoryAndAtlases()
    //{
    //    var folderString = @"Assets/Resources/Textures";
    //    string guid = AssetDatabase.CreateFolder(folderString, title);
    //    string newFolderPath = AssetDatabase.GUIDToAssetPath(guid);

    //    var noPunc = new string(poem.Where(c => c == '-' || c == '\'' || c == '’' || !char.IsPunctuation(c)).ToArray());
    //    var noReturn = noPunc.Replace("\n", " ");
    //    var split = noReturn.Split(' ');
    //    var words = split.ToList().Where(w => w != string.Empty && w != "" && w != " ");

    //    //foreach (var word in words)
    //    //{
    //    //    Debug.Log(word);
    //    //    var texture = Resources.Load<Texture2D>(@"WordsPNG\" + word.ToLower()); // + " t:texture2d", new[] { @"Assets\Resources\WordsPNG" });

    //    //    if (texture)
    //    //        AssetDatabase.CopyAsset(@"Assets\Resources\WordsPNG\" + word.ToLower() + ".png", newFolderPath + @"\" + word + ".png");
    //    //    else
    //    //        Debug.Log("No texture at" + @"Assets\Resources\WordsPNG\" + word.ToLower() + ".png");

    //    //    AssetDatabase.SaveAssets();
    //    //    AssetDatabase.Refresh();
    //    //}

    //    GetOriginals(newFolderPath, title, words);
    //}

    //private void GetOriginals(string newFolderPath, string title, IEnumerable<string> words)
    //{
    //    AssetDatabase.CopyAsset(@"Assets\Resources\Originals\_Sprite Packer.asset", newFolderPath + @"\" + title + "_Packer.asset");
    //    AssetDatabase.CopyAsset(@"Assets\Resources\Originals\_Sprite Packer.png", newFolderPath + @"\" + title + "_Packer.png");

    //    AssetDatabase.SaveAssets();
    //    AssetDatabase.Refresh();
    //    AssetDatabase.ImportAsset(newFolderPath + @"\" + title + "_Packer.asset");
    //    AssetDatabase.ImportAsset(newFolderPath + @"\" + title + "_Packer.png");

    //    var packer = AssetDatabase.LoadAssetAtPath<SimpleSpritePacker.SPInstance>(newFolderPath + @"\" + title + "_Packer.asset");
    //    var packerPNG = AssetDatabase.LoadAssetAtPath<Texture2D>(newFolderPath + @"\" + title + "_Packer.png");

    //    packer.texture = packerPNG;

    //    foreach (var word in words)
    //    {
    //        var texture = Resources.Load<Texture2D>(@"WordsPNG\" + word.ToLower());

    //        var alreadyContains = false;
    //        foreach (var info in packer.sprites)
    //        {
    //            if (info.source == texture)
    //                alreadyContains = true;
    //        }

    //        if (alreadyContains)
    //            continue;

    //        var spriteInfo = new SimpleSpritePacker.SPSpriteInfo();
    //        spriteInfo.source = texture;
    //        packer.AddSprite(spriteInfo);

    //        AssetDatabase.SaveAssets();
    //        AssetDatabase.Refresh();
    //    }

    //    var builder = new SPAtlasBuilder(packer);
    //    builder.RebuildAtlas();

    //    AssetDatabase.SaveAssets();
    //    AssetDatabase.Refresh();

    //    // DECALS

    //    AssetDatabase.CopyAsset(@"Assets\Resources\Originals\_Decal Atlas.asset", newFolderPath + @"\" + title + "_DecalAtals.asset");
    //    AssetDatabase.CopyAsset(@"Assets\Resources\Originals\_Decal Atlas Mat.mat", newFolderPath + @"\" + title + "_DecalAtlasMat.mat");

    //    AssetDatabase.SaveAssets();
    //    AssetDatabase.Refresh();
    //    AssetDatabase.ImportAsset(newFolderPath + @"\" + title + "_DecalAtals.asset");
    //    AssetDatabase.ImportAsset(newFolderPath + @"\" + title + "_DecalAtlasMat.mat");

    //    var decalAtlas = AssetDatabase.LoadAssetAtPath<DecalTextureAtlas>(newFolderPath + @"\" + title + "_DecalAtals.asset");
    //    var material = AssetDatabase.LoadAssetAtPath<Material>(newFolderPath + @"\" + title + "_DecalAtlasMat.mat");

    //    material.SetTexture("_MainTex", packerPNG);
    //    decalAtlas.Material = material;

    //    AssetDatabase.SaveAssets();
    //    AssetDatabase.Refresh();

    //    var spritesArray = AssetDatabase.LoadAllAssetsAtPath(newFolderPath + @"\" + title + "_Packer.png").OfType<Sprite>().ToArray();

    //    foreach (var sprite in spritesArray)
    //    {
    //        var region = new AtlasRegion();

    //        region.Name = sprite.name;

    //        var position = new Vector2(sprite.rect.x / sprite.texture.width, sprite.rect.y / sprite.texture.height);
    //        var size = new Vector2(sprite.rect.width / sprite.texture.width, sprite.rect.height / sprite.texture.height);

    //        Debug.Log(position);
    //        Debug.Log(size);

    //        region.Region = new Rect(position, size);

    //        decalAtlas.Regions.Add(region);

    //        EditorUtility.SetDirty(decalAtlas);

    //        AssetDatabase.SaveAssets();
    //        AssetDatabase.Refresh();

    //        decalAtlas.CallOnAtlasChanged();
    //    }

    //    AssetDatabase.SaveAssets();
    //    AssetDatabase.Refresh();

    //    decalTextureAtlas = decalAtlas;

    //    EditorUtility.SetDirty(this);
    //    AssetDatabase.SaveAssets();
    //    AssetDatabase.Refresh();
    //}

    //[Button("GetImages")]
    //public void GetImages()
    //{
    //    EditorCoroutineUtility.StartCoroutineOwnerless(GetRequest());
    //}

    public void SaveTextureToFile(Texture texture, string fileName)
    {
        var texture2D = texture as Texture2D;
        var bytes = texture2D.EncodeToPNG();
        var file = File.Open("Assets/Resources/Textures" + "/" + fileName + ".png", System.IO.FileMode.Create);
        var binary = new BinaryWriter(file);
        binary.Write(bytes);
        file.Close();
    }

    //private IEnumerator GetRequest()
    //{
    //    foreach (var word in words)
    //    {
    //        var uri = "http://api.img4me.com/?text=" + word.word + "&font=arial&fcolor=000000&size=34&bcolor=&type=png";

    //        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
    //        {
    //            // Request and wait for the desired page.
    //            yield return webRequest.SendWebRequest();

    //            string[] pages = uri.Split('/');
    //            int page = pages.Length - 1;

    //            if (webRequest.isNetworkError)
    //            {
    //                Debug.Log(pages[page] + ": Error: " + webRequest.error);
    //            }
    //            else
    //            {
    //                Debug.Log(webRequest.downloadHandler.text);

    //                UnityWebRequest resultRequest = UnityWebRequestTexture.GetTexture(webRequest.downloadHandler.text);

    //                yield return resultRequest.SendWebRequest();

    //                if (resultRequest.isNetworkError || resultRequest.isHttpError)
    //                    Debug.Log(resultRequest.error);
    //                else
    //                    word.texture = ((DownloadHandlerTexture)resultRequest.downloadHandler).texture;

    //                EditorUtility.SetDirty(this);
    //            }
    //        }
    //    }
    //}
#endif
}