using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion;
using RootMotion.FinalIK;
using Mediapipe.Unity.Sample.Holistic;
using System.Linq;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

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
  public SkinnedMeshRenderer DemoModel;
  string faceBlendshapeJson;
  public RenderTexture sourceRenderTexture;
  IEnumerator Start()
  {
    scaleRatio = -1;
    yield return new WaitUntil(() => solution.landmarkPoints.Count == 33);
    yield return new WaitForSeconds(0.2f);
   // HandVizualizer.transform.parent = ik.transform.parent;
    //HandVizualizer.transform.localPosition = Vector3.zero;
    ConfigureIk();
    while (true)
    {
      StartCoroutine(HandleScreens());
      yield return new WaitForSeconds(0.05f);
    }
  }


  IEnumerator HandleScreens()
  {
    yield return new WaitForEndOfFrame();


    // Set the source RenderTexture as active so we can read from it
    RenderTexture.active = sourceRenderTexture;

    // Create a new Texture2D with the width and height of the RenderTexture
    Texture2D screenTexture = new Texture2D(sourceRenderTexture.width, sourceRenderTexture.height, TextureFormat.RGB24, false);

    // Read pixels from the RenderTexture into the Texture2D
    screenTexture.ReadPixels(new UnityEngine.Rect(0, 0, sourceRenderTexture.width, sourceRenderTexture.height), 0, 0);
    screenTexture.Apply();

    // Compress the texture
    byte[] bytes = screenTexture.EncodeToJPG();

    // Reset the active RenderTexture to null after reading
    RenderTexture.active = null;

    yield return SendImageToAPI(bytes);
    //yield return SendImageToAPIMovement(bytes);

    // Clean up
    Destroy(screenTexture);
    //Destroy(resizedTexture);
  }



  private Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
  {
    source.filterMode = FilterMode.Bilinear;
    RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
    rt.filterMode = FilterMode.Bilinear;
    RenderTexture.active = rt;
    Graphics.Blit(source, rt);
    Texture2D newTexture = new Texture2D(newWidth, newHeight);
    newTexture.ReadPixels(new UnityEngine.Rect(0, 0, newWidth, newHeight), 0, 0);
    newTexture.Apply();
    RenderTexture.active = null;
    RenderTexture.ReleaseTemporary(rt);
    return newTexture;
  }

  private IEnumerator SendImageToAPI(byte[] imageBytes)
  {
    string url = "https://satesto.top/api/geoPlus/face-ai";
    WWWForm form = new WWWForm();
    form.AddBinaryData("image", imageBytes);
    UnityWebRequest www = UnityWebRequest.Post(url, form);
    www.SetRequestHeader("Authorization", "bearer " +
      "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjYzZjIzM2E4OWY1Nzg0MjkzMjMyYzllNiIsImlhdCI6MTcwNTQ5MzA4NywiZXhwIjoxNzA4MDg1MDg3fQ.Bzl46b32M1nvyc5tHQ9RLk8TpkBFNFC5qRKPAKGTdgc");
    //www.SetRequestHeader("Content-Type", "application/octet-stream");
    // Add any additional headers or API keys if required
    yield return www.SendWebRequest();

    if (www.result != UnityWebRequest.Result.Success)
    {
      Debug.Log("Error: " + www.error);
    }
    else
    {
      faceBlendshapeJson = www.downloadHandler.text;

      Debug.Log("AI FACE Image sent successfully " + faceBlendshapeJson);
    }
  }

  private void Update()
  {
    
      if (!string.IsNullOrEmpty(faceBlendshapeJson))
      {
        ApplyFaceBlendhsapes(JObject.Parse(faceBlendshapeJson), DemoModel);
      }
    

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


  public float GetBlendshapeValue(string Blendshape)
  {
    return DemoModel.GetBlendShapeWeight(
     DemoModel.
     sharedMesh.GetBlendShapeIndex(Blendshape));
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

  public void ApplyFaceBlendhsapes(JObject data, SkinnedMeshRenderer skinnedMeshRenderer)
  {
    float SmileLeft =
        data["blendshapes"]["mouthSmileLeft"].Value<float>();
    float SmileRight =
        data["blendshapes"]["mouthSmileRight"].Value<float>();
    float blinkLeft =
        data["blendshapes"]["eyeBlinkLeft"].Value<float>();
    float blinkRight =
        data["blendshapes"]["eyeBlinkRight"].Value<float>();
    float mouthFrownLeft =
        data["blendshapes"]["mouthFrownLeft"].Value<float>();
    float mouthFrownRight =
        data["blendshapes"]["mouthFrownRight"].Value<float>();
    float CheeckPuff =
        data["blendshapes"]["cheekPuff"].Value<float>();
    float BrowUp =
        data["blendshapes"]["browInnerUp"].Value<float>();
    float BrowOuterUpLeft =
        data["blendshapes"]["browOuterUpLeft"].Value<float>();
    float BrowOuterUpRight =
        data["blendshapes"]["browOuterUpRight"].Value<float>();
    float BrowDownLeft =
        data["blendshapes"]["browDownLeft"].Value<float>();
    float BrowDownRight =
        data["blendshapes"]["browDownRight"].Value<float>();
    float MouthClose =
        data["blendshapes"]["mouthClose"].Value<float>();

    float JawForward =
        data["blendshapes"]["jawForward"].Value<float>();
    float JawOpen =
        data["blendshapes"]["jawOpen"].Value<float>();
    float MouthFunnel =
        data["blendshapes"]["mouthFunnel"].Value<float>();
    float mouthLeft =
        data["blendshapes"]["mouthLeft"].Value<float>();
    float mouthRight =
        data["blendshapes"]["mouthRight"].Value<float>();
    float mouthRollLower =
        data["blendshapes"]["mouthRollLower"].Value<float>();
    float mouthRollUpper =
        data["blendshapes"]["mouthRollUpper"].Value<float>();

    float speed = 5f;


    float MouthCloseRecalculated = (100 - (JawOpen * 100)) - 50;

    /*skinnedMeshRenderer.SetBlendShapeWeight(
   skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Mouth_Close"),
   Mathf.Lerp(GetBlendshapeValue("Mouth_Close"), MouthCloseRecalculated < 36 ? 0 :
     MouthCloseRecalculated, speed * Time.deltaTime));*/

    skinnedMeshRenderer.SetBlendShapeWeight(
   skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Mouth_Roll_In_Upper_L"),
   Mathf.Lerp(GetBlendshapeValue("Mouth_Roll_In_Upper_L"), mouthRollUpper * 200, speed * Time.deltaTime));

    skinnedMeshRenderer.SetBlendShapeWeight(
        skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Mouth_Roll_In_Upper_R"),
        Mathf.Lerp(GetBlendshapeValue("Mouth_Roll_In_Upper_R"), mouthRollUpper * 200, speed * Time.deltaTime));

    /* skinnedMeshRenderer.SetBlendShapeWeight(
         skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Mouth_Roll_In_Down_L"),
         Mathf.Lerp(GetBlendshapeValue("Mouth_Roll_In_Down_L"), mouthRollLower * 100, speed * Time.deltaTime));

     skinnedMeshRenderer.SetBlendShapeWeight(
         skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Mouth_Roll_In_Down_R"),
         Mathf.Lerp(GetBlendshapeValue("Mouth_Roll_In_Down_R"), mouthRollLower * 100, speed * Time.deltaTime));
     */
    skinnedMeshRenderer.SetBlendShapeWeight(
        skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Mouth_L"),
        Mathf.Lerp(GetBlendshapeValue("Mouth_L"), mouthLeft * 200, speed * Time.deltaTime));

    skinnedMeshRenderer.SetBlendShapeWeight(
        skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Mouth_R"),
        Mathf.Lerp(GetBlendshapeValue("Mouth_R"), mouthRight * 200, speed * Time.deltaTime));

    skinnedMeshRenderer.SetBlendShapeWeight(
        skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Mouth_Funnel_Up_L"),
        Mathf.Lerp(GetBlendshapeValue("Mouth_Funnel_Up_L"), MouthFunnel * 200, speed * Time.deltaTime));

    skinnedMeshRenderer.SetBlendShapeWeight(
        skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Mouth_Funnel_Up_R"),
        Mathf.Lerp(GetBlendshapeValue("Mouth_Funnel_Up_R"), MouthFunnel * 200, speed * Time.deltaTime));

    /*skinnedMeshRenderer.SetBlendShapeWeight(
        skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Jaw_Open"),
        Mathf.Lerp(GetBlendshapeValue("Jaw_Open"), JawOpen * 200, speed * Time.deltaTime));*/

    skinnedMeshRenderer.SetBlendShapeWeight(
        skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Jaw_Forward"),
        Mathf.Lerp(GetBlendshapeValue("Jaw_Forward"), JawForward * 200, speed * Time.deltaTime));

    skinnedMeshRenderer.SetBlendShapeWeight(
        skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Mouth_Smile_L"),
        Mathf.Lerp(GetBlendshapeValue("Mouth_Smile_L"), SmileLeft * 200, speed * Time.deltaTime));

    skinnedMeshRenderer.SetBlendShapeWeight(
        skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Mouth_Smile_R"),
        Mathf.Lerp(GetBlendshapeValue("Mouth_Smile_R"), SmileRight * 200, speed * Time.deltaTime));

    skinnedMeshRenderer.SetBlendShapeWeight(
        skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Eye_Blink_L"),
        Mathf.Lerp(GetBlendshapeValue("Eye_Blink_L"), blinkLeft * 200, speed * Time.deltaTime));

    skinnedMeshRenderer.SetBlendShapeWeight(
        skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Eye_Blink_R"),
        Mathf.Lerp(GetBlendshapeValue("Eye_Blink_R"), blinkRight * 200, speed * Time.deltaTime));

    skinnedMeshRenderer.SetBlendShapeWeight(
        skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Mouth_Frown_L"),
        Mathf.Lerp(GetBlendshapeValue("Mouth_Frown_L"), mouthFrownLeft * 200, speed * Time.deltaTime));

    skinnedMeshRenderer.SetBlendShapeWeight(
        skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Mouth_Frown_R"),
        Mathf.Lerp(GetBlendshapeValue("Mouth_Frown_R"), mouthFrownRight * 200, speed * Time.deltaTime));

    skinnedMeshRenderer.SetBlendShapeWeight(
        skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Cheek_Puff_L"),
        Mathf.Lerp(GetBlendshapeValue("Cheek_Puff_L"), CheeckPuff * 200, speed * Time.deltaTime));

    skinnedMeshRenderer.SetBlendShapeWeight(
        skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Cheek_Puff_R"),
        Mathf.Lerp(GetBlendshapeValue("Cheek_Puff_R"), CheeckPuff * 200, speed * Time.deltaTime));

    skinnedMeshRenderer.SetBlendShapeWeight(
        skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Brow_Raise_Inner_L"),
        Mathf.Lerp(GetBlendshapeValue("Brow_Raise_Inner_L"), BrowUp * 100, speed * Time.deltaTime));

    skinnedMeshRenderer.SetBlendShapeWeight(
        skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Brow_Raise_Inner_R"),
        Mathf.Lerp(GetBlendshapeValue("Brow_Raise_Inner_R"), BrowUp * 100, speed * Time.deltaTime));

    skinnedMeshRenderer.SetBlendShapeWeight(
        skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Brow_Raise_Outer_R"),
        Mathf.Lerp(GetBlendshapeValue("Brow_Raise_Outer_R"), BrowOuterUpRight * 100, speed * Time.deltaTime));

    skinnedMeshRenderer.SetBlendShapeWeight(
        skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Brow_Raise_Outer_L"),
        Mathf.Lerp(GetBlendshapeValue("Brow_Raise_Outer_L"), BrowOuterUpLeft * 100, speed * Time.deltaTime));

    skinnedMeshRenderer.SetBlendShapeWeight(
        skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Brow_Drop_L"),
        Mathf.Lerp(GetBlendshapeValue("Brow_Drop_L"), BrowDownLeft * 100, speed * Time.deltaTime));

    skinnedMeshRenderer.SetBlendShapeWeight(
        skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Brow_Drop_R"),
        Mathf.Lerp(GetBlendshapeValue("Brow_Drop_R"), BrowDownRight * 100, speed * Time.deltaTime));


    skinnedMeshRenderer.SetBlendShapeWeight(
        skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("V_Lip_Open"),
        Mathf.Lerp(GetBlendshapeValue("V_Lip_Open"), 100 - BrowDownRight * 200, speed * Time.deltaTime));
  }
}
