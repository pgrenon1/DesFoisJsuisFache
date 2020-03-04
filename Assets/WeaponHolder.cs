using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class WeaponHolder : SingletonMonoBehaviour<WeaponHolder>
{
    public Transform weaponPivot;
    public float gunPunchScale = 0.005f;
    public AnimationCurve turnAmountCurve;
    public float rotationScale = 2f;
    public float turnReturnRate = 1f;

    private int _bumpDirection = 1;
    private float _turnAmount = 0f;

    public void Step()
    {
        transform.DOPunchPosition(-Vector3.up * gunPunchScale, 0.35f);
        weaponPivot.DOPunchRotation(Vector3.up * _bumpDirection * 2.5f, 0.5f, 1, 0);

        _bumpDirection = _bumpDirection == -1 ? 1 : -1;
    }

    private void FixedUpdate()
    {
        var yRot = CrossPlatformInputManager.GetAxis("Mouse X");
        var xRot = CrossPlatformInputManager.GetAxis("Mouse Y");

        weaponPivot.localRotation = Quaternion.Lerp(weaponPivot.localRotation,
            Quaternion.AngleAxis(-yRot * rotationScale, Vector3.up) *
            Quaternion.AngleAxis(xRot * rotationScale, Vector3.right), Time.deltaTime * turnReturnRate);
    }
}
