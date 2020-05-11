using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reset : Interaction
{
    protected override void Interact()
    {
        base.Interact();
        GameManager.Instance.Reset();
    }
}
