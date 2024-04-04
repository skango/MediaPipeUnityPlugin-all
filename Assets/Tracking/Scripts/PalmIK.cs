using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PalmIK : MonoBehaviour
{
  [SerializeField] private Transform _firstPalmPoint;
  [SerializeField] private Transform _secondPalmPoint;
  [SerializeField] private float _maxDistance;
  [SerializeField] private float _minDistance;
  [SerializeField] private float _maxAdditionalRotation = 70;
  [SerializeField] private float _distance;

  [SerializeField] private Transform _target;
  [SerializeField] private Vector3 _offset;

  private void LateUpdate()
  {
    float additionalX;

    _distance = Vector3.Distance(_firstPalmPoint.transform.position, _secondPalmPoint.transform.position);

    if (_firstPalmPoint.transform.position.x > _secondPalmPoint.transform.position.x)
    {
      additionalX = _maxAdditionalRotation * (Mathf.Clamp(_distance / _maxDistance, 0f, 1f));
    }
    else
    {
      additionalX = -_maxAdditionalRotation * (Mathf.Clamp(_distance / _maxDistance, 0f, 1f));
    }

    transform.LookAt( _target);
    Quaternion additionalRotation = Quaternion.Euler(new Vector3(_offset.x + additionalX, _offset.y, _offset.z));
    transform.rotation = transform.rotation * additionalRotation;
  }
}
