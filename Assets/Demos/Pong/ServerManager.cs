using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    public UDPService UDP; // Service UDP pour gérer la communication réseau
    public int ListenPort = 25000; // Port sur lequel le serveur écoute

    public Dictionary<string, IPEndPoint> Clients = new Dictionary<string, IPEndPoint>();
    public Dictionary<int, Vector3> PaddlePositions = new Dictionary<int, Vector3>(); // Ajouté : Stocke les positions des paddles
    public Vector3 BallPosition = Vector3.zero; // Ajouté : Stocke la position actuelle de la balle
    private float NextBallUpdateTimeout = -1; // Ajouté : Contrôle la fréquence des mises à jour de la balle

    void Awake()
    {
        // Désactive l'objet si ce n'est pas un serveur
        if (!Globals.IsServer)
        {
            gameObject.SetActive(false);
        }
    }

    void Start()
    {
        UDP.Listen(ListenPort); // Initialise l'écoute sur le port spécifié

        // Événement déclenché lorsqu'un message est reçu
        UDP.OnMessageReceived += (string message, IPEndPoint sender) => {
            Debug.Log("[SERVER] Message received from " +
                sender.Address.ToString() + ":" + sender.Port
                + " => " + message);

            // Gérer les différents types de messages reçus
            switch (message.Split('|')[0])
            {
                case "coucou":
                    // Ajouter le client au dictionnaire
                    string addr = sender.Address.ToString() + ":" + sender.Port;
                    if (!Clients.ContainsKey(addr))
                    {
                        Clients.Add(addr, sender);
                    }
                    Debug.Log("There are " + Clients.Count + " clients present.");

                    // Envoie un message de bienvenue
                    UDP.SendUDPMessage("welcome!", sender);
                    break;

                case "PADDLE_POSITION": // Ajouté : Gestion des positions des paddles
                    string paddleJson = message.Split('|')[1];
                    PaddlePositionUpdate paddleUpdate = JsonUtility.FromJson<PaddlePositionUpdate>(paddleJson);

                    // Mettre à jour la position du paddle
                    PaddlePositions[paddleUpdate.player_id] = paddleUpdate.position;

                    // Diffuser la mise à jour à tous les clients
                    BroadcastUDPMessage(message);
                    break;

                default:
                    Debug.LogWarning("Unknown message type received: " + message);
                    break;
            }
        };
    }

    void Update()
    {
        // Ajouté : Diffuser la position de la balle à intervalles réguliers
        if (Time.time > NextBallUpdateTimeout)
        {
            BallState ballState = new BallState
            {
                Position = BallPosition // À mettre à jour selon la logique réelle du jeu
            };

            string ballMessage = "BALL_UPDATE|" + JsonUtility.ToJson(ballState);
            BroadcastUDPMessage(ballMessage);

            NextBallUpdateTimeout = Time.time + 0.03f; // Mise à jour toutes les 0.03 secondes
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

// Ajouté : Classe pour structurer les messages des paddles
[System.Serializable]
public class PaddlePositionUpdate
{
    public int player_id; // ID du joueur (1 pour gauche, 2 pour droite)
    public Vector3 position; // Position actuelle de la raquette
}
