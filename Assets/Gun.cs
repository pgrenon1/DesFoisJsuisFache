using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Gun : BaseBehaviour
{
    public Collider pickupCollider;
    public Poem poem;
    public AudioSource emptySound;
    public Bullet bulletPrefab;

    private int _index;
    private float _currentIndexFloat;
    private Word _currentWord;
    //private List<Bullet> _bullets = new List<Bullet>();

    //private void Start()
    //{
    //    _words = new List<Word>(poem.words);

    //    foreach (var word in _words)
    //    {
    //        var newBullet = Instantiate(bulletPrefab, transform);
    //        newBullet.Word = word;
    //        newBullet.AudioSource.clip = word.clip;
    //        newBullet.gameObject.SetActive(false);
    //        //_bullets.Add(newBullet);
    //    }
    //}

    private void Update()
    {
        var scrollValue = Input.GetAxis("Mouse ScrollWheel");
        _currentIndexFloat += scrollValue;

        if (_currentIndexFloat > poem.words.Count - 1)
            _currentIndexFloat = 0;
        else if (_currentIndexFloat < 0)
            _currentIndexFloat = poem.words.Count - 1;

        Debug.Log(poem.words[Mathf.FloorToInt(_currentIndexFloat)].word);

        //if (scrollValue > 0)
        //{

        //    if (_currentIndex > poem.words.Count - 1)
        //        _currentIndex = 0;


        //    //var startingIndex = _currentIndex;
        //    //_currentIndex++;

        //    //var i = 0;
        //    //while (poem.words[_currentIndex].IsUsed)
        //    //{
        //    //    _currentIndex++;

        //    //    if (_currentIndex == poem.words.Count - 1)
        //    //        _currentIndex = 0;

        //    //    if (i++ > 300)
        //    //    {
        //    //        Debug.Log("Busted");
        //    //        break;
        //    //    }
        //    //}
        //    Debug.Log(poem.words[_currentIndex].word);
        //}
        //else if (scrollValue < 0)
        //{
        //    _currentIndex--;

        //    if (_currentIndex < 0)
        //        _currentIndex = poem.words.Count - 1;
        //}
    }

    public void Shoot()
    {
        //if (_index > _bullets.Count - 1)
        //{
        //    emptySound.Play();
        //}
        //else
        //{
        //var bullet = _bullets[_index];
        var bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity, transform);
        bullet.Word = poem.words[Mathf.FloorToInt(_currentIndexFloat)];
        bullet.AudioSource.clip = bullet.Word.clip;
        bullet.transform.parent = null;
        bullet.gameObject.SetActive(true);
        bullet.Move(Camera.main.transform.forward);
        //}

        _index++;
    }
}
