using UnityEngine;

public class HandVisualization : MonoBehaviour
{
  // Objects representing points A, B, and C in 2D space
  public Transform pointA;
  public Transform pointB;
  public Transform pointC;

  // Define the distances between the points observed in 2D space
  public float distanceAB_2D = 2.27f;
  public float distanceAC_2D = 3f;
  public float distanceBC_2D = 3.6f;

  // Define the GameObjects for points A, B, and C
  [SerializeField] private GameObject _a;
  [SerializeField] private GameObject _b;
  [SerializeField] private GameObject _c;

  [SerializeField] private Transform _rotationTarget;

  private Vector3 _previousValidA;
  private Vector3 _previousValidB;

  private void Start()
  {
    _a.transform.position = pointA.transform.position;
    _b.transform.position = pointB.transform.position;
    _c.transform.position = pointC.transform.position;
  }

  void LateUpdate()
  {
    // Calculate the positions of points A, B, and C in 3D space
    CalculatePositions();

    // Calculate and apply the rotation
    CalculateRotation();
  }

  void CalculatePositions()
  {
    // Get the X and Y values from the objects in 2D space
    float xA = pointA.position.x;
    float yA = pointA.position.y;
    float xB = pointB.position.x;
    float yB = pointB.position.y;
    float xC = pointC.position.x;
    float yC = pointC.position.y;

    // Initialize the Z coordinates of points A and B
    float zA = 0f;
    float zB = 0f;

    // Define the maximum number of iterations and the tolerance for convergence
    int maxIterations = 1000;
    float tolerance = 0.001f;

    // Perform iterative adjustment of Z coordinates to meet the observed distances
    for (int i = 0; i < maxIterations; i++)
    {
      // Calculate the squared distances between points A, B, and C
      float squaredDistanceAC = (xA - xC) * (xA - xC) + (yA - yC) * (yA - yC) + zA * zA;
      float squaredDistanceBC = (xB - xC) * (xB - xC) + (yB - yC) * (yB - yC) + zB * zB;
      float squaredDistanceAB = (xA - xB) * (xA - xB) + (yA - yB) * (yA - yB) + (zA - zB) * (zA - zB);

      // Calculate the errors in distances compared to observed distances
      float errorAC = Mathf.Abs(Mathf.Sqrt(squaredDistanceAC) - distanceAC_2D);
      float errorBC = Mathf.Abs(Mathf.Sqrt(squaredDistanceBC) - distanceBC_2D);
      float errorAB = Mathf.Abs(Mathf.Sqrt(squaredDistanceAB) - distanceAB_2D);

      // Check if the errors are within tolerance
      if (errorAC < tolerance && errorBC < tolerance && errorAB < tolerance)
      {
        // If the errors are within tolerance, break out of the loop
        break;
      }

      // Update the Z coordinates based on the errors
      zA += (distanceAC_2D - Mathf.Sqrt(squaredDistanceAC)) * 0.5f;
      zB += (distanceBC_2D - Mathf.Sqrt(squaredDistanceBC)) * 0.5f;
    }

    // Set the 3D positions of points A, B, and C
    Vector3 pointA_3D = new Vector3(xA, yA, zA);
    Vector3 pointB_3D = new Vector3(xB, yB, zB);
    Vector3 pointC_3D = new Vector3(xC, yC, 0f); // Z position of point C is fixed at 0

    if(Vector3.Distance(_previousValidA, pointA_3D) > 100)
    {
      _a.transform.position = new Vector3(pointA_3D.x, pointA_3D.y, _previousValidA.z);
    }
    else
    {
      _a.transform.position = pointA_3D;
      _previousValidA = _a.transform.position;
    }

    if (Vector3.Distance(_previousValidB, pointB_3D) > 100)
    {
      _b.transform.position = new Vector3(pointB_3D.x, pointB_3D.y, _previousValidB.z);
    }
    else
    {
      _b.transform.position = pointB_3D;
      _previousValidB = _b.transform.position;
    }

    // Assign positions to the GameObjects
    _c.transform.position = pointC_3D;

    _rotationTarget.position = _c.transform.position;
  }

  void CalculateRotation()
  {
    // Calculate the normal vector of the plane formed by points A, B, and C
    Vector3 planeNormal = Vector3.Cross(_a.transform.position - _c.transform.position, _b.transform.position - _c.transform.position);

    Quaternion currentRotation = Quaternion.LookRotation(planeNormal, Vector3.up);

    // Convert the current rotation to Euler angles
    Vector3 currentEulerAngles = currentRotation.eulerAngles;

    // Invert the rotation around the Y-axis
    currentEulerAngles.y = -currentEulerAngles.y;

    // Create a new quaternion from the inverted Euler angles
    Quaternion invertedRotation = Quaternion.Euler(currentEulerAngles);

    _rotationTarget.transform.localRotation = invertedRotation;
  }

  void OnDrawGizmos()
  {
    // Set Gizmo color
    Gizmos.color = Color.blue;

    // Draw points A, B, and C
    Gizmos.DrawSphere(_a.transform.position, 0.1f);
    Gizmos.DrawSphere(_b.transform.position, 0.1f);
    Gizmos.DrawSphere(_c.transform.position, 0.1f);

    // Draw lines between points A and B, and points B and C
    Gizmos.DrawLine(_a.transform.position, _b.transform.position);
    Gizmos.DrawLine(_b.transform.position, _c.transform.position);
  }
}
