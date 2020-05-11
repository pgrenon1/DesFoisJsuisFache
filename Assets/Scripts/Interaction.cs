using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Interaction : BaseBehaviour
{
    public float fadeSpeed = 1f;

    private TextMeshPro _text;
    private bool _playerIsNear;
    private float _currentAlphaVelocity;

    protected virtual void Start()
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
        var alpha = Mathf.SmoothDamp(_text.color.a, _playerIsNear && IsInteractable() ? .7f : 0f, ref _currentAlphaVelocity, fadeSpeed);
        var color = _text.color;
        color.a = alpha;
        _text.color = color;

        if (!_playerIsNear)
            return;

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.F))
        {
            Interact();
        }
    }

    protected virtual void Interact()
    {
        if (AudioSource)
            AudioSource.Play();
    }

    protected virtual bool IsInteractable()
    {
        return true;
    }
}