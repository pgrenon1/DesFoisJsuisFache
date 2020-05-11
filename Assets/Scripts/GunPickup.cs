using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunPickup : Interaction
{
    public Gun gunPrefab;
    public Transform spawnTransform;

    public Gun Gun { get; set; }


    protected override void Start()
    {
        base.Start();

        Gun = Instantiate(gunPrefab, spawnTransform.position, spawnTransform.rotation, spawnTransform);
    }

    protected override void Interact()
    {
        if (!IsInteractable())
            return;

        base.Interact();

        FirstPersonController.Instance.Equip(this);
    }

    protected override bool IsInteractable()
    {
        return Gun != null;
    }
}
