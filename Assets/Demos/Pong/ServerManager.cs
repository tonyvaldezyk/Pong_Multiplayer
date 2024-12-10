using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace PongGame
{
    [System.Serializable]
    public class BallState
    {
        public Vector3 Position; // Position actuelle de la balle
    }
}

public class ServerManager : MonoBehaviour
{
    public UDPService UDP;
    public int ListenPort = 25000;
    public Dictionary<string, IPEndPoint> Clients = new Dictionary<string, IPEndPoint>();
    public Vector3 BallPosition = Vector3.zero;
    private float NextBallUpdateTimeout = -1;

    void Awake()
    {
        if (!Globals.IsServer)
        {
            gameObject.SetActive(false);
        }
    }

    void Start()
    {
        UDP.Listen(ListenPort);
        UDP.OnMessageReceived += (string message, IPEndPoint sender) => {
            Debug.Log("[SERVER] Message received from " + sender.Address + ":" + sender.Port + " => " + message);
            // Gestion des messages
        };
    }

    void Update()
    {
        if (Time.time > NextBallUpdateTimeout)
        {
            PongGame.BallState ballState = new PongGame.BallState { Position = BallPosition };
            string ballMessage = "BALL_UPDATE|" + JsonUtility.ToJson(ballState);
            BroadcastUDPMessage(ballMessage);
            NextBallUpdateTimeout = Time.time + 0.03f;
        }
    }

    public void BroadcastUDPMessage(string message)
    {
        foreach (KeyValuePair<string, IPEndPoint> client in Clients)
        {
            UDP.SendUDPMessage(message, client.Value);
        }
    }
}
