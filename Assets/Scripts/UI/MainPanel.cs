using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainPanel : MonoBehaviour
{
    [SerializeField] Button StartButton;
    [SerializeField] Button DeviceConnectButton;
    [SerializeField] Button LeftHandButton;
    [SerializeField] Button RightHandButton;

    [Header("Panel")]
    [SerializeField] GameObject DeviceConnectPanel;
    // Start is called before the first frame update
    void Start()
    {
        StartButton.onClick.AddListener(StartGame);
        DeviceConnectButton.onClick.AddListener(()=> { DeviceConnectPanel.SetActive(true); });
        LeftHandButton.onClick.AddListener(()=>SetHand(0));
        RightHandButton.onClick.AddListener(()=>SetHand(1));
        SetHand(0);
    }

    private void SetHand(int v)
    {
        GameManage.mInstance.SetHand(v);
    }

    public void StartGame()
    {
        if (GForceHub.instance.connectedDevice!=null)
        {
            gameObject.SetActive(false);
        }
    }
}
