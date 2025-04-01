#pragma once

#ifdef _WIN32
    #define EXPORT_API __declspec(dllexport)
#else
    #define EXPORT_API
#endif

#include <string>
#include <vector>
#include <mutex>
#include <queue>

namespace uWS {
    template <bool SSL> class WebSocket;
    template <bool SSL> class TemplatedApp;
}

struct PlayerData {
    int id;
    std::string username;
    float health;
    std::string modelType;
    float positionX;
    float positionY;
    float positionZ;
};

/* struct CardData {
    int id;
    std::string name;
    std::vector<std::string> effects;
    int ownerId;
    bool isPlayed;
    float positionX;
    float positionY;
    float positionZ;
}; */

struct AttackData {
    int attackerId;
    int targetId;
    float damage;
    std::string attackType;
};

struct GameStateData {
    std::vector<PlayerData> players;
    //std::vector<CardData> activeCards;
    std::vector<AttackData> pendingAttacks;
    int gameStatus; // 0: waiting, 1: playing, 2: finished
};

class NetworkManager {
private:
    static NetworkManager* instance;
    std::string serverUrl;
    int roomCode;
    bool isConnected;
    
    uWS::TemplatedApp<false>* app;
    uWS::WebSocket<false, true>* ws;
    
    std::mutex stateMutex;
    std::mutex messageMutex;
    
    GameStateData currentGameState;
    
    std::queue<std::string> incomingMessages;
    
    NetworkManager();
    void ProcessJsonMessage(const std::string& jsonStr);
    
public:
    static NetworkManager* GetInstance();
    
    bool Initialize(const char* url, int code);
    void Shutdown();
    bool ProcessEvents();
    const char* GetGameStateJson();
    
    int GetPlayerCount();
    int GetCardCount();
    int GetAttackCount();
    
    bool IsConnected();
};

extern "C" {
    EXPORT_API bool InitializeConnection(const char* serverUrl, int roomCode);
    EXPORT_API void ShutdownConnection();
    EXPORT_API bool ProcessEvents();
    EXPORT_API const char* GetGameStateJson();
    EXPORT_API int GetPlayerCount();
    EXPORT_API int GetCardCount();
    EXPORT_API int GetAttackCount();
    EXPORT_API bool IsConnected();
}