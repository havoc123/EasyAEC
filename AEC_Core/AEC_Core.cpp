#include "AEC_Core.h"

#include <algorithm>
#include <memory>

#include "api/echo_canceller3_config.h"
#include "api/echo_canceller3_factory.h"
#include "api/echo_control.h"
#include "audio_processing/audio_buffer.h"
#include "audio_processing/high_pass_filter.h"
#include "audio_processing/include/audio_processing.h"

namespace {

constexpr int kLinearOutputRateHz = 16000;
constexpr int kFrameSamples48k = 480;

std::unique_ptr<webrtc::EchoControl> g_echo;
std::unique_ptr<webrtc::HighPassFilter> g_hp;
std::unique_ptr<webrtc::AudioBuffer> g_render;
std::unique_ptr<webrtc::AudioBuffer> g_capture;
std::unique_ptr<webrtc::AudioBuffer> g_linear;
int g_sample_rate_hz = 0;

void ResetState()
{
    g_echo.reset();
    g_hp.reset();
    g_render.reset();
    g_capture.reset();
    g_linear.reset();
    g_sample_rate_hz = 0;
}

void ApplySuppressionLevel(webrtc::EchoCanceller3Config& cfg, int level)
{
    // 与 UI「柔和 / 标准 / 强力」粗略对应（在 Validate 前微调典型参数）
    level = std::clamp(level, 0, 2);
    if (level == 0)
    {
        cfg.erle.min = 0.85f;
        cfg.ep_strength.default_gain = 0.95f;
    }
    else if (level == 2)
    {
        cfg.erle.min = 1.1f;
        cfg.ep_strength.default_gain = 1.05f;
    }
    webrtc::EchoCanceller3Config::Validate(&cfg);
}

}  // namespace

extern "C" {

AEC_API int AEC_Init(int sampleRate, int suppressionLevel)
{
    ResetState();

    if (sampleRate != 48000)
        return 0;

    webrtc::EchoCanceller3Config cfg;
    cfg.filter.export_linear_aec_output = true;
    ApplySuppressionLevel(cfg, suppressionLevel);

    webrtc::EchoCanceller3Factory factory(cfg);
    g_echo = factory.Create(sampleRate, 1, 1);
    if (!g_echo)
        return 0;

    g_hp = std::make_unique<webrtc::HighPassFilter>(sampleRate, 1);

    const webrtc::StreamConfig stream_cfg(sampleRate, 1, false);
    g_render = std::make_unique<webrtc::AudioBuffer>(
        stream_cfg.sample_rate_hz(),
        stream_cfg.num_channels(),
        stream_cfg.sample_rate_hz(),
        stream_cfg.num_channels(),
        stream_cfg.sample_rate_hz(),
        stream_cfg.num_channels());
    g_capture = std::make_unique<webrtc::AudioBuffer>(
        stream_cfg.sample_rate_hz(),
        stream_cfg.num_channels(),
        stream_cfg.sample_rate_hz(),
        stream_cfg.num_channels(),
        stream_cfg.sample_rate_hz(),
        stream_cfg.num_channels());
    g_linear = std::make_unique<webrtc::AudioBuffer>(
        kLinearOutputRateHz,
        1,
        kLinearOutputRateHz,
        1,
        kLinearOutputRateHz,
        1);

    g_sample_rate_hz = sampleRate;
    return 1;
}

AEC_API void AEC_ProcessFrame(
    float* nearEndMic,
    float* farEndRef,
    float* outClean,
    int frameLength)
{
    if (!g_echo || !g_render || !g_capture || !g_linear || !g_hp)
        return;
    if (nearEndMic == nullptr || farEndRef == nullptr || outClean == nullptr)
        return;
    if (frameLength != kFrameSamples48k || g_sample_rate_hz != 48000)
        return;

    const webrtc::StreamConfig cfg(48000, 1, false);

    float* render_ptrs[1] = { farEndRef };
    float* capture_ptrs[1] = { nearEndMic };

    g_render->CopyFrom(render_ptrs, cfg);
    g_capture->CopyFrom(capture_ptrs, cfg);

    g_render->SplitIntoFrequencyBands();
    g_echo->AnalyzeRender(g_render.get());
    g_render->MergeFrequencyBands();

    g_echo->AnalyzeCapture(g_capture.get());
    g_capture->SplitIntoFrequencyBands();
    g_hp->Process(g_capture.get(), true);
    g_echo->SetAudioBufferDelay(0);
    g_echo->ProcessCapture(g_capture.get(), g_linear.get(), false);
    g_capture->MergeFrequencyBands();

    float* out_ptrs[1] = { outClean };
    g_capture->CopyTo(cfg, out_ptrs);
}

AEC_API void AEC_Destroy(void)
{
    ResetState();
}

}  // extern "C"
