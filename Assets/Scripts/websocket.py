import asyncio
import websockets
import time

async def send_sequence(websocket, path):
    sequence = ["UP", "RIGHT", "UP", "SELECT", "LEFT", "LEFT", "UP", "RIGHT", "UP"]
    await asyncio.sleep(5)
    for command in sequence:
        await websocket.send(command)
        print(f"Sent: {command}")
        await asyncio.sleep(11)

async def main():
    async with websockets.serve(send_sequence, "localhost", 8765):
        print("WebSocket server started on ws://localhost:8765")
        await asyncio.Future()  # run forever

if __name__ == "__main__":
    asyncio.run(main())
