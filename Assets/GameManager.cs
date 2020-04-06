using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    public WordCollection wordCollection;
    public FirstPersonController firstPersonControllerPrefab;

    private void Start()
    {
        Instantiate(firstPersonControllerPrefab, transform.position, Quaternion.identity, null);
    }
}
