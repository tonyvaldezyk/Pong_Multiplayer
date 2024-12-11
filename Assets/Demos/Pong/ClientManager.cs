using System.Net;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    public UDPService UDP;
    public string ServerIP = "192.168.190.102";
    public int ServerPort = 25000;

    public GameObject Paddle; // Paddle contrôlé par ce client
    public int PlayerID; // Identifiant du joueur (1 pour gauche, 2 pour droite)

    private float NextPaddleUpdateTimeout = -1;
    private IPEndPoint ServerEndpoint;

    void Awake()
    {
        // Désactiver l'objet si ce n'est pas un client
        if (Globals.IsServer)
        {
            gameObject.SetActive(false);
        }
    }

    void Start()
    {
        UDP.InitClient();

        ServerEndpoint = new IPEndPoint(IPAddress.Parse(ServerIP), ServerPort);

        UDP.OnMessageReceived += (string message, IPEndPoint sender) =>
        {
            if (message.StartsWith("PADDLE_POSITION"))
            {
                // Traiter les mises à jour des paddles
                string json = message.Split('|')[1];
                ServerManager.PaddleState paddleState = JsonUtility.FromJson<ServerManager.PaddleState>(json);

                // Mettre à jour la position locale du paddle (si ce n'est pas le paddle local)
                if (paddleState.PlayerID != PlayerID && Paddle != null)
                {
                    Paddle.transform.position = paddleState.Position;
                    Debug.Log($"[CLIENT] Paddle {paddleState.PlayerID} position updated: {paddleState.Position}");
                }
            }
            else
            {
                Debug.Log("[CLIENT] Message received from " +
                          sender.Address.ToString() + ":" + sender.Port +
                          " => " + message);
            }
        };
    }

    void Update()
    {
        // Envoyer périodiquement la position locale du paddle au serveur
        if (Time.time > NextPaddleUpdateTimeout && Paddle != null)
        {
            ServerManager.PaddleState paddleState = new ServerManager.PaddleState
            {
                PlayerID = PlayerID,
                Position = Paddle.transform.position
            };

            string json = JsonUtility.ToJson(paddleState);
            UDP.SendUDPMessage($"PADDLE_POSITION|{json}", ServerEndpoint);

            Debug.Log($"[CLIENT] Paddle {PlayerID} position sent: {paddleState.Position}");
            NextPaddleUpdateTimeout = Time.time + 0.03f; // Envoyer toutes les 30ms
        }
    }
}
