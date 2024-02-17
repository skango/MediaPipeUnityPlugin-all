using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadIK : MonoBehaviour
{
  [SerializeField] private Transform _backLeftPoint;
  [SerializeField] private Transform _backRightPoint;
  [SerializeField] private Transform _frontLeftPoint;
  [SerializeField] private Transform _frontRightPoint;
  [SerializeField] private Transform _newPoint;
  [SerializeField] private float _lengthen;
  [SerializeField] private Vector3 _offset;

  private void Update()
  {
    // Calculate middle point between back points
    Vector3 middleBackPoint = (_backLeftPoint.position + _backRightPoint.position) / 2f;

    // Calculate middle point between front points
    Vector3 middleFrontPoint = (_frontLeftPoint.position + _frontRightPoint.position) / 2f;

    // Calculate direction from center back point to center front point
    Vector3 direction = (middleFrontPoint - middleBackPoint).normalized;

    // Position this GameObject in center point of front points
    _newPoint.position = middleFrontPoint;

    // Move it in direction and lengthen it by _lengthen
    _newPoint.Translate(direction * _lengthen, Space.World);
    _newPoint.position += _offset;

    transform.LookAt(_newPoint.position);
  }
}
