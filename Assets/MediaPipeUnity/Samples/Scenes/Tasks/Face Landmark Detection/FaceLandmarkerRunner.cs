// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using UnityEngine;

using Mediapipe.Tasks.Vision.FaceLandmarker;
using UnityEngine.Rendering;
using Newtonsoft.Json.Linq;

namespace Mediapipe.Unity.Sample.FaceLandmarkDetection
{
  public class FaceLandmarkerRunner : VisionTaskApiRunner<FaceLandmarker>
  {
    [SerializeField] private FaceLandmarkerResultAnnotationController _faceLandmarkerResultAnnotationController;

    private Experimental.TextureFramePool _textureFramePool;

    public readonly FaceLandmarkDetectionConfig config = new FaceLandmarkDetectionConfig();
    public SkinnedMeshRenderer DemoModel;
    string faceBlendshapeJson;

    public override void Stop()
    {
      base.Stop();
      _textureFramePool?.Dispose();
      _textureFramePool = null;
    }

    protected override IEnumerator Run()
    {
      Debug.Log($"Delegate = {config.Delegate}");
      Debug.Log($"Running Mode = {config.RunningMode}");
      Debug.Log($"NumFaces = {config.NumFaces}");
      Debug.Log($"MinFaceDetectionConfidence = {config.MinFaceDetectionConfidence}");
      Debug.Log($"MinFacePresenceConfidence = {config.MinFacePresenceConfidence}");
      Debug.Log($"MinTrackingConfidence = {config.MinTrackingConfidence}");
      Debug.Log($"OutputFaceBlendshapes = {config.OutputFaceBlendshapes}");
      Debug.Log($"OutputFacialTransformationMatrixes = {config.OutputFacialTransformationMatrixes}");

      yield return AssetLoader.PrepareAssetAsync(config.ModelPath);

      var options = config.GetFaceLandmarkerOptions(config.RunningMode == Tasks.Vision.Core.RunningMode.LIVE_STREAM ? OnFaceLandmarkDetectionOutput : null);
      taskApi = FaceLandmarker.CreateFromOptions(options);
      var imageSource = ImageSourceProvider.ImageSource;

      yield return imageSource.Play();

      if (!imageSource.isPrepared)
      {
        Debug.LogError("Failed to start ImageSource, exiting...");
        yield break;
      }

      // Use RGBA32 as the input format.
      // TODO: When using GpuBuffer, MediaPipe assumes that the input format is BGRA, so maybe the following code needs to be fixed.
      _textureFramePool = new Experimental.TextureFramePool(imageSource.textureWidth, imageSource.textureHeight, TextureFormat.RGBA32, 10);

      // NOTE: The screen will be resized later, keeping the aspect ratio.
      screen.Initialize(imageSource);

      SetupAnnotationController(_faceLandmarkerResultAnnotationController, imageSource);

      var transformationOptions = imageSource.GetTransformationOptions();
      var flipHorizontally = transformationOptions.flipHorizontally;
      var flipVertically = transformationOptions.flipVertically;
      var imageProcessingOptions = new Tasks.Vision.Core.ImageProcessingOptions(rotationDegrees: (int)transformationOptions.rotationAngle);

      AsyncGPUReadbackRequest req = default;
      var waitUntilReqDone = new WaitUntil(() => req.done);
      var result = FaceLandmarkerResult.Alloc(options.numFaces);

      while (true)
      {
        if (isPaused)
        {
          yield return new WaitWhile(() => isPaused);
        }

        if (!_textureFramePool.TryGetTextureFrame(out var textureFrame))
        {
          yield return new WaitForEndOfFrame();
          continue;
        }

        // Copy current image to TextureFrame
        req = textureFrame.ReadTextureAsync(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
        yield return waitUntilReqDone;

        if (req.hasError)
        {
          Debug.LogError($"Failed to read texture from the image source, exiting...");
          break;
        }

        var image = textureFrame.BuildCPUImage();
        switch (taskApi.runningMode)
        {
          case Tasks.Vision.Core.RunningMode.IMAGE:
            if (taskApi.TryDetect(image, imageProcessingOptions, ref result))
            {
              _faceLandmarkerResultAnnotationController.DrawNow(result);
            }
            else
            {
              _faceLandmarkerResultAnnotationController.DrawNow(default);
            }
            break;
          case Tasks.Vision.Core.RunningMode.VIDEO:
            if (taskApi.TryDetectForVideo(image, GetCurrentTimestampMillisec(), imageProcessingOptions, ref result))
            {
              _faceLandmarkerResultAnnotationController.DrawNow(result);
            }
            else
            {
              _faceLandmarkerResultAnnotationController.DrawNow(default);
            }
            break;
          case Tasks.Vision.Core.RunningMode.LIVE_STREAM:
            taskApi.DetectAsync(image, GetCurrentTimestampMillisec(), imageProcessingOptions);
            break;
        }

        textureFrame.Release();
      }
    }

    private void OnFaceLandmarkDetectionOutput(FaceLandmarkerResult result, Image image, long timestamp)
    {
      Debug.Log("FACE LANDMARKER OUTPUT GOT");
      if (result.faceBlendshapes != null)
      {
        Debug.Log("BLANDSHAPES: " + result.faceBlendshapes);
        ProcessBlendshaepes(result.faceBlendshapes[0].ToString());
        _faceLandmarkerResultAnnotationController.DrawLater(result);
      }
    }

    private void Update()
    {
      if (faceBlendshapeJson != null)
      {
        ApplyFaceBlendhsapes(JObject.Parse(faceBlendshapeJson), DemoModel);
      }
    }

    public void ProcessBlendshaepes(string json)
    {
      JObject data = JObject.Parse(json);
      JArray arr = data["categories"].ToObject<JArray>();
      JObject newdata = new JObject();
      JObject blendshapes = new JObject();
      for (int i = 0; i < arr.Count; i++)
      {
        blendshapes.Add(arr[i]["categoryName"].Value<string>(),
            arr[i]["score"].Value<float>());
      }
      newdata["blendshapes"] = blendshapes;
      faceBlendshapeJson = newdata.ToString();
      Debug.Log("Mediapipe Blendshapes converted!");
      
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


      float MouthCloseRecalculated = (100 - (JawOpen * 100)) - 61;

      skinnedMeshRenderer.SetBlendShapeWeight(
     skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Mouth_Close"),
     Mathf.Lerp(GetBlendshapeValue("Mouth_Close"), MouthCloseRecalculated < 36 ? 0 :
     MouthCloseRecalculated, speed * Time.deltaTime));

      skinnedMeshRenderer.SetBlendShapeWeight(
     skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Mouth_Roll_In_Upper_L"),
     Mathf.Lerp(GetBlendshapeValue("Mouth_Roll_In_Upper_L"), mouthRollUpper * 100, speed * Time.deltaTime));

      skinnedMeshRenderer.SetBlendShapeWeight(
          skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Mouth_Roll_In_Upper_R"),
          Mathf.Lerp(GetBlendshapeValue("Mouth_Roll_In_Upper_R"), mouthRollUpper * 100, speed * Time.deltaTime));

      /*skinnedMeshRenderer.SetBlendShapeWeight(
          skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Mouth_Roll_In_Down_L"),
          Mathf.Lerp(GetBlendshapeValue("Mouth_Roll_In_Down_L"), mouthRollLower * 100, speed * Time.deltaTime));

      skinnedMeshRenderer.SetBlendShapeWeight(
          skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Mouth_Roll_In_Down_R"),
          Mathf.Lerp(GetBlendshapeValue("Mouth_Roll_In_Down_R"), mouthRollLower * 100, speed * Time.deltaTime));
      */
      skinnedMeshRenderer.SetBlendShapeWeight(
          skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Mouth_L"),
          Mathf.Lerp(GetBlendshapeValue("Mouth_L"), mouthLeft * 100, speed * Time.deltaTime));

      skinnedMeshRenderer.SetBlendShapeWeight(
          skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Mouth_R"),
          Mathf.Lerp(GetBlendshapeValue("Mouth_R"), mouthRight * 100, speed * Time.deltaTime));

      skinnedMeshRenderer.SetBlendShapeWeight(
          skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Mouth_Funnel_Up_L"),
          Mathf.Lerp(GetBlendshapeValue("Mouth_Funnel_Up_L"), MouthFunnel * 100, speed * Time.deltaTime));

      skinnedMeshRenderer.SetBlendShapeWeight(
          skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Mouth_Funnel_Up_R"),
          Mathf.Lerp(GetBlendshapeValue("Mouth_Funnel_Up_R"), MouthFunnel * 100, speed * Time.deltaTime));

      skinnedMeshRenderer.SetBlendShapeWeight(
          skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Jaw_Open"),
          Mathf.Lerp(GetBlendshapeValue("Jaw_Open"), JawOpen * 100, speed * Time.deltaTime));

      skinnedMeshRenderer.SetBlendShapeWeight(
          skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Jaw_Forward"),
          Mathf.Lerp(GetBlendshapeValue("Jaw_Forward"), JawForward * 100, speed * Time.deltaTime));

      skinnedMeshRenderer.SetBlendShapeWeight(
          skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Mouth_Smile_L"),
          Mathf.Lerp(GetBlendshapeValue("Mouth_Smile_L"), SmileLeft * 100, speed * Time.deltaTime));

      skinnedMeshRenderer.SetBlendShapeWeight(
          skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Mouth_Smile_R"),
          Mathf.Lerp(GetBlendshapeValue("Mouth_Smile_R"), SmileRight * 100, speed * Time.deltaTime));

      skinnedMeshRenderer.SetBlendShapeWeight(
          skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Eye_Blink_L"),
          Mathf.Lerp(GetBlendshapeValue("Eye_Blink_L"), blinkLeft * 100, speed * Time.deltaTime));

      skinnedMeshRenderer.SetBlendShapeWeight(
          skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Eye_Blink_R"),
          Mathf.Lerp(GetBlendshapeValue("Eye_Blink_R"), blinkRight * 100, speed * Time.deltaTime));

      skinnedMeshRenderer.SetBlendShapeWeight(
          skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Mouth_Frown_L"),
          Mathf.Lerp(GetBlendshapeValue("Mouth_Frown_L"), mouthFrownLeft * 100, speed * Time.deltaTime));

      skinnedMeshRenderer.SetBlendShapeWeight(
          skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Mouth_Frown_R"),
          Mathf.Lerp(GetBlendshapeValue("Mouth_Frown_R"), mouthFrownRight * 100, speed * Time.deltaTime));

      skinnedMeshRenderer.SetBlendShapeWeight(
          skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Cheek_Puff_L"),
          Mathf.Lerp(GetBlendshapeValue("Cheek_Puff_L"), CheeckPuff * 100, speed * Time.deltaTime));

      skinnedMeshRenderer.SetBlendShapeWeight(
          skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Cheek_Puff_R"),
          Mathf.Lerp(GetBlendshapeValue("Cheek_Puff_R"), CheeckPuff * 100, speed * Time.deltaTime));

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
          Mathf.Lerp(GetBlendshapeValue("V_Lip_Open"), 100 - BrowDownRight * 100, speed * Time.deltaTime));
    }

    public float GetBlendshapeValue(string Blendshape)
    {
      return DemoModel.GetBlendShapeWeight(
       DemoModel.
       sharedMesh.GetBlendShapeIndex(Blendshape));
    }


  }
}
