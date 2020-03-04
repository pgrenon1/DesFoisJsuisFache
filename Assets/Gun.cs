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
    public Transform bulletOrigin;
    [Space]
    public bool bumpIndexOnShoot = false;
    public int bulletsPerShot = 1;
    public float rateOfFire = 2f;
    public float spreadPerShot = 0.5f;
    public float spreadReturnRate = 5f;
    public AnimationCurve spreadReturnCurve;

    public float TargetSpreadValue { get; set; } = 0f;

    private float _mainTimer = 0f;
    private float _paddingTimer = 0f;
    private Animator _crosshairAnimator;

    public bool ShotIsReady
    {
        get
        {
            return _mainTimer <= 0f;
        }
    }

    public void Equip()
    {
        PoemHUD.Instance.Poem = poem;

        FirstPersonController.Instance.StartCoroutine(FirstPersonController.Instance.RefreshAtEndOfFrame());

        _mainTimer = 1f;

        _crosshairAnimator = Crosshair.Instance.GetComponent<Animator>();
    }

    private void Update()
    {
        UpdateMainTimer();

        UpdateSpread();
    }

    private void UpdateSpread()
    {
        if (!_crosshairAnimator)
            return;

        TargetSpreadValue -= Time.deltaTime * spreadReturnRate * spreadReturnCurve.Evaluate(TargetSpreadValue);

        TargetSpreadValue = Mathf.Clamp01(TargetSpreadValue);

        _crosshairAnimator.SetFloat("Spread", TargetSpreadValue);
    }

    private void UpdateMainTimer()
    {
        if (_mainTimer <= 0f)
            return;

        _mainTimer -= Time.deltaTime * rateOfFire;
    }

    public void Shoot(int index)
    {
        for (int i = 0; i < bulletsPerShot; i++)
        {
            var rotation = Quaternion.LookRotation(Camera.main.transform.forward, Camera.main.transform.up);

            if (bulletsPerShot > 1)
            {
                rotation.x = rotation.x + UnityEngine.Random.Range(-1f, 1f) * 0.07f;
                rotation.y = rotation.y + UnityEngine.Random.Range(-1f, 1f) * 0.07f;
                rotation.z = rotation.z + UnityEngine.Random.Range(-1f, 1f) * 0.07f;
            }

            var bullet = Instantiate(bulletPrefab, bulletOrigin.position, rotation, transform);
            bullet.Word = poem.words[index];
            bullet.AudioSource.clip = bullet.Word.clip;
            bullet.transform.parent = null;
            bullet.Move();
        }

        _mainTimer = 1f;
        TargetSpreadValue += spreadPerShot;
        PoemHUD.Instance.Alpha += 0.2f;
    }
}
