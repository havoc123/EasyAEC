# EasyAEC — 零延迟声学回声消除引擎

EasyAEC 是一个专为 AI 虚拟主播和高保真变声（如 RVC）设计的极低延迟前置音频净化工具。它通过 C# WinForms 与底层 C++ WebRTC AEC3 算法的 P/Invoke 桥接，实现了在复杂的双讲环境下，完美消除外放回声并保留纯净人声。

---
## 鸣谢与底层支持 (Acknowledgments)

本项目的核心降噪算法基于 Google WebRTC 框架。
特别感谢开源社区开发者提供的干净、易于编译的 AEC3 C++ 独立版本，为本项目的跨语言桥接提供了坚实的基础。
* **核心算法来源:** [ewan-xu/AEC3](https://github.com/ewan-xu/AEC3)

## 🏗️ 系统架构设计

本项目采用前后端分离的设计思想，前端使用 C# 构建直观的控制台与设备路由，后端通过静态链接的 WebRTC AEC3 C++ DLL 进行毫秒级的音频帧切片与回声消除。

## 💾 已实现核心机制 (Core Mechanisms)

* **本地状态记忆 (Auto-Save Config):**
  * 采用 `System.Text.Json` 实现轻量级本地存储 (`config.json`)。
  * **保存逻辑**: 挂载于主窗体的 `FormClosing` 事件，退出时自动序列化界面上的 3 大音频设备选择、压制强度、延迟补偿（如 60ms）及 Debug 状态。
  * **加载逻辑**: 挂载于主窗体的 `Load` 事件，在读取到本地配置后，**带有安全校验机制**。仅当保存的设备名确实存在于当前系统的 WASAPI 列表中时才会选中，完美规避了因设备拔插导致的启动崩溃。

## 📡 自动延迟标定系统 (Auto-Calibration) - 规划中
为彻底解决多硬件组合下的延迟测量痛点，本项目设计了一套“声学雷达标定”机制，全程在内存中闭环，无外部文件依赖：

1. **测试信号源 (`RandomKickGenerator`)**: 采用 C# 原生 `ISampleProvider` 动态生成随机间隔的低音鼓点 (Kick Drum, 150Hz -> 40Hz 滑频)。低频穿透力极强，且随机间隔能打破 AEC 的周期性预测，完美模拟人类爆破音。
2. **标定算法逻辑**:
   - **状态锁定**: 强制将 AEC 压制强度设为 `0 (柔和)`，暴露真实的物理残留。
   - **步进扫描**: 从 `0ms` 扫描至 `120ms`，步长 `5ms`。
   - **收敛与采样**: 每次修改延迟后，给予 WebRTC 算法 `1500ms` 的声学模型收敛时间，随后在 `1000ms` 内抓取残留输出的最高峰值 (OutputPeak)。
   - **寻谷寻优**: 扫描结束后，自动找出残留峰值最低（即抵消最完美）的延迟常数，并自动回填至主界面。
   
```mermaid
graph TD
    subgraph UI[前端 UI 界面层 - C# WinForms]
        Config[(本地配置 .ini\n记忆上次设备)] -.-> Dropdowns
        
        subgraph 设备路由区
            Dropdowns[3大下拉菜单\n监听输入 / 监听输出 / 音频输出]
            MeterIn[监听输入 电平表]
            MeterRef["监听输出(参考) 电平表"]
            MeterOut[音频输出 电平表]
        end
        
        subgraph 核心控制区
            ParamSupp[压制强度选择\n柔和 / 标准 / 强力]
            ParamDelay[延迟补偿微调\n+/- ms 输入框]
            ChkDebug[🐞Debug 模式勾选框\n开启底层详细追踪]
            Btn((开始 / 停止\n控制按钮))
        end
        
        LogBar[底部状态栏\n仅显示简要就绪/报错状态]
    end

    subgraph Core[后端音频引擎层 - C# WASAPI + C++ AEC3 DLL]
        CapIn[监听输入捕获\nWASAPI Capture]
        CapRef["监听输出捕获(环出)"\nWASAPI Loopback]
        
        Resample[重采样模块\n强制转换至 48kHz]
        
        Sync[环形缓冲与时钟同步器\n对齐两条音频流]
        Split[10ms 帧切片器\n严格切割 10ms 数据块]
        
        AEC3{WebRTC AEC3 算法核心\nC++ 动态链接库}
        
        Render[音频渲染输出\nWASAPI Render]
        
        LogWriter[结构化日志写入器\n抓取引擎各模块运行参数]
    end
    
    subgraph 物理与虚拟外部环境
        HW_Mic[物理麦克风]
        HW_Spk[音响 / 电视外放]
        VAC[虚拟声卡 VB-Cable]
        RVC[RVC 变声器]
        LogFile[(本地 Debug.log 文件\n标准化格式供 AI 分析)]
    end

    Btn -.->|下发启停指令| Core
    ParamDelay -.->|动态注入时间偏移值| Sync
    ParamSupp -.->|注入 NLP 非线性处理强度| AEC3
    Dropdowns -.->|绑定底层音频设备 ID| Core
    
    ChkDebug -.->|开启 Trace 级别追踪| LogWriter
    Core -.->|抛出异常/关键节点| LogBar
    CapIn -.->|状态报告| LogWriter
    CapRef -.->|状态报告| LogWriter
    Sync -.->|缓冲与漂移数据| LogWriter
    AEC3 -.->|算法执行耗时/状态| LogWriter
    LogWriter -.->|持续写入| LogFile

    HW_Mic -->|近端人声+电视回音| CapIn
    HW_Spk -.->|远端纯电视音| CapRef
    
    CapIn -->|实时振幅数据| MeterIn
    CapRef -->|实时振幅数据| MeterRef
    Render -->|实时振幅数据| MeterOut
    
    CapIn ==> Resample
    CapRef ==> Resample
    
    Resample ==> Sync
    Sync ==> Split
    
    Split ==>|10ms 近端音频帧| AEC3
    Split ==>|10ms 远端参考帧| AEC3
    
    AEC3 ==>|消除回声后的纯净人声| Render
    
    Render ==>|纯净音频流| VAC
    VAC ==>|直接喂给变声器| RVC
	
	