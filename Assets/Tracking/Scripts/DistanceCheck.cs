using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceCheck : MonoBehaviour
{
  public float Distance;
  public Transform Target;

  private void Update()
  {
    Distance = Vector3.Distance(new Vector3(Target.position.x, 0, 0), new Vector3(transform.position.x, 0, 0));

  }
}
