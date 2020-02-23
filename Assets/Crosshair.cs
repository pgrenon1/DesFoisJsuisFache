using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : SingletonMonoBehaviour<Crosshair>
{
    public float returnRate = 1f;
    public AnimationCurve returnCurve;

    public float TargetSpreadValue { get; set; } = 0f;

    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        TargetSpreadValue -= Time.deltaTime * returnRate * returnCurve.Evaluate(TargetSpreadValue);

        TargetSpreadValue = Mathf.Clamp01(TargetSpreadValue);

        _animator.SetFloat("Spread", TargetSpreadValue);
    }
}
