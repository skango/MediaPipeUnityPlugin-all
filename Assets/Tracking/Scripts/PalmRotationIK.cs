using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PalmRotationIK : MonoBehaviour
{
  public float CurrentDistance;

  [SerializeField] private float _maxRefDistance;
  [SerializeField] private float _minRefDistance;
  [SerializeField] private float _maxXRotation;
  [SerializeField] private float _minXRotation;
  [SerializeField] private Transform _backRotationPoint;
  [SerializeField] private Transform _frontRotationPoint;

  private void LateUpdate()
  {
    CurrentDistance = Vector3.Distance(_frontRotationPoint.position, _backRotationPoint.position);
    // Calculate the interpolation factor based on the CurrentDistance
    float distanceFactor = Mathf.Clamp01((CurrentDistance - _minRefDistance) / (_maxRefDistance - _minRefDistance));

    float targetXRotation = _minXRotation - _maxXRotation * (1f-distanceFactor);
    // Apply the rotation locally
    transform.localRotation = Quaternion.Euler(targetXRotation, 0, 0f);
  }
}
