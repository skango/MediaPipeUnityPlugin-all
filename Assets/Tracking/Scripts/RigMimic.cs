using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RigMimic : MonoBehaviour
{
  [SerializeField] private Transform _target;
  [SerializeField] private Transform _mimic;

  private List<Transform> _targetBones = new List<Transform>();
  private List<Transform> _mimicBones = new List<Transform>();

  // Dictionary to store the smoothing factors for each bone
  private Dictionary<Transform, float> _positionSmoothingFactors = new Dictionary<Transform, float>();
  private Dictionary<Transform, float> _rotationSmoothingFactors = new Dictionary<Transform, float>();

  // Smoothing parameters
  [SerializeField] private float _defaultPositionSmoothness = 5f;
  [SerializeField] private float _minPositionSmoothness = 1f;
  [SerializeField] private float _maxPositionSmoothness = 7f;

  [SerializeField] private float _defaultRotationSmoothness = 5f;
  [SerializeField] private float _minRotationSmoothness = 1f;
  [SerializeField] private float _maxRotationSmoothness = 7f;

  private void Awake()
  {
    _targetBones = _target.GetComponentsInChildren<Transform>(includeInactive: true).ToList();
    _mimicBones = _mimic.GetComponentsInChildren<Transform>(includeInactive: true).ToList();

    // Initialize smoothing factors for each bone
    foreach (Transform bone in _mimicBones)
    {
      _positionSmoothingFactors[bone] = _defaultPositionSmoothness;
      _rotationSmoothingFactors[bone] = _defaultRotationSmoothness;
    }
  }

  private void Update()
  {
    for (int i = 0; i < _targetBones.Count; i++)
    {
      if (!_mimicBones[i].gameObject.activeInHierarchy)
        continue;

      Transform targetBone = _targetBones[i];
      Transform mimicBone = _mimicBones[i];

      // Calculate the magnitude of position and rotation movement for this bone
      float positionMovementMagnitude = Vector3.Distance(mimicBone.localPosition, targetBone.localPosition);
      float rotationMovementMagnitude = Quaternion.Angle(mimicBone.localRotation, targetBone.localRotation);

      // Calculate the smoothing factors based on movement magnitudes
      float positionSmoothingFactor = Mathf.Lerp(_maxPositionSmoothness, _minPositionSmoothness, Mathf.InverseLerp(0f, 1f, positionMovementMagnitude));
      float rotationSmoothingFactor = Mathf.Lerp(_maxRotationSmoothness, _minRotationSmoothness, Mathf.InverseLerp(0f, 180f, rotationMovementMagnitude));

      // Apply smoothing based on the calculated factors
      Vector3 smoothedPosition = Vector3.Lerp(mimicBone.localPosition, targetBone.localPosition, Time.deltaTime * positionSmoothingFactor);
      Quaternion smoothedRotation = Quaternion.Lerp(mimicBone.localRotation, targetBone.localRotation, Time.deltaTime * rotationSmoothingFactor);

      // Update mimic bone's position and rotation
      mimicBone.localPosition = smoothedPosition;
      mimicBone.localRotation = smoothedRotation;
    }
  }
}

