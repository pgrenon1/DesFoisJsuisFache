using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Bullet : BaseBehaviour
{
    public float decalDistance = 1f;
    public PersistentDecal decalPrefab;
    public LayerMask layerMask;

    public Word Word { get; set; }

    private bool _isMoving;
    private Vector3 _lastPosition;

    private void Start()
    {
        Despawn();
    }

    private void Update()
    {
        var direction = transform.position - _lastPosition;

        if (Physics.Raycast(_lastPosition, direction, direction.magnitude, layerMask))
        {
            SpawnDecal(_lastPosition, direction);
        }

        _lastPosition = transform.position;
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (!_isMoving)
    //        return;

    //    var direction = Rigidbody.velocity.normalized;
    //    var contactPoint = Collider.ClosestPointOnBounds(other.transform.position);

    //    SpawnDecal(contactPoint, direction);
    //}

    private void SpawnDecal(Vector3 point, Vector3 direction)
    {
        var rotation = Quaternion.LookRotation(direction);
        var decalPosition = point - direction * decalDistance;
        var persistentDecal = Instantiate(decalPrefab, decalPosition, rotation);
        persistentDecal.Word = Word.word;
        var index = persistentDecal.Decal.Atlas.Regions.FindIndex(x => x.Name == Word.word.ToLower());
        persistentDecal.Decal.AtlasRegionIndex = index;
        DecalManager.PersistentDecals.Add(persistentDecal);
        persistentDecal.Decal.LateBake();

        //Debug.Log(DecalManager.PersistentDecals.Count);

        _isMoving = false;
    }

    private void Despawn()
    {
        Destroy(gameObject, 1f);
    }

    public virtual void Move(Vector3 direction, float speed)
    {
        AudioSource.Play();

        transform.rotation.SetLookRotation(direction);

        Rigidbody.AddForce(direction * speed);

        _isMoving = true;
    }
}
