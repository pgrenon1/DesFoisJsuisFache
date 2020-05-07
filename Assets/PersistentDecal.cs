using ch.sycoforge.Decal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentDecal : MonoBehaviour
{
    public string Word { get; set; }

    private EasyDecal _decal;
    public EasyDecal Decal
    {
        get
        {
            if (_decal == null)
                _decal = GetComponentInChildren<EasyDecal>();

            return _decal;
        }
    }
}
