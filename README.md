# GforceProGesture

=====
 #### 1. 使用
  打开项目 (推荐切换到 Android 环境下)  
  打开场景 GforcePro  
  点击deviceConnect 按钮 打开设备连接界面(在此之前需要先打开gforce手环)  
  点击扫描到对应设备的connecet  
  **如果连接正确,gforcePro会震动,电源指示灯会常量,一段时间后,电源指示灯快闪后即可正常使用**  
  点击close 关闭设备连接界面  
  点击leftHand 或者RightHand选择左右手
  点击Start开始使用  
  佩戴gforcePro 充电口朝手心  
  做手势1 抓起物体  
  做手势2 放开物体  
  **需要先在gfrocePro app 训练手势**

  <img src="Assets/redeme_image/Device.png"/>
  
#### 2. SDK 
sdk目前仅支持 win 和 Android 环境  
dll都在Plugins中

#### 3. 开发
 #### Device 直接对Devcei发送命令的类
 **注意:频繁的发送命令可能导致设备或者hub死机**
 * getAddress(out string) 获取蓝牙地址  
  需要填写一个string作为接受值  
 * getAddress() 返回蓝牙地址  
  实际和上个方法一样,此方法自带一个string
 * getName(out string)  
  需要填写一个string作为接受值 
 * getName() 返回蓝牙地址  
  实际和上个方法一样,此方法自带一个string
 * getRssi()
  获取 蓝牙信号强度 
 * getConnectionStatus 获取连接状态
   获取设备的连接状态:分为 断开,断开中,连接,连接中
 * connect 连接该设备
   连接设备
 * disconnect 断开该设备
   断开设备
 * setDataSwitch(uint) 设置数据开关
   根据项目需求可以打开不同的数据开关  
   **注意: 如果要打开多个开关需要用 | 进行连接,如果已打开的开关在这次中没有通过 | 连接,则会被关闭**  
   该方法不可以更改完成回调
 * setDataSwitch(uint,ResultCallback) 设置数据开关
   根据项目需求可以打开不同的数据开关  
   该方法可以自定义完成回调
 * setEmgConfig 设置数据传输
   设置 数据传输的 采集率,每秒采集次数,传输包长度
 * enableDataNotification 设置数据开关
   设置数据开关 0为关,1为开

 #### Hub 对于hub 的操作
  * init 初始化
  * deinit 销毁
  * setWorkMode 设置工作模式
  * getWorkMode 获取当前工作模式
  * getStatus 获取hub单腔状态
  * registerListener 注册回调
  * unregisterListener 取消回调
  * startScan 开始扫描
  * stopScan 结束扫描
  * run 运行
  * Dispose 销毁处理
 ##### GameManage 负责更新连接界面的UI, 连接设备,抓取物体
  * showDeviceItem()  
  创建初始化设备UI
  * StopScan()
  停止扫描  
  * Connect(Device)  
  连接设备 设备为device  
  **注意连接设备前需要先要hub停止扫描**
  * Disconnect  
  断开当前连接的设备  
  * SetHand(int)  
  设置左右手 0为left 1为right
  * RotatingHand(Quaternion)  
    设置场景中的手臂的四元数
  * GetGesture(int)  
    获取手势编号
  * setGfroceDevice()  
    设置设备的数据参数,打开数据开关
##### GForceDevice 负责设备数据回调
  **该类的回调皆为多线程的回调,无法直接调用Unity的生命周期函数或者在Unity的生命周期函数类使用**
  * onOrientationData() 设备的四元数  
    该回调分别返回了四元数的四个变量并非整个四元数
    因为产品的实际佩戴方式会不同,具体的变化需要更具实际要求进行旋转变化
  * onGestureData() 设备当前的手势  
    本项目不支持训练手势,仅能获取设备的手势,目前支持0~8个的手势识别
  * onDeviceStatusChanged() 设备当前设备发生变化  
    ReCenter 按下电源键  
    UsbPlugged 插上充电线  
    UsbPulled 拔下充电线
  * onExtendedDeviceData() 设备其他数据的返回  
    设备除了陀螺仪和手势以外其他数据的回调返回,具体数据类型会根据不同的需求选择
##### GForceHub 负责对于hub的设置和设备接受回调
 * Listener 回调  
   调用GforceDevice的回调的类,设备的回调会先由hub收到,再通过hub调用不同的设备的数据回调,本项目中仅展示了单个设备的数据回调
   * onScanFinished 扫描结束的回调
   * onStateChanged hub 状态更改的回调
   * onDeviceFound 找到设备的回调
   * onDeviceDiscard 设备失效的回调
   * onDeviceConnected 连接完成的回调
   * onDeviceDisconnected 设备断开完成的回调
   * onOrientationData 设备四元数的回调
   * onGestureData 手势回调
   * onDeviceStatusChanged 设备状态变化的回调
   * onExtendedDeviceData 其他数据的回调
 * prepare 初始化hub
 * terminal 销毁断开hub
 * runThreadFn 检查hub是否在正常工作  
   此函数是hub的心跳函数,该函数为多线程开启,并且每隔一段时间就会打印log记录运行时间,如果没用发送则需要重新初始化
 * GetDevice 获取当前连接设备
 * StopScan 停止扫描
 * StartScan 开始扫描
 * GetHubState 获取hub当前状态

#### 4. 常见问题
1. Q: 为什么我没有扫描到设备  
   A:如果你在PC上(不管是不是安卓环境下) 你需要检查 设备初始化是否正确并完成,扫描是否正常开启,duangoushi是否在正常工作

2. Q:连接点击无反应  
   A:请检查连接的是否是正确设备,PC上扫描和连接有概率会丢失,可以多连接几次

3. Q:点解连接无反应,但是设备正常震动  
   A:检查电源指示灯是否常量,如果是常量,需要等待至电源灯快闪,如果10s后依然是常量,则需要重启设备或者软件如果是PC端还需要拔插dogou
