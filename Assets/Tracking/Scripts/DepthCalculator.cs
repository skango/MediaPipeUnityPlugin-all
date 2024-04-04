using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthCalculator : MonoBehaviour
{
  public bool IsActive;

  [SerializeField] private Transform _pointA_3D;
  [SerializeField] private Transform _pointB_3D;

  [SerializeField] private Transform _pointA_2D;
  [SerializeField] private Transform _pointB_2D;

  [SerializeField] private float _angleSlack;

  [SerializeField] private float _startingLength;
  [SerializeField] private float _angle;
  [SerializeField] private float _length_2D;
  [SerializeField] private float _length_2D_Adjusted;
  [SerializeField] private float _depthRatio;

  public float GetDepth() => _depthRatio;

  private void Awake()
  {
    StartCoroutine(DelayedSet());
  }

  IEnumerator DelayedSet()
  {
    yield return new WaitForSeconds(3f);

    IsActive = true;
    _startingLength = CalculateAdjustedLength();
  }

  private void Update()
  {
    _length_2D_Adjusted = CalculateAdjustedLength();

    _depthRatio = (float)Math.Round(Mathf.Abs(_startingLength / _length_2D_Adjusted), 2);
  }

  private float CalculateAdjustedLength()
  {
    Vector3 directionAB_3D = _pointB_3D.position - _pointA_3D.position;
    Vector3 directionAB_YZ = new Vector3(0, directionAB_3D.y, directionAB_3D.z).normalized;
    _angle = Vector3.Angle(directionAB_YZ, Vector3.up);
    _length_2D = Vector3.Distance(_pointA_2D.position, _pointB_2D.position);
    float modifiedLength = (float)Math.Round(_length_2D * Mathf.Cos(Mathf.Deg2Rad * _angle), 2);
    float calculatedLength = (float)Math.Round(Mathf.Abs(_length_2D - modifiedLength) + _length_2D, 2);

    return _angleSlack >= _angle? _length_2D : calculatedLength;
  }
}
