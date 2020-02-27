using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PoemHUD : SingletonMonoBehaviour<PoemHUD>
{
    public float scrollSpeed = 1f;
    public float fadeSpeed = 1f;

    private Poem _poem;
    public Poem Poem
    {
        get
        {
            return _poem;
        }
        set
        {
            _poem = value;
            _poemText.SetText(_poem.poem);
        }
    }

    public float Alpha { get; set; }

    private TextMeshProUGUI _poemText;
    private RectTransform _rectTransform;
    private float _currentAlphaVelocity;
    private CanvasGroup _canvasGroup;

    private void Start()
    {
        _poemText = GetComponent<TextMeshProUGUI>();
        _rectTransform = transform as RectTransform;
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        Alpha = Mathf.Max(Alpha - Time.deltaTime * fadeSpeed, 0);

        _canvasGroup.alpha = Alpha;
    }

    public void ColorWord(TMP_WordInfo wordInfo, Color color)
    {
        //Debug.Log("Coloring " + wordInfo.GetWord());
        for (int i = 0; i < wordInfo.characterCount; ++i)
        {
            int charIndex = wordInfo.firstCharacterIndex + i;
            int meshIndex = _poemText.textInfo.characterInfo[charIndex].materialReferenceIndex;
            int vertexIndex = _poemText.textInfo.characterInfo[charIndex].vertexIndex;

            Color32[] vertexColors = _poemText.textInfo.meshInfo[meshIndex].colors32;
            vertexColors[vertexIndex + 0] = color;
            vertexColors[vertexIndex + 1] = color;
            vertexColors[vertexIndex + 2] = color;
            vertexColors[vertexIndex + 3] = color;
        }
        _poemText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }

    private void ScrollToLine(int selectedLineIndex)
    {
        var textInfo = _poemText.textInfo;
        var lineCount = textInfo.lineCount;
        var lineSample = textInfo.lineInfo[0];
        var lineHeight = lineSample.lineHeight;
        var targetY = lineHeight * selectedLineIndex + transform.parent.GetComponent<RectTransform>().rect.height / 4;

        _rectTransform.DOLocalMove(new Vector2(0f, targetY), scrollSpeed);
    }

    private int GetLineIndex(int currentIndexInt)
    {
        var lineIndex = 0;
        var wordCount = 0;
        foreach (var line in Poem.lines)
        {
            wordCount += line.words.Count;
            if (wordCount > currentIndexInt)
            {
                break;
            }
            lineIndex++;
        }

        return lineIndex;
    }

    public void Deselect(int previousIndex)
    {
        var textInfo = _poemText.textInfo;
        var previousWordInfo = textInfo.wordInfo[previousIndex];

        ColorWord(previousWordInfo, Color.white);
    }

    public void Select(int currentIndex)
    {
        var textInfo = _poemText.textInfo;
        var currentWordInfo = textInfo.wordInfo[currentIndex];

        ColorWord(currentWordInfo, Color.cyan);

        Alpha = 1f;

        var lineIndex = GetLineIndex(currentIndex);

        Debug.Log("line: " + lineIndex + " wordIndex = " + currentIndex + " " + currentWordInfo.GetWord());


        ScrollToLine(lineIndex);
    }
}
