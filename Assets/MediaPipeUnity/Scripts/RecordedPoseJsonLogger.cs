using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RecordedPoseJsonLogger : MonoBehaviour
{
  [SerializeField]
  private List<GameObject> gameObjectsToLog;
  [SerializeField] private Transform _pelvisBone;

  private bool isLogging = false;
  private float startTime;
  private List<FrameData> frameDataList = new List<FrameData>();

  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.S))
    {
      StartLogging();
    }
    else if (Input.GetKeyDown(KeyCode.E))
    {
      StopAndSaveLogging();
    }

    if (isLogging)
    {
      LogFrame();
    }
  }

  private void StartLogging()
  {
    isLogging = true;
    startTime = Time.time;
    frameDataList.Clear();
    Debug.Log("Logging started.");
  }

  private void StopAndSaveLogging()
  {
    isLogging = false;
    string json = JsonUtility.ToJson(new FrameDataListWrapper { FrameDataList = frameDataList }, true);
    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    string filePath = Path.Combine(desktopPath, "GameObjectsLog.json");
    File.WriteAllText(filePath, json);
    Debug.Log($"Logging stopped. Data saved to {filePath}");
  }

  private void LogFrame()
  {
    float elapsedTime = Time.time - startTime;
    var frameData = new FrameData
    {
      TimeElapsed = elapsedTime,
      PelvisDirection = _pelvisBone.transform.forward.normalized,
      GameObjects = new List<GameObjectData>()
    };

    foreach (var go in gameObjectsToLog)
    {
      frameData.GameObjects.Add(new GameObjectData { Name = go.name, Position = go.transform.position });
    }

    frameDataList.Add(frameData);
  }

  [Serializable]
  public class FrameData
  {
    public float TimeElapsed;
    public Vector3 PelvisDirection;
    public List<GameObjectData> GameObjects;
  }

  [Serializable]
  public class GameObjectData
  {
    public string Name;
    public Vector3 Position;
  }

  [Serializable]
  public class FrameDataListWrapper
  {
    public List<FrameData> FrameDataList;
  }
}
