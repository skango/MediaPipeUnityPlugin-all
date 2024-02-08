using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StabilizationIK : MonoBehaviour
{
  public float Distance;

  [SerializeField] private Transform _target;
  [SerializeField] private float _stabilizationOffset;
  [SerializeField] private float _stabilizationStrength;

  private void Update()
  {
    Distance = Vector3.Distance(transform.position, _target.position);
    float factor = Mathf.Clamp(Distance / _stabilizationOffset, 0f, 10f);
    transform.position = Vector3.MoveTowards(transform.position, _target.position, _stabilizationStrength * factor);
  }
}
