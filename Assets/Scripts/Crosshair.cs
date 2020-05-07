using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : BaseBehaviour
{
    private FirstPersonController _fpsController;

    private Gun CurrentGun
    {
        get
        {
            return _fpsController.Gun;
        }
    }

    private void Start()
    {
        _fpsController = FindObjectOfType<FirstPersonController>();
    }

    private void Update()
    {
        float targetSpread = 0f;

        if (CurrentGun)
            targetSpread = CurrentGun.CurrentSpreadValue;

        Animator.SetFloat("Spread", targetSpread);
    }


}
