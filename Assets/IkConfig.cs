using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion;
using RootMotion.FinalIK;
using Mediapipe.Unity.Sample.Holistic;
using System.Linq;
using static Mediapipe.CopyCalculatorOptions.Types;


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
  public Transform IkParent; // The virtual parent transform

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
        // Combine the scaled delta position with the virtual parent's transform
        Vector3 worldPosition = IkParent.TransformPoint(deltaPosition);
        RhandIkTargets[i].transform.position = worldPosition;

        // Update the previous position for the next frame
        previousPositions[i] = currentPosition;

      }


    }
  }
  public void ConfigureIk()
  {
    GameObject head = new GameObject("HEAD Simulator");
    head.transform.parent = solution.landmarkPoints[0].transform;
    head.transform.localPosition = new Vector3(0, 0, 10f);
    head.transform.localRotation = Quaternion.Euler(0, -1.6f, 0);
    ik.solver.spine.headTarget = head.transform;
    ik.solver.leftArm.target = solution.landmarkPoints[16].transform;
    ik.solver.rightArm.target = solution.landmarkPoints[15].transform;
    ik.solver.leftLeg.target = solution.landmarkPoints[32].transform;
    ik.solver.rightLeg.target = solution.landmarkPoints[31].transform;
    ik.solver.leftArm.bendGoal = solution.landmarkPoints[14].transform;
    ik.solver.rightArm.bendGoal = solution.landmarkPoints[13].transform;
    ik.solver.leftLeg.bendGoal = solution.landmarkPoints[26].transform;
    ik.solver.rightLeg.bendGoal = solution.landmarkPoints[25].transform;
    GameObject pelvis = new GameObject("Pelvis Simulator");
    pelvis.transform.parent = solution.landmarkPoints[25].transform;
    pelvis.transform.localPosition = new Vector3(5f, 25f, 4);
    ik.solver.spine.pelvisTarget = pelvis.transform;
    GameObject chest = new GameObject("Chest Simulator");
    chest.transform.parent = solution.landmarkPoints[12].transform;
    chest.transform.localPosition = new Vector3(5f, -5f, 0);
    ik.solver.spine.chestGoal = chest.transform;
  }
}
