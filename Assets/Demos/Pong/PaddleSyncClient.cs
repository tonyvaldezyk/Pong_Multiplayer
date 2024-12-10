using System.Net;
using UnityEngine;

public class PaddleSyncClient : MonoBehaviour
{
    UDPService UDP;
    public GameObject PaddleLeft; // Paddle contrôlé par le joueur de gauche
    public GameObject PaddleRight; // Paddle contrôlé par le joueur de droite

    void Awake()
    {
        if (Globals.IsServer)
        {
            enabled = false; // Désactive ce script si ce n'est pas un client
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UDP = FindFirstObjectByType<UDPService>();

        UDP.OnMessageReceived += (string message, IPEndPoint sender) =>
        {
            if (!message.StartsWith("PADDLE_UPDATE")) { return; }

            string[] tokens = message.Split('|');
            string json = tokens[1];

            PaddleState state = JsonUtility.FromJson<PaddleState>(json);

            // Met à jour la position en fonction de l'ID du paddle
            if (state.PlayerID == 1 && PaddleLeft != null)
            {
                PaddleLeft.transform.position = state.Position;
                Debug.Log($"[PaddleSyncClient] Paddle gauche mis à jour : {state.Position}");
            }
            else if (state.PlayerID == 2 && PaddleRight != null)
            {
                PaddleRight.transform.position = state.Position;
                Debug.Log($"[PaddleSyncClient] Paddle droit mis à jour : {state.Position}");
            }
        };
    }

    // Update is empty, as this script only reacts to server updates
    void Update()
    {

    }
}
