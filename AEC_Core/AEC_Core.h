#pragma once

#ifdef __cplusplus
extern "C" {
#endif

#ifdef AEC_CORE_EXPORTS
#define AEC_API __declspec(dllexport)
#else
#define AEC_API __declspec(dllimport)
#endif

/// <summary>初始化 WebRTC AEC3（48kHz / 单声道 / 10ms=480 点）。</summary>
/// <returns>非零成功</returns>
AEC_API int AEC_Init(int sampleRate, int suppressionLevel);

/// <summary>处理一帧：nearEndMic=麦克风，farEndRef=远端/回路参考；输出写入 outClean。</summary>
AEC_API void AEC_ProcessFrame(
    float* nearEndMic,
    float* farEndRef,
    float* outClean,
    int frameLength);

AEC_API void AEC_Destroy(void);

#ifdef __cplusplus
}
#endif
