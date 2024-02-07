using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerIK : MonoBehaviour
{
  [Header("Scene Objects")]
  [SerializeField] private Transform _referencePoint;
  [SerializeField] private Transform _referencePalm;
  [SerializeField] private Transform _currentPalm;

  [Header("2D Configurations")]
  public float CurrentRefDistance;
  public float CurrentRefStretch;
  [SerializeField] private float _maxRefDistance;
  [SerializeField] private float _minRefDistance;

  [Header("3D Configurations")]
  public float CurrentDistance;
  [SerializeField] private float _lerpSpeed;
  [SerializeField] private float _maxX;
  [SerializeField] private float _minX;
  [SerializeField] private float _maxY;
  [SerializeField] private float _minY;
  [SerializeField] private float _maxZ;
  [SerializeField] private float _minZ;

  private void Update()
  {
    CurrentRefDistance = Vector3.Distance(_referencePoint.position, _referencePalm.position);
    CurrentDistance = Vector3.Distance(_currentPalm.position, transform.position);

    CurrentRefStretch = Mathf.Clamp((CurrentRefDistance - _minRefDistance) / (_maxRefDistance - _minRefDistance), 0f, 1f);

    // Interpolate between min and max values based on CurrentRefStretch
    float x = Mathf.Lerp(_maxX, _minX, CurrentRefStretch);
    float y = Mathf.Lerp(_maxY, _minY, CurrentRefStretch);
    float z = Mathf.Lerp(_maxZ, _minZ, CurrentRefStretch);

    // Move the transform locally
    Vector3 targetPosition = new Vector3(x, y, z);

    transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, _lerpSpeed * Time.deltaTime);
  }
}
