using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Bullet : BaseBehaviour
{
    public float speed = 1f;
    public float distance = 1f;
    public PersistentDecal decalPrefab;

    public Word Word { get; set; }

    private bool _isMoving;

    //private void Update()
    //{
    //    if (!_isMoving)
    //        return;

    //    var targetPosition = transform.position + _direction * speed;
    //    transform.position = targetPosition;
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (!_isMoving)
            return;

        var direction = Rigidbody.velocity.normalized;
        var contactPoint = Collider.ClosestPointOnBounds(other.transform.position);
        var decalPosition = contactPoint - direction * distance;
        var rotation = Quaternion.LookRotation(direction);

        var persistentDecal = Instantiate(decalPrefab, decalPosition, rotation);
        persistentDecal.Word = Word.word;
        var index = persistentDecal.Decal.Atlas.Regions.FindIndex(x => x.Name == Word.word.ToLower());
        persistentDecal.Decal.AtlasRegionIndex = index;
        DecalManager.PersistentDecals.Add(persistentDecal);

        _isMoving = false;
    }

    public void Move(Vector3 direction)
    {
        AudioSource.Play();

        transform.rotation.SetLookRotation(direction);

        Rigidbody.AddForce(direction * speed);

        _isMoving = true;
    }
}
