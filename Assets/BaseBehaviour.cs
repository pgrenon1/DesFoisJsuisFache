using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseBehaviour : MonoBehaviour
{
    private AudioSource _audioSource;
    public AudioSource AudioSource
    {
        get
        {
            if (!_audioSource)
                _audioSource = GetComponent<AudioSource>();

            return _audioSource;
        }
    }

    private CanvasGroup _canvasGroup;
    public CanvasGroup CanvasGroup
    {
        get
        {
            if (!_canvasGroup)
                _canvasGroup = GetComponent<CanvasGroup>();

            return _canvasGroup;
        }
    }

    private Rigidbody _rigidbody;
    public Rigidbody Rigidbody
    {
        get
        {
            if (!_rigidbody)
                _rigidbody = GetComponent<Rigidbody>();

            return _rigidbody;
        }
    }

    private Canvas _canvas;
    public Canvas Canvas
    {
        get
        {
            if (!_canvas)
                _canvas = GetComponent<Canvas>();

            return _canvas;
        }
    }

    private Collider _collider;
    public Collider Collider
    {
        get
        {
            if (!_collider)
                _collider = GetComponent<Collider>();

            return _collider;
        }
    }

    private Image _image;
    public Image Image
    {
        get
        {
            if (!_image)
                _image = GetComponent<Image>();

            return _image;
        }
    }

    private Camera _camera;
    public Camera Camera
    {
        get
        {
            if (!_camera)
                _camera = GetComponent<Camera>();

            return _camera;
        }
    }

    private RectTransform _rectTransform;
    public RectTransform RectTransform
    {
        get
        {
            if (!_rectTransform)
                _rectTransform = GetComponent<RectTransform>();

            return _rectTransform;
        }
    }
}
