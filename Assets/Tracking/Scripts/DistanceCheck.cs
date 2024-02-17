using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceCheck : MonoBehaviour
{
  public float Distance;
  public Transform Target;

  private void Update()
  {
    Distance = Vector3.Distance(Target.position, transform.position);

  }
}
