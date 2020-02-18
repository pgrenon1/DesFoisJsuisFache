using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class Word
{
    [HorizontalGroup("Word", LabelWidth = 50f)]
    public string word;
    [HorizontalGroup("Word", LabelWidth = 50f)]
    [HorizontalGroup("Word")]
    public AudioClip clip;
    [LabelWidth(50f)]
    public Texture texture;

    public bool IsUsed { get; set; }

    public Word(string word, AudioClip clip, Texture texture = null)
    {
        this.word = word;
        this.clip = clip;
        this.texture = texture;
    }
}