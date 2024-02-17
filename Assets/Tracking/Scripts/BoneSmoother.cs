using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoneSmoother : MonoBehaviour
{
  public string ID;

  [SerializeField] private float _minPositionSmoothness = 1f;
  [SerializeField] private float _maxPositionSmoothness = 7f;

  [SerializeField] private float _minRotationSmoothness = 1f;
  [SerializeField] private float _maxRotationSmoothness = 7f;

  private Transform _target;

  private void Start()
  {
    _target = FindObjectsOfType<BoneSmootherTarget>().ToList().First(x => x.ID == ID).transform;
  }

  private void Update()
  {
    float positionMovementMagnitude = Vector3.Distance(transform.localPosition, _target.localPosition);
    float rotationMovementMagnitude = Quaternion.Angle(transform.localRotation, _target.localRotation);

    // Calculate the smoothing factors based on movement magnitudes
    float positionSmoothingFactor = Mathf.Lerp(_maxPositionSmoothness, _minPositionSmoothness, Mathf.InverseLerp(0f, 1f, positionMovementMagnitude));
    float rotationSmoothingFactor = Mathf.Lerp(_maxRotationSmoothness, _minRotationSmoothness, Mathf.InverseLerp(0f, 180f, rotationMovementMagnitude));

    // Apply smoothing based on the calculated factors
    Vector3 smoothedPosition = Vector3.Lerp(transform.localPosition, _target.localPosition, Time.deltaTime * positionSmoothingFactor);
    Quaternion smoothedRotation = Quaternion.Lerp(transform.localRotation, _target.localRotation, Time.deltaTime * rotationSmoothingFactor);

    // Update mimic bone's position and rotation
    transform.localPosition = smoothedPosition;
    transform.localRotation = smoothedRotation;
  }
}
