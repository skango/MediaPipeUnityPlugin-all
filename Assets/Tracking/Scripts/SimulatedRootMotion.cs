using UnityEngine;

public class SimulatedRootMotion : MonoBehaviour
{
  [SerializeField] private Transform leftFoot;
  [SerializeField] private Transform rightFoot;
  [SerializeField] private Transform pelvis;
  [SerializeField] private float movementScale = 1f; // Scaling factor for movement

  private Vector3 initialLeftFootPosition;
  private Vector3 initialRightFootPosition;

  void Start()
  {
    // Record initial positions of the left and right feet
    initialLeftFootPosition = leftFoot.position;
    initialRightFootPosition = rightFoot.position;
  }

  void Update()
  {
    // Calculate the difference in position between the initial and current foot positions
    Vector3 leftFootMovement = leftFoot.position - initialLeftFootPosition;
    Vector3 rightFootMovement = rightFoot.position - initialRightFootPosition;

    // Translate the movement from world space to local space relative to the pelvis
    Vector3 localLeftFootMovement = pelvis.InverseTransformDirection(leftFootMovement);
    Vector3 localRightFootMovement = pelvis.InverseTransformDirection(rightFootMovement);

    // Calculate averaged foot movement (taking into account forward and backward movements)
    Vector3 averagedFootMovement = (localLeftFootMovement - localRightFootMovement) / 2f;

    // Apply the averaged foot movement to the character's position with scaling and deltaTime
    transform.position += averagedFootMovement * movementScale * Time.deltaTime;

    // Rotate the character based on the pelvis rotation
    //transform.rotation = pelvis.rotation;

    // Update initial foot positions for the next frame
    initialLeftFootPosition = leftFoot.position;
    initialRightFootPosition = rightFoot.position;
  }
}
