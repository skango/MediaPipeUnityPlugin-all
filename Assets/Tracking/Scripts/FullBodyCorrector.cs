using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullBodyCorrector : MonoBehaviour
{
  [Header("Raw Bones")]
  [SerializeField] private Transform _R_ShoulderRaw;
  [SerializeField] private Transform _R_ElbowRaw;
  [SerializeField] private Transform _R_HandRaw;
  [SerializeField] private Transform _L_ShoulderRaw;
  [SerializeField] private Transform _L_ElbowRaw;
  [SerializeField] private Transform _L_HandRaw;

  [Space(10)]

  [Header("Corrected Bones")]
  [SerializeField] private Transform _R_ShoulderCorrected;
  [SerializeField] private Transform _R_ElbowCorrected;
  [SerializeField] private Transform _R_HandCorrected;
  [SerializeField] private Transform _L_ShoulderCorrected;
  [SerializeField] private Transform _L_ElbowCorrected;
  [SerializeField] private Transform _L_HandCorrected;

  [Header("Config")]
  [SerializeField] private float _shoulderLengthModifier;
  [SerializeField] private float _elbowLengthModifier;
  [SerializeField] private float _handLengthModifier;

  private void Update()
  {
    _R_ShoulderCorrected.position = _R_ShoulderRaw.position + (_L_ShoulderRaw.position - _R_ShoulderRaw.position).normalized * _shoulderLengthModifier;
    _L_ShoulderCorrected.position = _L_ShoulderRaw.position + (_R_ShoulderRaw.position - _L_ShoulderRaw.position).normalized * _shoulderLengthModifier;

    _R_ElbowCorrected.position = _R_ElbowRaw.position + (_R_ShoulderRaw.position - _R_ElbowRaw.position).normalized * _elbowLengthModifier;
    _L_ElbowCorrected.position = _L_ElbowRaw.position + (_L_ShoulderRaw.position - _L_ElbowRaw.position).normalized * _elbowLengthModifier;

    _R_HandCorrected.position = _R_HandRaw.position + (_R_ElbowRaw.position - _R_HandRaw.position).normalized * _handLengthModifier;
    _L_HandCorrected.position = _L_HandRaw.position + (_L_ElbowRaw.position - _L_HandRaw.position).normalized * _handLengthModifier;
  }
}
