using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion;
using RootMotion.FinalIK;
using Mediapipe.Unity.Sample.Holistic;
using System.Linq;


public class IkConfig : MonoBehaviour
{
  public VRIK ik;
  public HolisticTrackingSolution solution;
  public GenericPoser poser;
  public GameObject RhandAnotationParent;
  public List<GameObject> RhandIkTargets = new List<GameObject>();
  public List<GameObject> RhandIkSources = new List<GameObject>();
  List<Vector3> previousPositions;
  public float scaleRatio = -1;

  IEnumerator Start()
  {
    scaleRatio = -1;
    yield return new WaitUntil(() => solution.landmarkPoints.Count == 33);
    yield return new WaitForSeconds(2);
   // HandVizualizer.transform.parent = ik.transform.parent;
    //HandVizualizer.transform.localPosition = Vector3.zero;
    ConfigureIk();
  }

  private void Update()
  {
   
    if (RhandIkSources.Count == 0)
    {
      try
      {
        for (int i = 4; i <= 20; i += 4)
        {
          RhandIkSources.Add(RhandAnotationParent.transform.GetChild(i).gameObject);
        }
       
        previousPositions = RhandIkSources.Select(source => source.transform.localPosition).ToList();
        //GameObject.FindObjectOfType<HolisticTrackingSolution>().targets = RhandIkTargets;
      }
      catch
      {

      }
    }

    if (RhandIkSources.Count > 0)
    {
      for (int i = 0; i < RhandIkSources.Count; i++)
      {
        // Calculate the delta position (change in position since last frame)
        Vector3 currentPosition = RhandIkSources[i].transform.localPosition;
        Vector3 deltaPosition = currentPosition;

        scaleRatio = 0.0005f;
       
        deltaPosition *= scaleRatio;

        // Convert the deltaPosition to the local space of the avatar's hand
        //Vector3 localDelta = ik.references.rightHand.InverseTransformDirection(deltaPosition);

        // Assuming deltaPosition is the movement delta obtained from MediaPipe data
        //Vector3 worldPosition = ik.references.rightHand.TransformPoint(deltaPosition);
        //Vector3 newPositionRelativeToNewParent = RhandIkTargets[i].transform.parent.InverseTransformPoint(worldPosition);


        // Apply the localDelta to move the IK target
        RhandIkTargets[i].transform.localPosition = deltaPosition;

        // Update the previous position for the next frame
        previousPositions[i] = currentPosition;
        
      }


    }
  }
  public void ConfigureIk()
  {
    ik.solver.spine.headTarget = solution.landmarkPoints[0].transform;
    ik.solver.leftArm.target = solution.landmarkPoints[16].transform;
    ik.solver.rightArm.target = solution.landmarkPoints[15].transform;
    ik.solver.leftLeg.target = solution.landmarkPoints[28].transform;
    ik.solver.rightLeg.target = solution.landmarkPoints[27].transform;
    ik.solver.leftArm.bendGoal = solution.landmarkPoints[14].transform;
    ik.solver.rightArm.bendGoal = solution.landmarkPoints[13].transform;
    ik.solver.leftLeg.bendGoal = solution.landmarkPoints[26].transform;
    ik.solver.rightLeg.bendGoal = solution.landmarkPoints[25].transform;
  }
}
