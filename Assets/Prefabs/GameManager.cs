using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;
using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    public WordCollection wordCollection;
    public FirstPersonController firstPersonControllerPrefab;

    private void Start()
    {
        Instantiate(firstPersonControllerPrefab, transform.position, Quaternion.identity, null);
    }

    public void Reset()
    {
        DecalManager.Instance.Reset();
    }
}
