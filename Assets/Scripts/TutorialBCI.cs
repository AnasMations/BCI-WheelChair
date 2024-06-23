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

[Serializable]
public class InputSequence
{
    public string command;
    public UnityEvent onStart;
    public UnityEvent onEnd;
}

public class TutorialBCI : MonoBehaviour
{
    public Movement movement;
    public AudioSource audioSource;
    public InputSequence[] inputSequences;
    public bool enableWebsocket = false;
    private ClientWebSocket webSocket;
    private int currentSequenceIndex = 0;
    private bool isHandlingMessage = false;
    private string defaultWebSocketUrl = "ws://localhost:8765";
    private string filePath;

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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            HandleMessage("UP");
            Debug.Log("Up");
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            HandleMessage("DOWN");
            Debug.Log("Down");
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            HandleMessage("RIGHT");
            Debug.Log("Right");
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            HandleMessage("LEFT");
            Debug.Log("Left");
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleMessage("SELECT");
            Debug.Log("Select");
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
        if (!isHandlingMessage && message == inputSequences[currentSequenceIndex].command)
        {
            StartCoroutine(HandleMessageCoroutine(message));
        }
        else
        {
            Debug.LogWarning("Unknown command or already handling a message: " + message);
        }
    }

    private IEnumerator HandleMessageCoroutine(string message)
    {
        audioSource?.Play();
        int waitTime = 10;
        isHandlingMessage = true;

        switch (message)
        {
            case "UP":
                movement.MoveForward();
                break;
            case "DOWN":
                movement.StopMovement();
                break;
            case "RIGHT":
                movement.RotateRight();
                break;
            case "LEFT":
                movement.RotateLeft();
                break;
            case "SELECT":
                waitTime = 1;
                Debug.Log("Select");
                break;
        }

        inputSequences[currentSequenceIndex].onStart?.Invoke();
        yield return new WaitForSeconds(waitTime);
        inputSequences[currentSequenceIndex].onEnd?.Invoke();

        currentSequenceIndex++;
        if (currentSequenceIndex >= inputSequences.Length)
        {
            currentSequenceIndex = 0;
        }
        waitTime = 10;

        isHandlingMessage = false;
    }
}
