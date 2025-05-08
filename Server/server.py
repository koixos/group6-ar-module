import asyncio
import json
import sys
import websockets

clients = set()


async def send_states(data_queue):
    for data in data_queue:
        await asyncio.get_event_loop().run_in_executor(None, input)
        for ws in clients:
            await ws.send(json.dumps(data))
        print(data["gameStatus"])
        await asyncio.sleep(2)


async def echo(websocket):
    clients.add(websocket)

    try:
        async for msg in websocket:
            print(msg)
    finally:
        clients.remove(websocket)


async def main():
    with open("game_state_data.json", "r", encoding="utf-8") as f:
        data = json.load(f)

    server = await websockets.serve(echo, "0.0.0.0", 8080)
    print("WebSocket server running...")

    await send_states(data)

    await server.wait_closed()


if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        sys.exit(0)