/*
 * Copyright 2017, OYMotion Inc.
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in
 *    the documentation and/or other materials provided with the
 *    distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
 * FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
 * COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS
 * OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED
 * AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
 * OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF
 * THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
 * DAMAGE.
 *
 */
    ﻿using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using gf;

#if UNITY_EDITOR
using UnityEditor;
#endif



public class GForceHub : MonoBehaviour
{
    public static GForceHub instance
    {
        get { return mInstance; }
    }

    public bool Reset()
    {
        if (mHub != null)
        {
            foreach (GForceDevice dev in mDeviceComps)
            {
                if (null == dev.device)
                    continue;
                dev.device.disconnect();
                dev.device = null;
            }

            mHub.Dispose();
            mHub = null;
        }
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var unityActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");

            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                mHub = Hub.Instance;
                prepare();
            }));
            return true;
#else
        mHub = Hub.Instance;
        prepare();
        return true;
#endif
    }

    void Awake()
    {
        // Ensure that there is only one Hub.
        if (mInstance != null)
        {
#if UNITY_EDITOR
            EditorUtility.DisplayDialog("Can only have one Hub",
                                        "Your scene contains more than one Hub. Remove all but one Hub.",
                                        "OK");
#endif
            Destroy(this.gameObject);
            return;
        }
        else
        {
            mInstance = this;
        }

        // Do not destroy this game object. This will ensure that it remains active even when
        // switching scenes.
        DontDestroyOnLoad(this);

        for (int i = 0; i < transform.childCount; ++i)
        {
            Transform child = transform.GetChild(i);

            var dev = child.gameObject.GetComponent<GForceDevice>();
            
            if (dev != null)
            {
                mDeviceComps.Add(dev);
            }
        }

        if (mDeviceComps.Count < 1)
        {
            string errorMessage = "The GForceHub GameObject must have at least one child with a DeviceComonent. Check prefab";
#if UNITY_EDITOR
            EditorUtility.DisplayDialog("No DeviceComponent child.", errorMessage, "OK");
#else
                throw new UnityException (errorMessage);
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var unityActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");

            var applicationContext = unityActivity.Call<AndroidJavaObject>("getApplicationContext");

            // Need to pass the Android Application Context to the jni plugin before initializing the Hub.
            AndroidJavaClass nativeEventsClass = new AndroidJavaClass("com.oymotion.ble.GlobalContext");
            nativeEventsClass.CallStatic("setApplicationContext", applicationContext);

            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                mHub = Hub.Instance;
                prepare();
            }));
#else
        mHub = Hub.Instance;
        prepare();
#endif
    }

    void OnApplicationQuit()
    {
        if (mHub != null)
        {
            terminal();
            mHub.Dispose();
            mHub = null;
        }
        Debug.Log("GForceHub.OnApplicationQuit");
    }

    private void Start()
    {
        //mHub.startScan();
    }

    void Update()
    {
        ////todo:自动扫描
        //if (mNeedDeviceScan)
        //{
        //    // if no available device, scan
        //    Debug.Log("Scan again until all preset devices are found.");
        //    mNeedDeviceScan = false;
        //    mHub.startScan();
        //}
        //mDeviceComps[0].TickGForce();
    }


    private static GForceHub mInstance = null;
    private Hub mHub = null;
    private List<GForceDevice> mDeviceComps = new List<GForceDevice>();
    private List<Device> mFoundDevices = new List<Device>();
    //private bool mNeedDeviceScan = false;
#if !UNITY_ANDROID
        private Hub.logFn logfun = new Hub.logFn(GForceHub.DebugLog);
#endif

    private class Listener : HubListener
    {
        class DeviceComparer : IComparer<Device>
        {
            public int Compare(Device x, Device y)
            {
                if (x == null)
                {
                    if (y == null)
                        return 0;
                    else
                        return 1;
                }
                else
                {
                    if (y == null)
                        return -1;
                    uint xrssi = x.getRssi();
                    uint yrssi = y.getRssi();
                    if (xrssi > yrssi)
                        return -1;
                    else if (xrssi < yrssi)
                        return 1;
                    else
                        return 0;
                }
            }
        }

        public override void onScanFinished()
        {
            Debug.Log("OnScanFinished");
            
            if (hubcomp.connectedDevice==null)
            {
                hubcomp.mFoundDevices.Clear();
                hubcomp.mHub.startScan();
            }
        }

        public override void onStateChanged(Hub.HubState state)
        {
            Debug.LogFormat("onStateChanged: {0}", state);
        }

        public override void onDeviceFound(Device device)
        {
            Debug.LogFormat("onDeviceFound, name = \'{0}\', rssi = {1}",
                device.getName(), device.getRssi());
            if (hubcomp.mFoundDevices.Count==0)
            {
                hubcomp.mFoundDevices.Add(device);
            }
            else
            {
                for (int i = 0; i < hubcomp.mFoundDevices.Count; i++)
                {
                    if (hubcomp.mFoundDevices[i].getAddress() == device.getAddress())
                    {
                        hubcomp.mFoundDevices[i] = device;
                        break;
                    }
                    else if (i == hubcomp.mFoundDevices.Count - 1 && device.getAddress() != "") 
                    {
                        hubcomp.mFoundDevices.Add(device);
                    }
                }
            }

            Debug.Log("hubcomp.mFoundDevices: " + hubcomp.mFoundDevices.Count);
            GameManage.mInstance.deviceList = hubcomp.mFoundDevices;

        }

        public override void onDeviceDiscard(Device device)
        {
            Debug.LogFormat("onDeviceDiscard, handle = name is \'{0}\'", device.getName());
            Debug.LogFormat("onDeviceDiscard, handle = name is \'{0}\'", device.getAddress());
            foreach (GForceDevice dev in hubcomp.mDeviceComps)
            {
                if (null == dev.device)
                    continue;
                if (device == dev.device)
                {
                    dev.device = null;
                    break;
                }
            }
            bool ret = hubcomp.mFoundDevices.Remove(device);
            Debug.LogFormat("hubcomp.mFoundDevices.Remove: {0} -> {1}", device.getName(), ret);
            GameManage.mInstance.deviceList = hubcomp.mFoundDevices;
        }

        public override void onDeviceConnected(Device device)
        {
            Debug.LogFormat("onDeviceConnected, name is \'{0}\'", device.getName());
            foreach (GForceDevice dev in hubcomp.mDeviceComps)
            {
                if (dev.device== null)
                {
                    dev.device = device;
                    
                    break;
                }
            }
        }

        public override void onDeviceDisconnected(Device device, int reason)
        {
            Debug.LogFormat("onDeviceDisconnected, name is \'{0}\', reason is {1}",
                device.getName(), reason);
            bool needReconnect = false;
            foreach (GForceDevice dev in hubcomp.mDeviceComps)
            {
                if (device == dev.device)
                {
                    Debug.Log("Need rescan");
                    dev.device = null;
                    
                    needReconnect = true;
                    break;
                }
            }
            if (needReconnect)
            {
                foreach (GForceDevice dev in hubcomp.mDeviceComps)
                {
                    if (null == dev.device)
                        continue;
                    Device.ConnectionStatus status = dev.device.getConnectionStatus();
                    if (Device.ConnectionStatus.Connecting == status)
                    {
                        // a device is in connecting state, we will do nothing until
                        // connection finished (succeeded or failed)
                        Debug.Log("A device is in connecting, wait after it done");
                        return;
                    }
                }
            }
        }

        public override void onOrientationData(Device device,
            float w, float x, float y, float z)
        {
            foreach (GForceDevice dev in hubcomp.mDeviceComps)
            {
                if (device == dev.device)
                    dev.onOrientationData(w, x, y, z);
            }
        }

        public override void onGestureData(Device device, Device.Gesture gest)
        {
            foreach (GForceDevice dev in hubcomp.mDeviceComps)
            {
                if (device == dev.device)
                    dev.onGestureData(gest);
            }
        }

        public override void onDeviceStatusChanged(Device device, Device.Status status)
        {
            foreach (GForceDevice dev in hubcomp.mDeviceComps)
            {
                if (device == dev.device)
                    dev.onDeviceStatusChanged(status);
            }
        }

        public override void onExtendedDeviceData(Device device, Device.DataType type, byte[] data)
        {
            foreach (GForceDevice dev in hubcomp.mDeviceComps)
            {
                if (device == dev.device&& type == Device.DataType.Emgraw)
                    dev.onExtendedDeviceData(type,data);
            }
        }

        public Listener(GForceHub theObj)
        {
            hubcomp = theObj;
        }

        private GForceHub hubcomp = null;
    };

    Listener mLsn = null;
    private volatile bool bRunThreadRun = false;

    public string lastlog;
    private static void DebugLog(Hub.LogLevel level, string value)
    {
        mInstance.lastlog = value;
        if (level >= Hub.LogLevel.GF_LOG_ERROR)
            Debug.LogError(value);
        else
            Debug.Log(value);
    }
    private void prepare()
    {
        mFoundDevices.Clear();
        mLsn = new Listener(this);
#if !UNITY_ANDROID
            mHub.setClientLogMethod(logfun);
#endif
        RetCode ret;
 
        ret = mHub.init(0);
        Debug.LogFormat("init = {0}", ret);
        Debug.LogFormat("Hub status is {0}", mHub.getStatus());
        mHub.setWorkMode(Hub.WorkMode.Polling);
        Debug.LogFormat("New work mode is {0}", mHub.getWorkMode());
        bRunThreadRun = true;
        runThread = new Thread(new ThreadStart(runThreadFn));
        runThread.Start();
        ret = mHub.registerListener(mLsn);
        Debug.LogFormat("registerListener = {0}", ret);
        ret = mHub.startScan();
        Debug.LogFormat("startScan = {0}", ret);
        
        if (RetCode.GF_SUCCESS == ret)
        {
            lastlog = "BLE scan starting succeeded.";
        }
        else
        {
            lastlog = "BLE scan starting failed.";
        }
    }

    private void terminal()
    {
        Debug.Log("terminal");
        bRunThreadRun = false;
        if (runThread != null)
        {
            runThread.Join();
        }
        mHub.unregisterListener(mLsn);
#if !UNITY_ANDROID
            mHub.setClientLogMethod(null);
#endif
        mFoundDevices.Clear();
    }
    private Thread runThread;
    private void runThreadFn()
    {
        Debug.Log("Start runThreadFn" + bRunThreadRun);
        int loop = 0;
        while (bRunThreadRun)
        {
            RetCode ret = mHub.run(50);
            if (RetCode.GF_SUCCESS != ret && RetCode.GF_ERROR_TIMEOUT != ret)
            {
                System.Threading.Thread.Sleep(5);
                Debug.Log("hub run err:" + ret);
                continue;
            }
            loop++;
#if DEBUG
            if (loop % 200 == 0)
                Debug.LogFormat("runThreadFn: {0} seconds elapsed.", loop / 20);
#endif
        }
        Debug.Log("Leave thread");
    }

    public List<Device> GetDevice()
    {
        return mFoundDevices;
    }

    /// <summary>
    /// Currently connected devices
    /// </summary>
    public Device connectedDevice;

    public void StopScan()
    {
        Debug.Log("StopScanDevice  ");
        mHub.stopScan();
    }

    public void StartScan()
    {
        Debug.Log("StartScanDevice  ");
        mHub.startScan();
    }

    public gf.Hub.HubState GetHubState()
    {
        return mHub.getStatus();
    }
}

