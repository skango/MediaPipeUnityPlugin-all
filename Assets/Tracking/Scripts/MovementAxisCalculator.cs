using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthCalculator : MonoBehaviour
{
  public bool IsActive;

  [Header("3D Bones")]
  [SerializeField] private Transform _leftHips_3D;
  [SerializeField] private Transform _leftShoulder_3D;

  [Header("2D Bones")]
  [SerializeField] private Transform _leftHips_2D;
  [SerializeField] private Transform _leftShoulder_2D;

  [Header("Configurables")]
  [SerializeField] private float _angleSlack;
  [SerializeField] private float _startingVerticalLength;
  [SerializeField] private float _startingHorizontalPoint;
  [SerializeField] private float _angle;
  [SerializeField] private float _length_2D;
  [SerializeField] private float _length_2D_Adjusted;
  [SerializeField] private float _depthRatio;
  [SerializeField] private float _horizontalRatio;

  public float GetDepth() => _depthRatio;
  public float GetHorizontalRatio() => _horizontalRatio;

  private void Awake()
  {
    StartCoroutine(DelayedSet());
  }

  IEnumerator DelayedSet()
  {
    yield return new WaitForSeconds(3f);

    IsActive = true;
    _startingVerticalLength = CalculateAdjustedLength();
    _startingHorizontalPoint = _leftHips_2D.position.x;
  }

  private void Update()
  {
    _length_2D_Adjusted = CalculateAdjustedLength();

    _depthRatio = (float)Math.Round(Mathf.Abs(_startingVerticalLength / _length_2D_Adjusted), 2);

    float currentHorizontalPoint = _leftHips_2D.position.x;

    _horizontalRatio = (currentHorizontalPoint - _startingHorizontalPoint) / _startingHorizontalPoint;
  }

  private float CalculateAdjustedLength()
  {
    Vector3 directionAB_3D = _leftShoulder_3D.position - _leftHips_3D.position;
    Vector3 directionAB_YZ = new Vector3(0, directionAB_3D.y, directionAB_3D.z).normalized;
    _angle = Vector3.Angle(directionAB_YZ, Vector3.up);
    _length_2D = Vector3.Distance(_leftHips_2D.position, _leftShoulder_2D.position);
    float modifiedLength = (float)Math.Round(_length_2D * Mathf.Cos(Mathf.Deg2Rad * _angle), 2);
    float calculatedLength = (float)Math.Round(Mathf.Abs(_length_2D - modifiedLength) + _length_2D, 2);

    return _angleSlack >= _angle? _length_2D : calculatedLength;
  }
}
