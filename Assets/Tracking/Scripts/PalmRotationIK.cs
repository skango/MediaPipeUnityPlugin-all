using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PalmRotationIK : MonoBehaviour
{
  public Transform pointA;
  public Transform pointB;
  public Transform pointC;

  void Start()
  {

  }

  private void Update()
  {
    // Get vectors AB and AC
    Vector3 AB = pointB.position - pointA.position;
    Vector3 AC = pointC.position - pointA.position;

    // Calculate the normal vector of the plane
    Vector3 normal = Vector3.Cross(AB, AC).normalized;

    // Calculate the angle of rotation about the Y-axis
    float yRotation = Mathf.Atan2(normal.x, normal.z) * Mathf.Rad2Deg;

    // Calculate the angle of rotation about the X-axis
    float xRotation = Mathf.Atan2(normal.y, normal.z) * Mathf.Rad2Deg;

    // Output the rotation angles
    Debug.Log("Rotation about Y-axis: " + yRotation + " degrees");
    Debug.Log("Rotation about X-axis: " + xRotation + " degrees");
  }
}
