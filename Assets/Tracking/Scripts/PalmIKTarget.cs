using UnityEngine;

public class PalmIKTarget : MonoBehaviour
{
  [SerializeField] private Transform _firstPoint2D;
  [SerializeField] private Transform _secondPoint2D;
  [SerializeField] private Transform _palmPoint2D;

  public float distanceAB_2D = 2.27f;
  public float distanceAC_2D = 3f;
  public float distanceBC_2D = 3.6f;

  [SerializeField] private Transform _directionTarget;
  [SerializeField] private Vector3 _directionOffset;
  [SerializeField] private float _directionMult;

  private Vector3 _previousValidA;
  private Vector3 _previousValidB;

  private Vector3 _firstPoint3D;
  private Vector3 _secondPoint3D;
  private Vector3 _palmPoint3D;

  private void Start()
  {
    _firstPoint3D = _firstPoint2D.transform.position;
    _secondPoint3D = _secondPoint2D.transform.position;
    _palmPoint3D = _palmPoint2D.transform.position;
  }

  void LateUpdate()
  {
    CalculatePositions();
  }

  void CalculatePositions()
  {
    float xA = _firstPoint2D.position.x;
    float yA = _firstPoint2D.position.y;
    float xB = _secondPoint2D.position.x;
    float yB = _secondPoint2D.position.y;
    float xC = _palmPoint2D.position.x;
    float yC = _palmPoint2D.position.y;

    float zA = 0f;
    float zB = 0f;

    int maxIterations = 1000;
    float tolerance = 0.001f;

    for (int i = 0; i < maxIterations; i++)
    {
      float squaredDistanceAC = (xA - xC) * (xA - xC) + (yA - yC) * (yA - yC) + zA * zA;
      float squaredDistanceBC = (xB - xC) * (xB - xC) + (yB - yC) * (yB - yC) + zB * zB;
      float squaredDistanceAB = (xA - xB) * (xA - xB) + (yA - yB) * (yA - yB) + (zA - zB) * (zA - zB);

      float errorAC = Mathf.Abs(Mathf.Sqrt(squaredDistanceAC) - distanceAC_2D);
      float errorBC = Mathf.Abs(Mathf.Sqrt(squaredDistanceBC) - distanceBC_2D);
      float errorAB = Mathf.Abs(Mathf.Sqrt(squaredDistanceAB) - distanceAB_2D);

      if (errorAC < tolerance && errorBC < tolerance && errorAB < tolerance)
      {
        break;
      }

      zA += (distanceAC_2D - Mathf.Sqrt(squaredDistanceAC)) * 0.5f;
      zB += (distanceBC_2D - Mathf.Sqrt(squaredDistanceBC)) * 0.5f;
    }

    Vector3 pointA_3D = new Vector3(xA, yA, zA);
    Vector3 pointB_3D = new Vector3(xB, yB, zB);
    Vector3 pointC_3D = new Vector3(xC, yC, 0f);

    if(Vector3.Distance(_previousValidA, pointA_3D) > 100)
    {
      _firstPoint3D = new Vector3(pointA_3D.x, pointA_3D.y, _previousValidA.z);
    }
    else
    {
      _firstPoint3D = pointA_3D;
      _previousValidA = _firstPoint3D;
    }

    if (Vector3.Distance(_previousValidB, pointB_3D) > 100)
    {
      _secondPoint3D = new Vector3(pointB_3D.x, pointB_3D.y, _previousValidB.z);
    }
    else
    {
      _secondPoint3D = pointB_3D;
      _previousValidB = _secondPoint3D;
    }

    _palmPoint3D = pointC_3D;

    Vector3 aimTarget = (_firstPoint3D + _secondPoint3D) / 2f;
    Vector3 direction = (aimTarget - _palmPoint3D).normalized;

    _directionTarget.localPosition = _directionOffset + direction * _directionMult;
  }
}
