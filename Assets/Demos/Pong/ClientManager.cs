using System.Net;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    public UDPService UDP;
    public string ServerIP = "127.0.0.1";
    public int ServerPort = 25000;

    private float NextCoucouTimeout = -1;
    private IPEndPoint ServerEndpoint;

    void Awake()
    {
        if (Globals.IsServer)
        {
            gameObject.SetActive(false);
        }
    }

    void Start()
    {
        UDP.InitClient();

        // Configurer l'EndPoint du serveur
        ServerEndpoint = new IPEndPoint(IPAddress.Parse(ServerIP), ServerPort);

        // Abonnement pour traiter les messages reçus
        UDP.OnMessageReceived += (string message, IPEndPoint sender) => {
            Debug.Log("[CLIENT] Message received from " +
                sender.Address + ":" + sender.Port + " => " + message);
        };
    }

    void Update()
    {
        // Envoyer périodiquement un message au serveur
        if (Time.time > NextCoucouTimeout)
        {
            UDP.SendUDPMessage("coucou", ServerEndpoint); // Ajout du deuxième paramètre
            NextCoucouTimeout = Time.time + 0.5f;
        }
    }
}
