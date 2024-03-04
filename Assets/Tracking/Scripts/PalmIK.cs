using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PalmIK : MonoBehaviour
{
  [SerializeField] private Transform _firstPalmPoint;
  [SerializeField] private Transform _secondPalmPoint;
  [SerializeField] private Transform _palmPoint;
  [SerializeField] private Transform _target;
  [SerializeField] private Vector3 _offset;

  private void LateUpdate()
  {
    float additionalX = 0f;

    if (_firstPalmPoint.transform.position.x > _secondPalmPoint.transform.position.x)
    {
      additionalX = 100;
    }
    else
    {
      additionalX = -100;
    }

    transform.LookAt( _target );
    Quaternion additionalRotation = Quaternion.Euler(new Vector3(_offset.x + additionalX, _offset.y, _offset.z));
    transform.rotation = transform.rotation * additionalRotation;
  }
}
