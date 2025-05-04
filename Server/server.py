import asyncio
import websockets

async def echo(websocket):
    async for msg in websocket:
        print("Incoming msg: ", msg)
        await websocket.send("Answer: " + msg)

async def main():
    async with websockets.serve(echo, "0.0.0.0", 8080) as server:
        print("WebSocket server running...")
        await asyncio.Future()

if __name__ == "__main__":
    asyncio.run(main())