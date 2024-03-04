using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootIK : MonoBehaviour
{
  [SerializeField] private Transform _refBone;
  [SerializeField] private Transform _targetBone;
  [SerializeField] private Vector3 _offset;


  private void LateUpdate()
  {
    Vector3 direction = _targetBone.position - new Vector3(_refBone.position.x, _targetBone.position.y, _refBone.position.z);
    Quaternion lookRotation = Quaternion.LookRotation(direction);
    transform.localRotation = lookRotation * Quaternion.Euler(_offset);
  }
}
