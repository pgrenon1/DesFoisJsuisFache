using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]
public class Bullet : BaseBehaviour
{
    public float decalDistance = 1f;
    public PersistentDecal decalPrefab;
    public LayerMask layerMask;
    public float maxDelay;
    public MeshRenderer meshRenderer;

    public Word Word { get; set; }
    public bool HasAudioDelay { get; set; }

    private bool _isMoving;
    private Vector3 _lastPosition;
    private float _speed;
    private Vector3 _direction;

    private void Start()
    {
        Despawn();
    }

    private void Update()
    {
        if (Physics.Linecast(_lastPosition, transform.position, layerMask))
        {
            SpawnDecal(_lastPosition, _direction);
        }

        _lastPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (_isMoving)
            transform.position = transform.position + _direction.normalized * _speed * Time.deltaTime;  
    }

    private void SpawnDecal(Vector3 point, Vector3 direction)
    {
        _isMoving = false;

        var rotation = Quaternion.LookRotation(direction);
        var decalPosition = point - direction * decalDistance;

        decalPrefab.Decal.Atlas = FirstPersonController.Instance.ActivePoem.decalTextureAtlas;

        var persistentDecal = Instantiate(decalPrefab, decalPosition, rotation);
        persistentDecal.Word = Word.word;

        var index = persistentDecal.Decal.Atlas.Regions.FindIndex(x => x.Name == Word.word.ToLower());

        persistentDecal.Decal.AtlasRegionIndex = index;

        DecalManager.PersistentDecals.Add(persistentDecal);

        persistentDecal.Decal.LateBake();

        meshRenderer.enabled = false;
    }

    private void Despawn()
    {
        Destroy(gameObject, 1f);
    }

    public virtual void Move(Vector3 direction, float speed)
    {
        if (HasAudioDelay)
            AudioSource.PlayDelayed(Random.Range(0f, maxDelay));
        else
            AudioSource.Play();

        _lastPosition = transform.position;

        transform.rotation.SetLookRotation(direction);

        _direction = direction;
        _speed = speed;
        //Rigidbody.AddForce(direction * speed);

        _isMoving = true;
    }
}
