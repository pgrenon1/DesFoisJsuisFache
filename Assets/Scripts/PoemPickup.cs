using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoemPickup : Interaction
{
    protected override void Interact()
    {
        base.Interact();

        FirstPersonController.Instance.CyclePoem();
    }
}
