#include "NetworkInterface.h"
#include <uwebsockets/App.h>
#include <nlohmann/json.hpp>
#include <thread>
#include <chrono>
#include <iostream>

using json = nlohmann::json;

NetworkManager* NetworkManager::instance = nullptr;

NetworkManager* NetworkManager::GetInstance() {
    return instance == nullptr ? new NetworkManager() : instance;
}

NetworkManager::NetworkManager() 
    : serverUrl(""), roomCode(0), isConnected(false), app(nullptr), ws(nullptr) {
}

bool NetworkManager::Initialize(const char* url, int code) {
    serverUrl = url;
    roomCode = code;
    isConnected = false;
    
    app = new uWS::App();
    
    // Configure WebSocket connection
    app->ws<false>("/*", {
        .open = [this](auto* ws) {
            this->ws = ws;
            
            json joinMsg = {
                {"type", "join"},
                {"role", "spectator"},
                {"roomCode", this->roomCode}
            };
            
            ws->send(joinMsg.dump(), uWS::OpCode::TEXT);
            this->isConnected = true;
            std::cout << "Connected to server, joining room: " << this->roomCode << std::endl;
        },
        
        .message = [this](auto* ws, std::string_view message, uWS::OpCode opCode) {
            if (opCode == uWS::OpCode::TEXT) {
                std::string msgStr(message);
                std::lock_guard<std::mutex> lock(this->messageMutex);
                this->incomingMessages.push(msgStr);
            }
        },
        
        .close = [this](auto* ws, int code, std::string_view message) {
            this->isConnected = false;
            this->ws = nullptr;
            std::cout << "Disconnected from server: " << code << std::endl;
        }
    });
    
    std::thread connectionThread([this, url]() {
        app->connect(url, [](auto *token) {
            std::cout << "Failed to connect to server" << std::endl;
        });
        app->run();
    });
    connectionThread.detach();
    
    std::this_thread::sleep_for(std::chrono::milliseconds(500));
    
    return isConnected;
}

bool NetworkManager::ProcessEvents() {
    std::queue<std::string> messagesToProcess;
    
    {
        std::lock_guard<std::mutex> lock(messageMutex);
        std::swap(messagesToProcess, incomingMessages);
    }
    
    bool hadMessages = !messagesToProcess.empty();
    while (!messagesToProcess.empty()) {
        ProcessJsonMessage(messagesToProcess.front());
        messagesToProcess.pop();
    }
    return hadMessages;
}

void NetworkManager::ProcessJsonMessage(const std::string& jsonStr) {
    try {
        json data = json::parse(jsonStr);
        std::string messageType = data["type"].get<std::string>();
        
        if (messageType == "game_state") {
            std::lock_guard<std::mutex> lock(stateMutex);
            
            if (data.contains("players") && data["players"].is_array()) {
                currentGameState.players.clear();
                
                for (auto& playerJson : data["players"]) {
                    PlayerData player;
                    player.id = playerJson["id"];
                    player.username = playerJson["username"];
                    player.health = playerJson["health"];
                    player.modelType = playerJson["modelType"];
                    player.positionX = playerJson["position"]["x"];
                    player.positionY = playerJson["position"]["y"];
                    player.positionZ = playerJson["position"]["z"];
                    currentGameState.players.push_back(player);
                }
            }
            /* 
            if (data.contains("cards") && data["cards"].is_array()) {
                currentGameState.activeCards.clear();
                
                for (auto& cardJson : data["cards"]) {
                    CardData card;
                    card.id = cardJson["id"];
                    card.name = cardJson["name"];
                    card.ownerId = cardJson["ownerId"];
                    card.isPlayed = cardJson["isPlayed"];
                    
                    if (cardJson.contains("position")) {
                        card.positionX = cardJson["position"]["x"];
                        card.positionY = cardJson["position"]["y"];
                        card.positionZ = cardJson["position"]["z"];
                    }
                    
                    if (cardJson.contains("effects") && cardJson["effects"].is_array()) {
                        for (auto& effect : cardJson["effects"]) {
                            card.effects.push_back(effect);
                        }
                    }
                    
                    currentGameState.activeCards.push_back(card);
                }
            } */
            
            if (data.contains("attacks") && data["attacks"].is_array()) {
                currentGameState.pendingAttacks.clear();
                
                for (auto& attackJson : data["attacks"]) {
                    AttackData attack;
                    attack.sourcePlayerId = attackJson["attackerId"];
                    attack.targetPlayerId = attackJson["targetId"];
                    attack.attackType = attackJson["attackType"];
                    attack.damage = attackJson["damage"];                    
                    currentGameState.pendingAttacks.push_back(attack);
                }
            }
            
            if (data.contains("gameStatus"))
                currentGameState.gameStatus = data["gameStatus"];
        }
    }
    catch (json::exception& e) {
        std::cerr << "JSON parsing error: " << e.what() << std::endl;
    }
    catch (std::exception& e) {
        std::cerr << "Error processing message: " << e.what() << std::endl;
    }
}

const char* NetworkManager::GetGameStateJson() {
    static std::string jsonStr;
    
    std::lock_guard<std::mutex> lock(stateMutex);
    
    json state;
    
    json playersJson = json::array();
    for (const auto& player : currentGameState.players) {
        json playerJson = {
            {"id", player.id},
            {"username", player.username},
            {"health", player.health},
            {"modelType", player.modelType},
            {"position", {
                {"x", player.positionX},
                {"y", player.positionY},
                {"z", player.positionZ}
            }}
        };
        playersJson.push_back(playerJson);
    }
    state["players"] = playersJson;
    
    /* json cardsJson = json::array();
    for (const auto& card : currentGameState.activeCards) {
        json cardJson = {
            {"id", card.id},
            {"name", card.name},
            {"ownerId", card.ownerId},
            {"isPlayed", card.isPlayed},
            {"position", {
                {"x", card.positionX},
                {"y", card.positionY},
                {"z", card.positionZ}
            }}
        };
        
        json effectsJson = json::array();
        for (const auto& effect : card.effects) {
            effectsJson.push_back(effect);
        }
        cardJson["effects"] = effectsJson;
        
        cardsJson.push_back(cardJson);
    }
    state["cards"] = cardsJson; */
    
    json attacksJson = json::array();
    for (const auto& attack : currentGameState.pendingAttacks) {
        json attackJson = {
            {"attackerId", attack.attackerId},
            {"targetId", attack.targetId},
            {"attackType", attack.attackType},
            {"damage", attack.damage},
        };
        attacksJson.push_back(attackJson);
    }
    state["attacks"] = attacksJson;
    
    state["gameStatus"] = currentGameState.gameStatus;
    
    jsonStr = state.dump();
    return jsonStr.c_str();
}

void NetworkManager::Shutdown() {
    if (ws != nullptr) ws = nullptr;
    if (app != nullptr) app = nullptr;
    isConnected = false;
}

int NetworkManager::GetPlayerCount() {
    std::lock_guard<std::mutex> lock(stateMutex);
    return static_cast<int>(currentGameState.players.size());
}

int NetworkManager::GetCardCount() {
    std::lock_guard<std::mutex> lock(stateMutex);
    return static_cast<int>(currentGameState.activeCards.size());
}

int NetworkManager::GetAttackCount() {
    std::lock_guard<std::mutex> lock(stateMutex);
    return static_cast<int>(currentGameState.pendingAttacks.size());
}

bool NetworkManager::IsConnected() {
    return isConnected;
}

extern "C" {
    EXPORT_API bool InitializeConnection(const char* serverUrl, int roomCode) {
        return NetworkManager::GetInstance()->Initialize(serverUrl, roomCode);
    }
    
    EXPORT_API void ShutdownConnection() {
        NetworkManager::GetInstance()->Shutdown();
    }
    
    EXPORT_API bool ProcessEvents() {
        return NetworkManager::GetInstance()->ProcessEvents();
    }
    
    EXPORT_API const char* GetGameStateJson() {
        return NetworkManager::GetInstance()->GetGameStateJson();
    }
    
    EXPORT_API int GetPlayerCount() {
        return NetworkManager::GetInstance()->GetPlayerCount();
    }
    
    EXPORT_API int GetCardCount() {
        return NetworkManager::GetInstance()->GetCardCount();
    }
    
    EXPORT_API int GetAttackCount() {
        return NetworkManager::GetInstance()->GetAttackCount();
    }
    
    EXPORT_API bool IsConnected() {
        return NetworkManager::GetInstance()->IsConnected();
    }
}