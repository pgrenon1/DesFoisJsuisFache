using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Reset : BaseBehaviour
{
    public float fadeSpeed = 1f;

    private TextMeshPro _text;
    private bool _playerIsNear;
    private float _currentAlphaVelocity;

    private void Start()
    {
        _text = GetComponentInChildren<TextMeshPro>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<FirstPersonController>())
        {
            _playerIsNear = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<FirstPersonController>())
        {
            _playerIsNear = false;
        }
    }

    private void Update()
    {
        var alpha = Mathf.SmoothDamp(_text.color.a, _playerIsNear ? .7f : 0f, ref _currentAlphaVelocity, fadeSpeed);
        var color = _text.color;
        color.a = alpha;
        _text.color = color;

        if (!_playerIsNear)
            return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            AudioSource.Play();
            GameManager.Instance.Reset();
        }
    }
}
