using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using gf;

public class UIDeviceItem : MonoBehaviour
{
    public Text NameText;
    public Button ConnectButton;
    public Device device;

    // Start is called before the first frame update
    void Start()
    {
        ConnectButton.onClick.AddListener(ConnectDevice);
    }

    private void Update()
    {
    }

    public void init(Device _device)
    {
        device = _device;
        NameText.text = _device.getName();
        
    }

    public void ConnectDevice()
    {
        Debug.Log("连接设备");
        GameManage.mInstance.Connect(device);

    }
}
