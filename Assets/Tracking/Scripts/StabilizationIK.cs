using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StabilizationIK : MonoBehaviour
{
  [SerializeField] private float _previousDelta;

  [SerializeField] private Transform _previousBone;
  [SerializeField] private Transform _target;
  [SerializeField] private Transform _stabilizationPoint;
  [SerializeField] private float _stabilizationSlack;
  [SerializeField] private float _stabilizationSpeed;
  [SerializeField] private float _Lengthen;

  private Vector3 _previousStabilizationPoint;

  private void Awake()
  {
    if(_stabilizationPoint && _target)
    {
      _previousStabilizationPoint = _stabilizationPoint.position;
      transform.position = _target.position + (_previousBone.position - _target.position).normalized * _Lengthen;
    }
  }

  private void Update()
  {
    if(_stabilizationPoint && _target && _previousBone)
    {
      _previousDelta = Vector3.Distance(_previousStabilizationPoint, _stabilizationPoint.position);

      if (_previousDelta > _stabilizationSlack)
      {
        transform.position = _target.position + (_previousBone.position - _target.position).normalized * _Lengthen;
      }
      else
      {
        transform.position = Vector3.MoveTowards(transform.position, _target.position + (_previousBone.position - _target.position).normalized * _Lengthen, _stabilizationSpeed * Time.deltaTime);
      }

      _previousStabilizationPoint = _stabilizationPoint.position;
    }
    else if (_previousBone && _target)
    {
      transform.position = _target.position + (_previousBone.position - _target.position).normalized * _Lengthen;
    }
  }
}
