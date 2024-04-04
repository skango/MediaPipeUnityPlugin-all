using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneLengthen : MonoBehaviour
{
  [SerializeField] private Transform _parentBone;
  [SerializeField] private Transform _childBone;
  [SerializeField] private Transform _offsetBone;
  [SerializeField] private float _modifier;

  private void Update()
  {
    if (!_offsetBone)
    {
      transform.position = _childBone.position + (_childBone.position - _parentBone.position).normalized * _modifier;
    }
    else
    {
      transform.position = _offsetBone.position + _childBone.position + (_childBone.position - _parentBone.position).normalized * _modifier;
    }
  }
}
