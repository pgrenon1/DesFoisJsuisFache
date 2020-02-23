using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Gun : BaseBehaviour
{
    public Collider pickupCollider;
    public Poem poem;
    public AudioSource emptySound;
    public Bullet bulletPrefab;
    [Space]
    public float spreadPerShot = 0.5f;

    public void Equip()
    {
        PoemHUD.Instance.Poem = poem;

        FirstPersonController.Instance.StartCoroutine(FirstPersonController.Instance.RefreshAtEndOfFrame());
    }

    public void Shoot(int index)
    {
        var bullet = Instantiate(bulletPrefab, Camera.main.transform.position, Quaternion.identity, transform);
        bullet.Word = poem.words[index];
        bullet.AudioSource.clip = bullet.Word.clip;
        bullet.transform.parent = null;
        bullet.gameObject.SetActive(true);
        bullet.Move(Camera.main.transform.forward);

        Crosshair.Instance.TargetSpreadValue += spreadPerShot;
    }
}
