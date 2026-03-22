#include "AEC_Core.h"

#include <cstring>

int AEC_Init(int sampleRate, int suppressionLevel)
{
    (void)sampleRate;
    (void)suppressionLevel;
    return 1;
}

void AEC_ProcessFrame(float* nearEndMic, float* farEndRef, float* outClean, int frameLength)
{
    (void)farEndRef;
    if (nearEndMic == nullptr || outClean == nullptr || frameLength <= 0)
        return;

    const size_t bytes = static_cast<size_t>(frameLength) * sizeof(float);
    std::memcpy(outClean, nearEndMic, bytes);
}

void AEC_Destroy(void)
{
}
