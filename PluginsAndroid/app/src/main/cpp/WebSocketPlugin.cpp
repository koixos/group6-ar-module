#include <utility>
#include <websocketpp/config/asio_no_tls_client.hpp>
#include <websocketpp/client.hpp>
#include <nlohmann/json.hpp>
#include <android/log.h>
#include <thread>
#include <mutex>

#define LOG(...) ((void)__android_log_print(ANDROID_LOG_INFO, "UnityWebSocket", __VA_ARGS__))

using json = nlohmann::json;
typedef websocketpp::client<websocketpp::config::asio_client> client;
typedef void(*MessageCallback)(const char *msg);

static MessageCallback unityCallback = nullptr;
std::unique_ptr<client> wsClient;
websocketpp::connection_hdl connection_hdl;
std::thread wsThread;
std::mutex wsMutex;
bool isConnected = false;

extern "C" {
    void InitializeWebSocket(const char *, MessageCallback);
    //void SendMessage(const char *);
    void CloseWebSocket();
    bool IsConnected();
}

extern "C" {
    void InitializeWebSocket(const char *url, MessageCallback cb) {
        try {
            unityCallback = cb;

            if (wsThread.joinable()) {
                wsClient->stop();
                wsThread.join();
            }

            wsClient = std::make_unique<client>();

            wsClient->clear_access_channels(websocketpp::log::alevel::all);
            wsClient->clear_error_channels(websocketpp::log::elevel::all);

            wsClient->init_asio();

            wsClient->set_open_handler([](websocketpp::connection_hdl hdl) {
                std::lock_guard<std::mutex> lock(wsMutex);
                connection_hdl = std::move(hdl);
                isConnected = true;
                LOG("Websocket connected successfully");
            });

            wsClient->set_close_handler([](const websocketpp::connection_hdl& hdl) {
                std::lock_guard<std::mutex> lock(wsMutex);
                isConnected = false;
                LOG("Websocket closed");
            });

            wsClient->set_fail_handler([](const websocketpp::connection_hdl& hdl) {
                std::lock_guard<std::mutex> lock(wsMutex);
                isConnected = false;
                LOG("Websocket connection failed");
            });

            wsClient->set_message_handler([](const websocketpp::connection_hdl&, const client::message_ptr& msg) {
                std::string payload = msg->get_payload();
                LOG("Received: %s", payload.c_str());
                if (unityCallback) {
                    try {
                        const char *raw = strdup(payload.c_str());
                        unityCallback(raw);
                        free((void*)raw);
                    } catch (...) {
                        LOG("Exception while calling Unity callback");
                    }
                }
            });

            websocketpp::lib::error_code ec;
            client::connection_ptr con = wsClient->get_connection(url, ec);

            if (ec) {
                LOG("Connection initialization error: %s", ec.message().c_str());
                return;
            }

            auto res = wsClient->connect(con);

            wsThread = std::thread([&]() {
                try {
                    LOG("Running wsClient->run...");
                    wsClient->run();
                    LOG("Finished wsClient->run");
                } catch (const std::exception& e) {
                    LOG("WebSocket thread exception: %s", e.what());
                } catch (...) {
                    LOG("Unknown exception in WebSocket thread");
                }

                std::lock_guard<std::mutex> lock(wsMutex);
                isConnected = false;
            });

            LOG("WebSocket initialization complete for URL: %s", url);
        } catch (const std::exception& e) {
            LOG("Exception in InitializeWebSocket: %s", e.what());
        } catch (...) {
            LOG("Unknown exception in InitializeWebSocket");
        }
    }

    /*void SendMessage(const char *msg) {
        std::lock_guard<std::mutex> lock(wsMutex);

        if (!isConnected) {
            LOG("Websocket not connected.");
            return;
        }

        try {
            websocketpp::lib::error_code ec;
            wsClient->send(connection_hdl, msg, websocketpp::frame::opcode::text, ec);

            if (ec) {
                LOG("Send failed: %s", ec.message().c_str());
                isConnected = false;
            } else {
                LOG("Message sent successfully");
            }
        } catch (const std::exception& e) {
            LOG("Exception in SendMessage: %s", e.what());
            isConnected = false;
        } catch (...) {
            LOG("Unknown exception in SendMessage");
            isConnected = false;
        }
    }*/

    void CloseWebSocket() {
        try {
            {
                std::lock_guard<std::mutex> lock(wsMutex);
                if (isConnected) {
                    websocketpp::lib::error_code ec;
                    wsClient->close(connection_hdl, websocketpp::close::status::normal, "Closing connection", ec);
                    std::this_thread::sleep_for(std::chrono::milliseconds(100));

                    if (ec)
                        LOG("Close failed: %s", ec.message().c_str());
                    else
                        LOG("Close request sent successfully");
                }
            }

            wsClient->stop();

            if (wsThread.joinable()) {
                wsThread.join();
                LOG("WebSocket thread joined successfully");
            }

            std::lock_guard<std::mutex> lock(wsMutex);
            isConnected = false;

            LOG("WebSocket closed completely");
        } catch (const std::exception& e) {
            LOG("Exception in CloseWebSocket: %s", e.what());
        } catch (...) {
            LOG("Unknown exception in CloseWebSocket");
        }
    }

    bool IsConnected() {
        std::lock_guard<std::mutex> lock(wsMutex);
        return isConnected;
    }
}
