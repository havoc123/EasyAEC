#pragma once

#ifdef __cplusplus
extern "C" {
#endif

#ifdef AEC_CORE_EXPORTS
#define AEC_API __declspec(dllexport)
#else
#define AEC_API __declspec(dllimport)
#endif

/// <returns>非零表示初始化成功（Mock 恒为 1）</returns>
AEC_API int AEC_Init(int sampleRate, int suppressionLevel);

/// <summary>处理一帧音频；Mock 将 nearEndMic 拷贝到 outClean。</summary>
AEC_API void AEC_ProcessFrame(
    float* nearEndMic,
    float* farEndRef,
    float* outClean,
    int frameLength);

AEC_API void AEC_Destroy(void);

#ifdef __cplusplus
}
#endif
