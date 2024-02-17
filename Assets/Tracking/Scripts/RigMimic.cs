using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RigMimic : MonoBehaviour
{
  [SerializeField] private Transform _target;

  private Dictionary<Transform, Transform> _targetToMimicMap = new Dictionary<Transform, Transform>();

  [SerializeField] private float _minPositionSmoothness = 1f;
  [SerializeField] private float _maxPositionSmoothness = 7f;

  [SerializeField] private float _minRotationSmoothness = 1f;
  [SerializeField] private float _maxRotationSmoothness = 7f;

  private void Awake()
  {
    if (_target == null)
    {
      _target = GameObject.Find("RawAvatar").transform;
    }

    Animator targetAnimator = _target.GetComponent<Animator>();
    Animator mimicAnimator = transform.GetComponent<Animator>();

    targetAnimator.enabled = false;
    mimicAnimator.enabled = false;

    HumanBodyBones[] allBones = (HumanBodyBones[]) System.Enum.GetValues(typeof(HumanBodyBones));

    foreach (HumanBodyBones bone in allBones)
    {
      try
      {
        Transform boneTransformA = targetAnimator.GetBoneTransform(bone);
        Transform boneTransformB = mimicAnimator.GetBoneTransform(bone);

        if (boneTransformA != null && boneTransformB != null)
        {
          _targetToMimicMap.Add(boneTransformA, boneTransformB);
        }
      }
      catch(Exception ex)
      {
        Debug.Log($"TESTING {ex.Message}");
      }
    }
  }

  private void Update()
  {
    foreach (var mapEntry in _targetToMimicMap)
    {
      Transform targetBone = mapEntry.Key;
      Transform mimicBone = mapEntry.Value;

      if (transform == targetBone || transform == mimicBone)
      {
        continue;
      }

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

