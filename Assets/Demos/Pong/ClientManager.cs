using System.Net;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    public UDPService UDP;
    public string ServerIP = "192.168.190.102"; // IP du serveur
    public int ServerPort = 25000;

    public GameObject Paddle; // Référence au paddle local
    public int PlayerID; // Identifiant du joueur (1 pour gauche, 2 pour droite)

    private IPEndPoint ServerEndpoint;

    void Awake()
    {
        // Désactiver cet objet si ce n'est pas un client
        if (Globals.IsServer)
        {
            gameObject.SetActive(false);
        }
    }

    void Start()
    {
        UDP.InitClient();

        // Configurer l'endpoint du serveur
        ServerEndpoint = new IPEndPoint(IPAddress.Parse(ServerIP), ServerPort);

        // Gérer les messages reçus du serveur
        UDP.OnMessageReceived += (string message, IPEndPoint sender) => {
            Debug.Log("[CLIENT] Message reçu de " +
                      sender.Address.ToString() + ":" + sender.Port +
                      " => " + message);
        };
    }

    void Update()
    {
        // Vérifie que le paddle est défini et que l'UDP est initialisé
        if (Paddle != null && UDP != null && ServerEndpoint != null)
        {
            // Crée un état du paddle
            PaddleState paddleState = new PaddleState
            {
                PlayerID = PlayerID,
                Position = Paddle.transform.position
            };

            // Convertit l'état en JSON
            string json = JsonUtility.ToJson(paddleState);

            // Envoie la position du paddle au serveur
            UDP.SendUDPMessage($"PADDLE_POSITION|{json}", ServerEndpoint);

            Debug.Log($"[CLIENT] Position du Paddle {PlayerID} envoyée : {json}");
        }
    }
}

[System.Serializable]
public class PaddleState
{
    public int PlayerID; // ID du joueur (1 pour gauche, 2 pour droite)
    public Vector3 Position; // Position actuelle du paddle
}
