using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigMovement : MonoBehaviour
{
  [SerializeField] private DepthCalculator _depthCalculator;
  [SerializeField] private float _depthMovementModifier;
  [SerializeField] private float _horizontalMovementModifier;
  [SerializeField] private float _movementSmoothSpeed;

  private Vector3 _startingLocation;
  private Vector3 _targetLocation;

  private void Awake()
  {
    _startingLocation = transform.position;
  }

  private void Update()
  {
    if (!_depthCalculator.IsActive)
      return;

    float depthRatio = 1f - _depthCalculator.GetDepth();

    Vector3 forwardMovement = transform.forward.normalized * _depthMovementModifier * depthRatio;
    Vector3 horizontalMovement = transform.right * _horizontalMovementModifier * _depthCalculator.GetHorizontalRatio();

    _targetLocation = _startingLocation + forwardMovement + horizontalMovement;

    transform.position = Vector3.MoveTowards(transform.position, _targetLocation, _movementSmoothSpeed * Time.deltaTime);
  }
}
