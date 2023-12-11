using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gf;
using System.Linq;

public class GameManage : MonoBehaviour
{
    public static GameManage mInstance = null;

    [SerializeField]Transform DeviceDisplayBox;

    public GameObject deviceItem;
    public List<Device> deviceList = new List<Device>();
    List<UIDeviceItem> deviceItemList = new List<UIDeviceItem>();

    uint emgSampleRateNew = 50;
    uint emgChannelMapNew = 0xFF;
    uint emgResolutionNew = 8;
    uint emgPacketLenNew = 128;

    bool setemg = false;
    bool setdataswithc = false;
    bool onenable = false;

    [Header("Hand")]
    [SerializeField] Transform leftHand;
    [SerializeField] Transform rightHand;
    Hand hand;
    [SerializeField] Transform gripPoint;
    Transform gripTansform;

    //left  0 right  1
    int handLeftRight = 0;

    int curGesture = 0;

    #region unity

    private void Awake()
    {
        mInstance = this;
    }

    private void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var unityActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");

        string[] strs = new string[] {
                "android.permission.BLUETOOTH",
                "android.permission.BLUETOOTH_ADMIN",
                //"android.permission.ACCESS_COARSE_LOCATION",
                "android.permission.ACCESS_FINE_LOCATION"
            };

        strs.ToList().ForEach(s =>
        {
            UnityEngine.Android.Permission.RequestUserPermission(s);
            Debug.Log("add RequestUserPermission: " + s);
        });
#endif
    }

    // Update is called once per frame
    void Update()
    {
        //updataDeivceItem
        if (GForceHub.instance.connectedDevice == null)
        {
            for (int i = 0; i < deviceItemList.Count; i++)
            {
                if (i <= deviceList.Count - 1)
                {
                    deviceItemList[i].init(deviceList[i]);
                }
                else if (deviceList.Count != 0)
                {
                    Destroy(deviceItemList[i].gameObject);
                    deviceItemList.Remove(deviceItemList[i]);
                }
            }
            if (deviceItemList.Count < deviceList.Count)
            {
                for (int i = 0; i < deviceList.Count - deviceItemList.Count; i++)
                {
                    showDeviceItem(deviceList[i]);
                }
            }
        }

        //SetEmgConfig
        if (GForceHub.instance.connectedDevice!=null&&
            GForceHub.instance.connectedDevice.getConnectionStatus()== Device.ConnectionStatus.Connected
            && !setemg)
        {
            StartCoroutine(setGfroceDevice());
            setemg = true;
        }

        //grip
        if (curGesture == 1 && gripTansform == null && hand.touchGameobj != null) 
        {
            hand.touchGameobj.SetParent(gripPoint);
            gripTansform = hand.touchGameobj;
        }
        if (curGesture == 2 && gripTansform != null)
        {
            gripTansform.SetParent(null);
            gripTansform = null;
        }
    }
#endregion

#region public

    public void showDeviceItem(Device device)
    {
        var go = Instantiate(deviceItem, DeviceDisplayBox);
        go.GetComponent<UIDeviceItem>().init(device);
        deviceItemList.Add(go.GetComponent<UIDeviceItem>());
    }

    public void StopScan()
    {
        GForceHub.instance.StopScan();
    }

    public void Connect(Device device)
    {
        GForceHub.instance.connectedDevice = device;

        StopScan();

        var ret = device.connect();
        

        Debug.LogFormat("[Device Event] Connect Device:  \'{0}\', ret = {1}", device.getName(), ret);
        if (ret != RetCode.GF_SUCCESS)
        {
            GForceHub.instance.connectedDevice = null;
            GForceHub.instance.StartScan();
        }
    }

    public void Disconnect()
    {
        RetCode ret;
        Debug.Log("Disconnect!");
        if (GForceHub.instance.connectedDevice!=null
            && GForceHub.instance.connectedDevice.getConnectionStatus() == Device.ConnectionStatus.Connected)
        {
            ret  = GForceHub.instance.connectedDevice.disconnect();
            Debug.LogFormat("Disconnect! ret:{0} ",ret);
            
        }
        else
        {
            Debug.LogFormat("Disconnect! GForceHub.instance.connectedDevice is {0} GForceHub.instance.connectedDevice.getConnectionStatus() : {1} "
                , GForceHub.instance.connectedDevice.getName()
                , GForceHub.instance.connectedDevice.getConnectionStatus());
        }
    }

    /// <summary>
    /// 0   left
    /// 1   right
    /// </summary>
    /// <param name="_hand"></param>
    public void SetHand(int _hand)
    {
        if (_hand ==0)
        {
            rightHand.gameObject.SetActive(false);
            leftHand.gameObject.SetActive(true);
            hand = leftHand.GetComponent<Hand>();
            handLeftRight = 0;
        }
        else if (_hand ==1)
        {
            rightHand.gameObject.SetActive(true);
            leftHand.gameObject.SetActive(false);
            hand = rightHand.GetComponent<Hand>();
            handLeftRight = 1;
        }
        gripPoint = hand.transform.Find("GripPoint").transform;
    }

    public void RotatingHand(Quaternion _q)
    {
        if (hand!=null)
        {
            if (handLeftRight == 0)
            {
                _q = Quaternion.AngleAxis(180, Vector3.forward) * _q;
            }
            hand.SetQuaternion(_q);
        }
    }

    /// <summary>
    /// 0   idle
    /// 1   grip
    /// 2   put
    /// </summary>
    /// <param name="nextGesture"></param>
    public void GetGesture(int nextGesture)
    {
        curGesture = nextGesture;
    }

#endregion


    IEnumerator setGfroceDevice()
    {
        RetCode ret;
        ret = GForceHub.instance.connectedDevice.setDataSwitch((uint)(gf.DataNotifFlags.DNF_DEVICE_STATUS | gf.DataNotifFlags.DNF_EMG_GESTURE | gf.DataNotifFlags.DNF_QUATERNION));

        Debug.Log("setDataSwitch" + ret);
        if (ret == RetCode.GF_SUCCESS)
        {
            yield return null;

            if (ret == RetCode.GF_SUCCESS)
            {
                yield return new WaitForSeconds(2f);
                
                ret = GForceHub.instance.connectedDevice.enableDataNotification(1);
                Debug.Log("enableDataNotification" + ret);
            }

            yield return null;
        }
    }

}
