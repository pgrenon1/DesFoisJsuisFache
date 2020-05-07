using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool : SingletonMonoBehaviour<Pool>
{
    public int startAmount;
    public Bullet bulletPrefab;

    private List<Bullet> _bulletInstances = new List<Bullet>();

    private void Start()
    {
        for (int i = 0; i < startAmount - 1; i++)
        {
            _bulletInstances.Add(Instantiate(bulletPrefab, transform));
        }
    }
}
