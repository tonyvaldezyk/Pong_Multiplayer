using System.Net;
using UnityEngine;

public class BallSyncClient : MonoBehaviour
{
    UDPService UDP;
    public GameObject Ball;

    void Awake()
    {
        if (Globals.IsServer)
        {
            enabled = false;
        }
    }

    void Start()
    {
        UDP = FindFirstObjectByType<UDPService>();
        UDP.OnMessageReceived += (string message, IPEndPoint sender) => {
            if (!message.StartsWith("BALL_UPDATE")) { return; }

            try
            {
                string json = message.Split('|')[1];
                BallState state = JsonUtility.FromJson<BallState>(json);
                if (Ball != null)
                {
                    Ball.transform.position = state.Position;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error processing BALL_UPDATE message: " + ex.Message);
            }
        };
    }

    void Update() { }
}
