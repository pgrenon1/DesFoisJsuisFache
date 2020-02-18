using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interacter : BaseBehaviour
{
    private Camera _mainCamera;
    private Gun _gun;

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        UpdateClick();
    }

    private void UpdateClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hitInfo;
            var forward = _mainCamera.transform.forward;
            if (Physics.Raycast(_mainCamera.transform.position, forward, out hitInfo))
            {
                var gun = hitInfo.collider.GetComponentInParent<Gun>();
                if (gun)
                {
                    Equip(gun);
                    return;
                }
            }

            if (_gun)
            {
                _gun.Shoot();
            }
        }
    }

    private void Equip(Gun gun)
    {
        if (_gun != null)
        {
            _gun.transform.parent = null;
            _gun.transform.position = gun.transform.position;
            _gun.pickupCollider.gameObject.SetActive(true);
        }

        gun.transform.parent = transform;
        gun.transform.localPosition = Vector3.zero;
        gun.transform.localRotation = Quaternion.identity;
        gun.pickupCollider.gameObject.SetActive(false);

        _gun = gun;
    }
}
