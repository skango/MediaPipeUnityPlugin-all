using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PelvisIK : MonoBehaviour
{
  [SerializeField] private Vector3 _offset;
  [SerializeField] private float _modifier;
  [SerializeField] private Transform _leftThigh;
  [SerializeField] private Transform _rightThigh;

  private Vector3 _previousCenterPoint;

  private void Awake()
  {
    _previousCenterPoint = (_leftThigh.position + _rightThigh.position) / 2f;
  }

  private void LateUpdate()
  {
    Vector3 currentCenterPoint = (_leftThigh.position + _rightThigh.position) / 2f;

    Vector3 delta = currentCenterPoint - _previousCenterPoint;
    Vector3 modifiedDelta = delta * _modifier;

    transform.position += modifiedDelta;

    _previousCenterPoint = currentCenterPoint;
  }
}
