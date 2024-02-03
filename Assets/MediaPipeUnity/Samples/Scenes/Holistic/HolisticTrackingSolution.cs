// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using Google.Protobuf.WellKnownTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Mediapipe.Unity.Sample.Holistic
{
  public class HolisticTrackingSolution : ImageSourceSolution<HolisticTrackingGraph>
  {
    [SerializeField] private RectTransform _worldAnnotationArea;
    [SerializeField] private DetectionAnnotationController _poseDetectionAnnotationController;
    [SerializeField] private HolisticLandmarkListAnnotationController _holisticAnnotationController;
    [SerializeField] private PoseWorldLandmarkListAnnotationController _poseWorldLandmarksAnnotationController;
    [SerializeField] private MaskAnnotationController _segmentationMaskAnnotationController;
    [SerializeField] private NormalizedRectAnnotationController _poseRoiAnnotationController;
    LandmarkList landmarkList = new LandmarkList();
    public List<GameObject> landmarkPoints = new List<GameObject>();
    public GameObject Humanoid,PointListAnotation;
    public List<GameObject> targets = new List<GameObject>();
    bool firsttime = true;
    public HolisticTrackingGraph.ModelComplexity modelComplexity
    {
      get => graphRunner.modelComplexity;
      set => graphRunner.modelComplexity = value;
    }

    public bool smoothLandmarks
    {
      get => graphRunner.smoothLandmarks;
      set => graphRunner.smoothLandmarks = value;
    }

    public bool refineFaceLandmarks
    {
      get => graphRunner.refineFaceLandmarks;
      set => graphRunner.refineFaceLandmarks = value;
    }

    public bool enableSegmentation
    {
      get => graphRunner.enableSegmentation;
      set => graphRunner.enableSegmentation = value;
    }

    public bool smoothSegmentation
    {
      get => graphRunner.smoothSegmentation;
      set => graphRunner.smoothSegmentation = value;
    }

    public float minDetectionConfidence
    {
      get => graphRunner.minDetectionConfidence;
      set => graphRunner.minDetectionConfidence = value;
    }

    public float minTrackingConfidence
    {
      get => graphRunner.minTrackingConfidence;
      set => graphRunner.minTrackingConfidence = value;
    }

    protected override void SetupScreen(ImageSource imageSource)
    {
      base.SetupScreen(imageSource);
      _worldAnnotationArea.localEulerAngles = imageSource.rotation.Reverse().GetEulerAngles();
    }

    protected override void OnStartRun()
    {
      if (!runningMode.IsSynchronous())
      {
        graphRunner.OnPoseDetectionOutput += OnPoseDetectionOutput;
        graphRunner.OnFaceLandmarksOutput += OnFaceLandmarksOutput;
        graphRunner.OnPoseLandmarksOutput += OnPoseLandmarksOutput;
        graphRunner.OnLeftHandLandmarksOutput += OnLeftHandLandmarksOutput;
        graphRunner.OnRightHandLandmarksOutput += OnRightHandLandmarksOutput;
        graphRunner.OnPoseWorldLandmarksOutput += OnPoseWorldLandmarksOutput;
        graphRunner.OnSegmentationMaskOutput += OnSegmentationMaskOutput;
        graphRunner.OnPoseRoiOutput += OnPoseRoiOutput;
      }

      var imageSource = ImageSourceProvider.ImageSource;
      SetupAnnotationController(_poseDetectionAnnotationController, imageSource);
      SetupAnnotationController(_holisticAnnotationController, imageSource);
      SetupAnnotationController(_poseWorldLandmarksAnnotationController, imageSource);
      SetupAnnotationController(_segmentationMaskAnnotationController, imageSource);
      _segmentationMaskAnnotationController.InitScreen(imageSource.textureWidth, imageSource.textureHeight);
      SetupAnnotationController(_poseRoiAnnotationController, imageSource);
    }

    protected override void AddTextureFrameToInputStream(TextureFrame textureFrame)
    {
      graphRunner.AddTextureFrameToInputStream(textureFrame);
    }

    protected override IEnumerator WaitForNextValue()
    {
      var task = graphRunner.WaitNextAsync();
      yield return new WaitUntil(() => task.IsCompleted);

      var result = task.Result;
      _poseDetectionAnnotationController.DrawNow(result.poseDetection);
      _holisticAnnotationController.DrawNow(result.faceLandmarks, result.poseLandmarks, result.leftHandLandmarks, result.rightHandLandmarks);
      _poseWorldLandmarksAnnotationController.DrawNow(result.poseWorldLandmarks);
      _segmentationMaskAnnotationController.DrawNow(result.segmentationMask);
      _poseRoiAnnotationController.DrawNow(result.poseRoi);

      result.segmentationMask?.Dispose();
    }
    private void OnPoseDetectionOutput(object stream, OutputStream<Detection>.OutputEventArgs eventArgs)
    {
      var packet = eventArgs.packet;
      var value = packet == null ? default : packet.Get(Detection.Parser);
      _poseDetectionAnnotationController.DrawLater(value);
    
    }

    private void OnFaceLandmarksOutput(object stream, OutputStream<NormalizedLandmarkList>.OutputEventArgs eventArgs)
    {
      var packet = eventArgs.packet;
      var value = packet == null ? default : packet.Get(NormalizedLandmarkList.Parser);
      _holisticAnnotationController.DrawFaceLandmarkListLater(value);
    }

    private void OnPoseLandmarksOutput(object stream, OutputStream<NormalizedLandmarkList>.OutputEventArgs eventArgs)
    {
      var packet = eventArgs.packet;
      var value = packet == null ? default : packet.Get(NormalizedLandmarkList.Parser);
      _holisticAnnotationController.DrawPoseLandmarkListLater(value);
    }

    private void Update()
    {
      Humanoid.transform.localPosition = Vector3.zero;
      if (landmarkPoints.Count == 0)
      {

        for (int i = 0; i < 33; i++)
        {
          GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
          sphere.transform.localScale = Vector3.one * 0f;
          sphere.transform.parent = Humanoid.transform;
          sphere.transform.localPosition = Vector3.zero;

          landmarkPoints.Add(sphere);
        }
      }

      if (landmarkList != null)
      {
        if (landmarkList.Landmark != null)
        {
          /*for (int i = 0; i < landmarkList.Landmark.Count; i++)
          {
            landmarkPoints[i].transform.localPosition = 
              PointListAnotation.transform.GetChild(landmarkList.Landmark.Count - i).transform.localPosition;
          }*/
          /*landmarkPoints[0] =
              PointListAnotation.transform.GetChild(0).gameObject;
          landmarkPoints[15] =
              PointListAnotation.transform.GetChild(15).gameObject;
          landmarkPoints[16] =
              PointListAnotation.transform.GetChild(16).gameObject;
          landmarkPoints[28] =
              PointListAnotation.transform.GetChild(28).gameObject;
          landmarkPoints[27] =
              PointListAnotation.transform.GetChild(27).gameObject;
          landmarkPoints[14] =
            PointListAnotation.transform.GetChild(14).gameObject;*/
          for (int i = 0; i < landmarkPoints.Count; i++)
          {
            if (firsttime)
            {
              GameObject newpoint = Instantiate(new GameObject(), PointListAnotation.transform);
              landmarkPoints[i] = newpoint;
            }
            landmarkPoints[i].transform.position = Vector3.Lerp(landmarkPoints[i].transform.position,
              PointListAnotation.transform.GetChild(i).transform.position,5 * Time.deltaTime);
          }
          firsttime = false;
        }
      }
    }

    private void OnLeftHandLandmarksOutput(object stream, OutputStream<NormalizedLandmarkList>.OutputEventArgs eventArgs)
    {
      var packet = eventArgs.packet;
      var value = packet == null ? default : packet.Get(NormalizedLandmarkList.Parser);
      _holisticAnnotationController.DrawLeftHandLandmarkListLater(value);
     // ApplyPos(value);
    }

    public void ApplyPos(NormalizedLandmarkList value)
    {
      if (targets.Count > 0)
      {
        targets[0].transform.localPosition = new Vector3(value.Landmark[4].X,
          value.Landmark[4].Y, value.Landmark[8].Z);
        targets[1].transform.localPosition = new Vector3(value.Landmark[4].X,
          value.Landmark[4].Y, value.Landmark[12].Z);
        targets[2].transform.localPosition = new Vector3(value.Landmark[4].X,
          value.Landmark[4].Y, value.Landmark[16].Z);
        targets[3].transform.localPosition = new Vector3(value.Landmark[4].X,
          value.Landmark[4].Y, value.Landmark[20].Z);
        targets[4].transform.localPosition = new Vector3(value.Landmark[4].X,
        value.Landmark[4].Y, value.Landmark[24].Z);
      }
    }

    private void OnRightHandLandmarksOutput(object stream, OutputStream<NormalizedLandmarkList>.OutputEventArgs eventArgs)
    {
      var packet = eventArgs.packet;
      var value = packet == null ? default : packet.Get(NormalizedLandmarkList.Parser);
      _holisticAnnotationController.DrawRightHandLandmarkListLater(value);
    }

    private void OnPoseWorldLandmarksOutput(object stream, OutputStream<LandmarkList>.OutputEventArgs eventArgs)
    {
      var packet = eventArgs.packet;
      var value = packet == null ? default : packet.Get(LandmarkList.Parser);
      _poseWorldLandmarksAnnotationController.DrawLater(value);
      landmarkList = value;
    }

    private void OnSegmentationMaskOutput(object stream, OutputStream<ImageFrame>.OutputEventArgs eventArgs)
    {
      var packet = eventArgs.packet;
      var value = packet == null ? default : packet.Get();
      _segmentationMaskAnnotationController.DrawLater(value);
      value?.Dispose();
    }

    private void OnPoseRoiOutput(object stream, OutputStream<NormalizedRect>.OutputEventArgs eventArgs)
    {
      var packet = eventArgs.packet;
      var value = packet == null ? default : packet.Get(NormalizedRect.Parser);
      _poseRoiAnnotationController.DrawLater(value);
    }
  }
}
