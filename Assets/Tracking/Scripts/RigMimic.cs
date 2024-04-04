using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RigMimic : MonoBehaviour
{
  [SerializeField] private Transform _target;

  private Dictionary<Transform, MimicBoneMap> _targetToMimicMap = new Dictionary<Transform, MimicBoneMap>();
  private Dictionary<Transform, Transform> _targetToIdleMap = new Dictionary<Transform, Transform>();

  [SerializeField] private float _minPositionSmoothness = 1f;
  [SerializeField] private float _maxPositionSmoothness = 7f;

  [SerializeField] private float _minRotationSmoothness = 1f;
  [SerializeField] private float _maxRotationSmoothness = 7f;
  [SerializeField] private List<Transform> _excludedBones = new List<Transform>();

  private void Awake()
  {
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
          _targetToMimicMap.Add(boneTransformA, new MimicBoneMap(boneTransformB, boneTransformB.GetComponent<RigBoneConstraint>()));
        }
      }
      catch (Exception ex)
      {
        Debug.Log($"TESTING {ex.Message}");
      }
    }
  }

  private void Update()
  {
    Dictionary<Transform, MimicBoneMap> mapToUse = _targetToMimicMap;

    foreach (var mapEntry in mapToUse)
    {
      Transform targetBone = mapEntry.Key;
      MimicBoneMap mimicBoneMap = mapEntry.Value;

      if (transform == targetBone || transform == mimicBoneMap.Bone)
      {
        continue;
      }

      bool foundExcluded = false;

      foreach (Transform bone in _excludedBones)
      {
        if (mimicBoneMap.Bone == bone || mimicBoneMap.Bone.IsChildOf(bone))
        {
          foundExcluded = true;
        }
      }

      if (foundExcluded)
        continue;

      float positionMovementMagnitude = Vector3.Distance(mimicBoneMap.Bone.localPosition, targetBone.localPosition);
      float rotationMovementMagnitude = Quaternion.Angle(mimicBoneMap.Bone.localRotation, targetBone.localRotation);

      float positionSmoothingFactor = Mathf.Lerp(_maxPositionSmoothness, _minPositionSmoothness, Mathf.InverseLerp(0f, 1f, positionMovementMagnitude));
      float rotationSmoothingFactor = Mathf.Lerp(_maxRotationSmoothness, _minRotationSmoothness, Mathf.InverseLerp(0f, 180f, rotationMovementMagnitude));

      Vector3 smoothedPosition = Vector3.Lerp(mimicBoneMap.Bone.localPosition, targetBone.localPosition, Time.deltaTime * positionSmoothingFactor);
      Quaternion smoothedRotation = Quaternion.Lerp(mimicBoneMap.Bone.localRotation, targetBone.localRotation, Time.deltaTime * rotationSmoothingFactor);

      mimicBoneMap.Bone.localPosition = smoothedPosition;
      mimicBoneMap.Bone.localRotation = smoothedRotation;
    }

    float ClampAngle(float angle, float min, float max)
    {
      angle = NormalizeAngle(angle);
      return Mathf.Clamp(angle, min, max);
    }

    float NormalizeAngle(float angle)
    {
      while (angle > 180)
        angle -= 360;
      while (angle < -180)
        angle += 360;
      return angle;
    }
  }

  [System.Serializable]
  public class MimicBoneMap
  {
    public Transform Bone;
    public RigBoneConstraint Constraint;

    public MimicBoneMap(Transform bone, RigBoneConstraint constraint)
    {
      Bone = bone;
      Constraint = constraint;
    }
  }
}

