#include "WebSocketPlugin.h"
#include <uwebsockets/App.h>
#include <iostream>
#include <mutex>
#include <string>
#include <functional>
#include <thread>

std::mutex connectionMutex;
bool isConnected = false;
uWS::Loop* loop = nullptr;
uWS::WebSocket<false, true, nullptr_t>* ws = nullptr;
std::thread wsThread;

extern "C" {
    typedef void (*UnityMsgCallback)(const char* msg);
    UnityMsgCallback unityCallback = nullptr;

    UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API SetMsgCallback(UnityMsgCallback callback) {
        unityCallback = callback;
    }

    UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API InitializeWebSocket(const char* url) {
        std::string urlStr(url);
        std::string host;
        std::string path = "/";
        int port = 80;

        if (urlStr.substr(0, 5) == "ws://")
            urlStr = urlStr.substr(5);

        size_t slashPos = urlStr.find('/');
        if (slashPos != std::string::npos) {
            host = urlStr.substr(0, slashPos);
            path = urlStr.substr(slashPos);
        } else {
            host = urlStr;
        }

        size_t colonPos = host.find(':');
        if (colonPos != std::string::npos) {
            port = std::stoi(host.substr(colonPos + 1));
            host = host.substr(0, colonPos);
        }

        if (loop != nullptr) {
            if (wsThread.joinable())
                wsThread.join();
            loop = nullptr;
        }

        wsThread = std::thread([host, port, path]() {
            uWS::App().ws<nullptr_t>("/*", {
                .open = [](auto* _ws) {
                    std::lock_guard<std::mutex> guard(connectionMutex);
                    ws = _ws;
                    isConnected = true;

                    if (unityCallback) {
                        std::string msg = "{\"type\":\"connection_status\",\"status\":\"connected\"}";
                        unityCallback(msg.c_str());
                    }
                },
                .message = [](auto* _ws, std::string_view msg, uWS::OpCode opcode) {
                    if (unityCallback) {
                        std::string msgStr(msg);
                        unityCallback(msgStr.c_str());
                    }
                },
                .close = [](auto* _ws, int code, std::string_view msg) {
                    std::lock_guard<std::mutex> guard(connectionMutex);
                    ws = nullptr;
                    isConnected = false;

                    if (unityCallback) {
                        std::string closeMsg = "{\"type\":\"connection_status\",\"status\":\"disconnected\"}";
                        unityCallback(closeMsg.c_str());
                    }
                }
            }).listen(port, [host, port](auto* listenSocket) {
                if (!listenSocket) {
                    if (unityCallback) {
                        std::string errorMsg = "{\"type\":\"connection_status\",\"status\":\"failed\",\"message\":\"Failed to connect\"}";
                        unityCallback(errorMsg.c_str());
                    }
                    return;
                }

                if (unityCallback) {
                    std::string errorMsg = "{\"type\":\"connection_status\",\"status\":\"error\",\"message\":\"Cannot establish client connection with uWebSockets\"}";
                    unityCallback(errorMsg.c_str());
                }
            }).run();
        });
    }

    UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API SendMsg(const char* msg) {
        std::lock_guard<std::mutex> guard(connectionMutex);
        if (!isConnected || ws == nullptr)
            return;

        ws->send(std::string(msg), uWS::OpCode::TEXT);
    }

    UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API CloseWebSocket() {
        std::lock_guard<std::mutex> guard(connectionMutex);
        if (!isConnected || ws == nullptr)
            return;

        ws->end(1000, "Closing connection");
        isConnected = false;
        ws = nullptr;
    }

    UNITY_INTERFACE_EXPORT bool UNITY_INTERFACE_API IsConnected() {
        std::lock_guard<std::mutex> guard(connectionMutex);
        return isConnected && ws != nullptr;
    }
}
