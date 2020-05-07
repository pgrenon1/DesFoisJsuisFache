using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalManager : SingletonMonoBehaviour<DecalManager>
{
    public static List<PersistentDecal> PersistentDecals { get; set; } = new List<PersistentDecal>();

    private void Start()
    {

    }
}
