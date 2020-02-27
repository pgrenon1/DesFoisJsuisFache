using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    public WordCollection wordCollection;
    public FirstPersonController firstPersonControllerPrefab;
    public Transform decalParent;

    private void Start()
    {
        Instantiate(firstPersonControllerPrefab, Vector3.zero, Quaternion.identity, null);
    }
}
