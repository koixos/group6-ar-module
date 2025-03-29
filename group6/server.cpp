#include <uwebsockets/App.h>
#include <iostream>
#include <unordered_map>

struct PerSocketData {
	int id;
};

std::unordered_map<int, uWS::WebSocket<false, true, PerSocketData>*> clients;
int clientId = 0;

int main() {
	uWS::App().ws<PerSocketData>("/*", {
		.open = [](auto* ws) {
			int id = clientId++;
			ws->getUserData()->id = id;
			clients[id] = ws;
			std::cout << "Client connected with id: " << id << std::endl;
		},

		.message = [](auto* ws, std::string_view message, uWS::OpCode opCode) {
			std::cout << "Received message: " << message << std::endl;
			for (auto& client : clients) {
				client.second->send(message, opCode);
			}
		},

		.close = [](auto* ws, int code, std::string_view message) {
			int id = ws->getUserData()->id;
			clients.erase(id);
			std::cout << "Client disconnected with id: " << id << std::endl;
		}
	}).listen(9001, [](auto* token) {
		if (token) {
			std::cout << "Server started at port 9001" << std::endl;
		}
		else {
			std::cout << "Server failed to start" << std::endl;
		}
	}).run();
}