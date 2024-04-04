using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigBoneConstraint : MonoBehaviour
{
  [SerializeField] private Transform _target;

  private void LateUpdate()
  {
    if (_target != null)
    {
      Quaternion targetRotation = Quaternion.LookRotation(_target.forward, Vector3.up);
      transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, targetRotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }
  }
}
