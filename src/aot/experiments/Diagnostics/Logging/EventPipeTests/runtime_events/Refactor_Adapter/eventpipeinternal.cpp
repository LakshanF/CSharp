// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#include "common.h"
#include "eventpipeadapter.h"

#include "gcenv.h"
#include "regdisplay.h"
#include "StackFrameIterator.h"
#include "thread.h"
#include "SpinLock.h"

#ifdef FEATURE_PERFTRACING

struct EventPipeEventInstanceData
{
    void *ProviderID;
    unsigned int EventID;
    unsigned int ThreadID;
    LARGE_INTEGER TimeStamp;
    GUID ActivityId;
    GUID RelatedActivityId;
    const uint8_t *Payload;
    unsigned int PayloadLength;
};

struct EventPipeSessionInfo
{
    int64_t StartTimeAsUTCFileTime;
    int64_t StartTimeStamp;
    uint64_t TimeStampFrequency;
};

struct EventPipeProviderConfigurationNative
{
    wchar_t *pProviderName;
    uint64_t keywords;
    uint32_t loggingLevel;
    wchar_t *pFilterData;
};

EXTERN_C NATIVEAOT_API uint64_t __cdecl RhEventPipeInternal_Enable(
    LPCWSTR outputFile,
    EventPipeSerializationFormat format,
    uint32_t circularBufferSizeInMB,
    /* COR_PRF_EVENTPIPE_PROVIDER_CONFIG */ const void * pProviders,
    uint32_t numProviders)
{
    uint64_t sessionID = 0;
    // Invalid input!
    if (circularBufferSizeInMB == 0 ||
        format >= EP_SERIALIZATION_FORMAT_COUNT ||
        numProviders == 0 ||
        pProviders == nullptr)
    {
        return 0;
    }

    const EventPipeProviderConfigurationNative *nativeProviders = reinterpret_cast<const EventPipeProviderConfigurationNative *>(pProviders);

    EventPipeProviderConfiguration * configProviders = new EventPipeProviderConfiguration[numProviders];
    if (configProviders) {
        for (uint32_t i = 0; i < numProviders; ++i) {
            ep_provider_config_init (
                &configProviders[i],
                ep_rt_utf16_to_utf8_string (reinterpret_cast<const ep_char16_t *>(nativeProviders[i].pProviderName), -1),
                nativeProviders[i].keywords,
                static_cast<EventPipeEventLevel>(nativeProviders[i].loggingLevel),
                ep_rt_utf16_to_utf8_string (reinterpret_cast<const ep_char16_t *>(nativeProviders[i].pFilterData), -1));
        }
    }

        ep_char8_t *outputPathUTF8 = NULL;
        if (outputFile)
            outputPathUTF8 = ep_rt_utf16_to_utf8_string (reinterpret_cast<const ep_char16_t *>(outputFile), -1);
        EventPipeSessionID result = ep_enable (
            outputPathUTF8,
            circularBufferSizeInMB,
            configProviders,
            numProviders,
            outputFile != NULL ? EP_SESSION_TYPE_FILE : EP_SESSION_TYPE_LISTENER,
            format,
            false,
            nullptr,
            nullptr,
            nullptr);
        ep_rt_utf8_string_free (outputPathUTF8);

    // pProviders = reinterpret_cast<const EventPipeProviderConfigurationNative *>(configProviders);

    // EventPipeProviderConfigurationAdapter configAdapter(reinterpret_cast<const COR_PRF_EVENTPIPE_PROVIDER_CONFIG *>(pProviders), numProviders);

    // sessionID = EventPipeAdapter::Enable(
    //     outputFile,
    //     circularBufferSizeInMB,
    //     configAdapter,
    //     outputFile != NULL ? EP_SESSION_TYPE_FILE : EP_SESSION_TYPE_LISTENER,
    //     format,
    //     true,
    //     nullptr,
    //     nullptr,
    //     nullptr);
    // EventPipeAdapter::StartStreaming(sessionID);

    // return sessionID;

    return result;
}

EXTERN_C NATIVEAOT_API void __cdecl RhEventPipeInternal_Disable(uint64_t sessionID)
{
    EventPipeAdapter::Disable(sessionID);
}

EXTERN_C NATIVEAOT_API intptr_t __cdecl RhEventPipeInternal_CreateProvider(
    LPCWSTR providerName,
    EventPipeCallback pCallbackFunc,
    void* pCallbackContext)
{
    EventPipeProvider* pProvider = EventPipeAdapter::CreateProvider(providerName, pCallbackFunc, pCallbackContext);
    return reinterpret_cast<intptr_t>(pProvider);
}

EXTERN_C NATIVEAOT_API intptr_t __cdecl RhEventPipeInternal_DefineEvent(
    intptr_t provHandle,
    uint32_t eventID,
    int64_t keywords,
    uint32_t eventVersion,
    uint32_t level,
    void *pMetadata,
    uint32_t metadataLength)
{
    EventPipeEvent *pEvent = NULL;

    _ASSERTE(provHandle != 0);
    EventPipeProvider *pProvider = reinterpret_cast<EventPipeProvider *>(provHandle);
    pEvent = EventPipeAdapter::AddEvent(pProvider, eventID, keywords, eventVersion, (EventPipeEventLevel)level, /* needStack = */ true, (uint8_t *)pMetadata, metadataLength);
    _ASSERTE(pEvent != NULL);

    return reinterpret_cast<intptr_t>(pEvent);
}

EXTERN_C NATIVEAOT_API intptr_t __cdecl RhEventPipeInternal_GetProvider(LPCWSTR providerName)
{
    EventPipeProvider* pProvider = EventPipeAdapter::GetProvider(providerName);
    return reinterpret_cast<intptr_t>(pProvider);
}

EXTERN_C NATIVEAOT_API void __cdecl RhEventPipeInternal_DeleteProvider(intptr_t provHandle)
{
    if (provHandle != 0)
    {
        EventPipeProvider *pProvider = reinterpret_cast<EventPipeProvider *>(provHandle);
        EventPipeAdapter::DeleteProvider(pProvider);
    }
}

EXTERN_C NATIVEAOT_API int __cdecl RhEventPipeInternal_EventActivityIdControl(uint32_t controlCode, GUID *pActivityId)
{
    PalDebugBreak();
    return 0;
}

EXTERN_C NATIVEAOT_API void __cdecl RhEventPipeInternal_WriteEventData(
    intptr_t eventHandle,
    EventData *pEventData,
    uint32_t eventDataCount,
    const GUID * pActivityId,
    const GUID * pRelatedActivityId)
{
    _ASSERTE(eventHandle != 0);
    EventPipeEvent *pEvent = reinterpret_cast<EventPipeEvent *>(eventHandle);
    EventPipeAdapter::WriteEvent(pEvent, pEventData, eventDataCount, pActivityId, pRelatedActivityId);
}

EXTERN_C NATIVEAOT_API UInt32_BOOL __cdecl RhEventPipeInternal_GetSessionInfo(uint64_t sessionID, EventPipeSessionInfo *pSessionInfo)
{
    bool retVal = false;
    if (pSessionInfo != NULL)
    {
        EventPipeSession *pSession = ep_get_session(sessionID);
        if (pSession != NULL)
        {
            pSessionInfo->StartTimeAsUTCFileTime = ep_session_get_session_start_time (pSession);
            pSessionInfo->StartTimeStamp = ep_session_get_session_start_timestamp(pSession);
            pSessionInfo->TimeStampFrequency = PalQueryPerformanceFrequency();
            retVal = true;
        }
    }
    return retVal;
}

EXTERN_C NATIVEAOT_API UInt32_BOOL __cdecl RhEventPipeInternal_GetNextEvent(uint64_t sessionID, EventPipeEventInstanceData *pInstance)
{
    EventPipeEventInstance *pNextInstance = NULL;
    _ASSERTE(pInstance != NULL);

    pNextInstance = EventPipeAdapter::GetNextEvent(sessionID);
    if (pNextInstance)
    {
        pInstance->ProviderID = EventPipeAdapter::GetEventProvider(pNextInstance);
        pInstance->EventID = EventPipeAdapter::GetEventID(pNextInstance);
        pInstance->ThreadID = static_cast<uint32_t>(EventPipeAdapter::GetEventThreadID(pNextInstance));
        pInstance->TimeStamp.QuadPart = EventPipeAdapter::GetEventTimestamp(pNextInstance);
        pInstance->ActivityId = *EventPipeAdapter::GetEventActivityID(pNextInstance);
        pInstance->RelatedActivityId = *EventPipeAdapter::GetEventRelativeActivityID(pNextInstance);
        pInstance->Payload = EventPipeAdapter::GetEventData(pNextInstance);
        pInstance->PayloadLength = EventPipeAdapter::GetEventDataLen(pNextInstance);
    }

    return pNextInstance != NULL;
}

EXTERN_C NATIVEAOT_API UInt32_BOOL __cdecl RhEventPipeInternal_SignalSession(uint64_t sessionID)
{
    return EventPipeAdapter::SignalSession(sessionID);
}

EXTERN_C NATIVEAOT_API UInt32_BOOL __cdecl RhEventPipeInternal_WaitForSessionSignal(uint64_t sessionID, int32_t timeoutMs)
{
    return EventPipeAdapter::WaitForSessionSignal(sessionID, timeoutMs);
}

#endif // FEATURE_PERFTRACING
