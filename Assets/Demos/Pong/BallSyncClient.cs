using System.Net;
using UnityEngine;

public class BallSyncClient : MonoBehaviour
{
    UDPService UDP; // Référence au service UDP
    public GameObject Ball; // Référence explicite à l'objet balle

    void Awake()
    {
        // Désactive ce script si la machine actuelle est un serveur
        if (Globals.IsServer)
        {
            enabled = false;
        }
    }

    void Start()
    {
        // Récupère le service UDP dans la scène
        UDP = FindFirstObjectByType<UDPService>();

        // Abonnement aux messages reçus
        UDP.OnMessageReceived += (string message, IPEndPoint sender) => {
            if (!message.StartsWith("BALL_UPDATE")) { return; } // Vérifie le type de message

            try
            {
                // Désérialise les données JSON et met à jour la position de la balle
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

    void Update()
    {
        // Rien à faire ici pour le moment, toute la logique est gérée via les messages reçus
    }
}
