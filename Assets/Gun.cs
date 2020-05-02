using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;
using Sirenix.OdinInspector;

[RequireComponent(typeof(AudioSource))]
public class Gun : BaseBehaviour
{
    public Poem poem;
    public AudioSource emptySound;
    public Bullet bulletPrefab;
    public GameObject pickupVisuals;
    public GameObject equipedVisuals;
    public ParticleSystem muzzleFlash;

    [Header("FIRING")]
    public Transform projectileOrigin;
    public bool isFullAuto = false;
    public float rateOfFire = 0.5f;
    public int bulletsPerShot = 1;
    public float spreadFactor = 0.001f;
    public float recoilPerShot = 0.5f;
    public float recoilReturnRate = 1f;
    public float bulletSpeed = 3000f;

    [Header("RECOIL")]
    public AnimationCurve recoilCurve;

    [Header("CROSSHAIR")]
    public Crosshair crosshairPrefab;
    public float spreadPerShot = 0.5f;
    public float returnRate = 1f;
    public AnimationCurve returnCurve;

    public bool HasReleasedTrigger { get; set; } = true;
    public float CurrentSpreadValue { get; set; }

    public bool IsROFReady
    {
        get
        {
            return _rateOfFireValue <= 0f && (isFullAuto || HasReleasedTrigger);
        }
    }

    private float _rateOfFireValue;
    private bool _isEquipped;
    private Crosshair _crosshair;
    private float _recoilValue;

    private void Update()
    {
        UpdateVisibility();

        UpdateCrosshair();

        UpdateRecoil();

        UpdateROF();
    }

    private void UpdateVisibility()
    {
        equipedVisuals.SetActive(_isEquipped);
        pickupVisuals.SetActive(!_isEquipped);
    }

    private void UpdateROF()
    {
        _rateOfFireValue = Mathf.Clamp01(_rateOfFireValue - Time.deltaTime);
    }

    private void UpdateRecoil()
    {
        _recoilValue = Mathf.Max(_recoilValue - Time.deltaTime * recoilReturnRate, 0f);

        Animator.SetFloat("Recoil", recoilCurve.Evaluate(_recoilValue));
    }

    private void UpdateCrosshair()
    {
        if (!_crosshair)
            return;

        CurrentSpreadValue -= Time.deltaTime * returnRate * returnCurve.Evaluate(CurrentSpreadValue);
        CurrentSpreadValue = Mathf.Clamp01(CurrentSpreadValue);
    }

    public void Equip()
    {
        PoemHUD.Instance.Poem = poem;

        FirstPersonController.Instance.StartCoroutine(FirstPersonController.Instance.RefreshAtEndOfFrame());

        pickupVisuals.SetActive(false);
        equipedVisuals.SetActive(true);
        _isEquipped = true;

        if (_crosshair)
            _crosshair.gameObject.SetActive(true);
        else
            _crosshair = Instantiate(crosshairPrefab, new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0), Quaternion.identity, FindObjectOfType<Canvas>().transform);
    }

    public void Unequip(Vector3 position)
    {
        transform.parent = null;
        transform.position = position;

        pickupVisuals.SetActive(true);
        equipedVisuals.SetActive(false);
        _crosshair.gameObject.SetActive(false);
        _isEquipped = false;
    }

    public void Shoot(int index)
    {
        for (int i = 0; i < bulletsPerShot; i++)
        {
            var bullet = Instantiate(bulletPrefab, projectileOrigin.position/*Camera.main.transform.position*/, Quaternion.identity, transform);

            bullet.Word = poem.words[index];
            bullet.AudioSource.clip = bullet.Word.clip;
            bullet.transform.parent = null;
            bullet.gameObject.SetActive(true);

            var direction = Camera.main.transform.forward;

            direction.x += Random.Range(-spreadFactor, spreadFactor);
            direction.y += Random.Range(-spreadFactor, spreadFactor);
            direction.z += Random.Range(-spreadFactor, spreadFactor);

            bullet.Move(direction, bulletSpeed);
        }

        muzzleFlash.Play();
        CurrentSpreadValue += spreadPerShot;
        _recoilValue = Mathf.Min(_recoilValue + recoilPerShot, 1f);
        _rateOfFireValue = rateOfFire;
        HasReleasedTrigger = false;
    }
}
