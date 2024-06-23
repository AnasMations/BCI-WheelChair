using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

public class BCI : MonoBehaviour
{
    public UnityEvent OnUp; //فوق
    public UnityEvent OnDown; //تحت
    public UnityEvent OnRight; //يمين
    public UnityEvent OnLeft; //شمال
    public UnityEvent OnSelect; //اختر
    public bool enableWebsocket = false;
    private ClientWebSocket webSocket;
    private string defaultWebSocketUrl = "ws://localhost:8765";
    private string filePath;

    // Start is called before the first frame update
    async void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "websocket.txt");

        string websocketUrl = defaultWebSocketUrl;

        if (File.Exists(filePath))
        {
            websocketUrl = File.ReadAllText(filePath).Trim();
            Debug.Log("WebSocket URL loaded from file: " + websocketUrl);
        }
        else
        {
            File.WriteAllText(filePath, defaultWebSocketUrl);
            Debug.Log("WebSocket file created with default URL: " + defaultWebSocketUrl);
        }

        webSocket = new ClientWebSocket();
        Uri serverUri = new Uri(websocketUrl);
        await webSocket.ConnectAsync(serverUri, CancellationToken.None);
        Debug.Log("Connected to WebSocket server");

        if (enableWebsocket)
        {
            StartReceiving();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Debug.Log("Up");
            OnUp?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Debug.Log("Down");
            OnDown?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Debug.Log("Right");
            OnRight?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Debug.Log("Left");
            OnLeft?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Select");
            OnSelect?.Invoke();
        }
    }

    private async void StartReceiving()
    {
        byte[] buffer = new byte[1024];

        while (webSocket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
            else
            {
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Debug.Log("Received: " + message);
                HandleMessage(message);
            }
        }
    }

    private void HandleMessage(string message)
    {
        switch (message)
        {
            case "UP":
                OnUp?.Invoke();
                break;
            case "DOWN":
                OnDown?.Invoke();
                break;
            case "RIGHT":
                OnRight?.Invoke();
                break;
            case "LEFT":
                OnLeft?.Invoke();
                break;
            case "SELECT":
                OnSelect?.Invoke();
                break;
            default:
                Debug.LogWarning("Unknown command: " + message);
                break;
        }
    }
}
