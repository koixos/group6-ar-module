from flask import Flask, jsonify
import json

app = Flask(__name__)

with open("game_state_data.json", "r", encoding="utf-8") as file:
    game_state_data = json.load(file)

currInd = 0


@app.route("/api/ar/681c8ee256a702d8c1500b40", methods=["GET"])
def get_game_state():
    global currInd
    if currInd >= len(game_state_data):
        return jsonify({"gameStatus": "finished", "players": []})

    response = game_state_data[currInd]
    currInd += 1
    return jsonify(response)


if __name__ == "__main__":
    app.run(host='0.0.0.0', port=3001)
