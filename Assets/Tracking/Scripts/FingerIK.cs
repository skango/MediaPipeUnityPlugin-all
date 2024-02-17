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
  [SerializeField] private Vector3 _maxPosition;
  [SerializeField] private Vector3 _minPosition;

  private void Update()
  {
    CurrentRefDistance = Vector3.Distance(_referencePoint.position, _referencePalm.position);
    CurrentDistance = Vector3.Distance(_currentPalm.position, transform.position);

    CurrentRefStretch = Mathf.Clamp((CurrentRefDistance - _minRefDistance) / (_maxRefDistance - _minRefDistance), 0f, 1f);

    // Interpolate between min and max values based on CurrentRefStretch
    float x = Mathf.Lerp(_minPosition.x, _maxPosition.x, CurrentRefStretch);
    float y = Mathf.Lerp(_minPosition.y, _maxPosition.y, CurrentRefStretch);
    float z = Mathf.Lerp(_minPosition.z, _maxPosition.z, CurrentRefStretch);

    // Move the transform locally
    Vector3 targetPosition = new Vector3(x, y, z);

    transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, _lerpSpeed * Time.deltaTime);
  }
}
