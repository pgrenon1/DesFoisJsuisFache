using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Gun : BaseBehaviour
{
    public Poem poem;
    public AudioClip clipEmptyClip;
    public Bullet bulletPrefab;

    private int _index;
    private List<KeyValuePair<string, AudioClip>> _bulletsData;
    private List<Bullet> _bullets = new List<Bullet>();

    private void Start()
    {
        _bulletsData = new List<KeyValuePair<string, AudioClip>>(poem.words);

        foreach (var bulletData in _bulletsData)
        {
            var newBullet = Instantiate(bulletPrefab, transform);
            newBullet.AudioSource.clip = bulletData.Value;
            //newBullet.gameObject.SetActive(false);
            _bullets.Add(newBullet);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        if (_index > _bullets.Count - 1)
        {
            AudioSource.clip = clipEmptyClip;
        }
        else
        {
            //_bullets[_index].gameObject.SetActive(true);
            _bullets[_index].AudioSource.PlayOneShot(_bulletsData[_index].Value);
        }

        _index++;
    }
}
