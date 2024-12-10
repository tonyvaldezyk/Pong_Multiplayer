using System.Net;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    public UDPService UDP; // Service UDP pour la communication réseau
    public string ServerIP = "192.168.190.123"; // Adresse IP du serveur
    public int ServerPort = 25000; // Port du serveur

    private float NextCoucouTimeout = -1;
    private float NextPaddleUpdateTimeout = -1; // Contrôle la fréquence des mises à jour du paddle (ajouté)
    private IPEndPoint ServerEndpoint;

    public int PlayerID = 1; // ID du joueur (1 pour gauche, 2 pour droite)
    public GameObject Paddle; // Paddle contrôlé par le joueur
    public GameObject Ball; // Référence à la balle (pour mise à jour de sa position)

    void Awake()
    {
        // Désactiver ce script si ce n'est pas un client
        if (Globals.IsServer)
        {
            gameObject.SetActive(false);
        }
    }

    void Start()
    {
        UDP.InitClient();

        // Configurer l'adresse du serveur
        ServerEndpoint = new IPEndPoint(IPAddress.Parse(ServerIP), ServerPort);

        // Gérer les messages reçus du serveur
        UDP.OnMessageReceived += (string message, IPEndPoint sender) => {
            Debug.Log("[CLIENT] Message received from " +
                sender.Address.ToString() + ":" + sender.Port +
                " => " + message);

            string[] tokens = message.Split('|');
            switch (tokens[0])
            {
                case "PADDLE_POSITION": // Mise à jour des paddles
                    PaddlePositionUpdate paddleUpdate = JsonUtility.FromJson<PaddlePositionUpdate>(tokens[1]);
                    UpdatePaddlePosition(paddleUpdate.player_id, paddleUpdate.position);
                    break;

                case "BALL_UPDATE": // Mise à jour de la balle
                    BallState ballState = JsonUtility.FromJson<BallState>(tokens[1]);
                    UpdateBallPosition(ballState.Position);
                    break;

                default:
                    Debug.LogWarning("Unknown message type received: " + tokens[0]);
                    break;
            }
        };
    }

    void Update()
    {
        // Envoyer un message "coucou" pour maintenir la connexion
        if (Time.time > NextCoucouTimeout)
        {
            UDP.SendUDPMessage("coucou", ServerEndpoint);
            NextCoucouTimeout = Time.time + 0.5f;
        }

        // Envoyer la position actuelle du paddle au serveur
        if (Time.time > NextPaddleUpdateTimeout && Paddle != null)
        {
            PaddlePositionUpdate paddleUpdate = new PaddlePositionUpdate
            {
                player_id = PlayerID,
                position = Paddle.transform.position
            };

            string paddleMessage = "PADDLE_POSITION|" + JsonUtility.ToJson(paddleUpdate);
            UDP.SendUDPMessage(paddleMessage, ServerEndpoint);

            NextPaddleUpdateTimeout = Time.time + 0.03f; // Mise à jour toutes les 0.03 secondes
        }
    }

    // Mettre à jour la position d'un paddle (reçu du serveur)
    void UpdatePaddlePosition(int playerID, Vector3 position)
    {
        if (playerID != PlayerID) // Ne met pas à jour le paddle local
        {
            // Trouver et mettre à jour le paddle adverse (à implémenter selon la structure)
            Debug.Log($"Updating paddle position for player {playerID} to {position}");
        }
    }

    // Mettre à jour la position de la balle (reçue du serveur)
    void UpdateBallPosition(Vector3 position)
    {
        if (Ball != null)
        {
            Ball.transform.position = position;
        }
    }
}

// Classe pour structurer les données des paddles (ajouté)
[System.Serializable]
public class PaddlePositionUpdate
{
    public int player_id; // ID du joueur
    public Vector3 position; // Position du paddle
}

// Classe pour structurer les données de la balle (ajouté)
[System.Serializable]
public class BallState
{
    public Vector3 Position; // Position de la balle
}
