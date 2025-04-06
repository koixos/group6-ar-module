#pragma once

#if defined(_MSC_VER)
    // Windows
    #define UNITY_INTERFACE_API __stdcall
    #define UNITY_INTERFACE_EXPORT __declspec(dllexport)
#else
    // macOS, Linux
    #define UNITY_INTERFACE_API
    #define UNITY_INTERFACE_EXPORT __attribute__((visibility("default")))
#endif

extern "C" {
    UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API SetMsgCallback(void (*callback)(const char* msg));
    UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API InitializeWebSocket(const char* url);
    UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API SendMsg(const char* msg);
    UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API CloseWebSocket();
    UNITY_INTERFACE_EXPORT bool UNITY_INTERFACE_API IsConnected();
}
