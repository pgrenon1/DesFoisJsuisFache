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

    private void OnTriggerEnter(Collider other)
    {
        if (!_isMoving)
            return;

        var direction = Rigidbody.velocity.normalized;
        var contactPoint = Collider.ClosestPointOnBounds(other.transform.position);
        var decalPosition = contactPoint - direction * distance;
        var decalRotation = Quaternion.LookRotation(direction);

        var decal = Instantiate(decalPrefab, decalPosition, decalRotation, GameManager.Instance.decalParent);
        decal.Word = Word.word;
        var index = decal.Decal.Atlas.Regions.FindIndex(x => x.Name == Word.word.ToLower());
        decal.Decal.AtlasRegionIndex = index;

        DecalManager.Instance.RuntimeDecalData.Add(new DecalData(decalPosition, decalRotation, decal.Word));

        _isMoving = false;

        Despawn();
    }

    private void Despawn()
    {
        Destroy(gameObject, 1f);
    }

    public void Move()
    {
        AudioSource.Play();

        transform.rotation.SetLookRotation(transform.forward);

        Rigidbody.AddForce(transform.forward * speed);

        _isMoving = true;
    }
}
