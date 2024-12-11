using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    public UDPService UDP;
    public int ListenPort = 25000;

    public Dictionary<string, IPEndPoint> Clients = new Dictionary<string, IPEndPoint>(); 
    public Dictionary<int, Vector3> PaddlePositions = new Dictionary<int, Vector3>(); // Stocker les positions des paddles

    void Awake() {
        // Désactiver l'objet si ce n'est pas un serveur
        if (!Globals.IsServer) {
            gameObject.SetActive(false);
        }
    }

    void Start()
    {
        UDP.Listen(ListenPort);

        UDP.OnMessageReceived +=  
            (string message, IPEndPoint sender) => {
                Debug.Log("[SERVER] Message received from " + 
                    sender.Address.ToString() + ":" + sender.Port 
                    + " => " + message);

                if (message.StartsWith("coucou"))
                {
                    // Ajouter le client à mon dictionnaire
                    string addr = sender.Address.ToString() + ":" + sender.Port;
                    if (!Clients.ContainsKey(addr)) {
                        Clients.Add(addr, sender);
                    }
                    Debug.Log("There are " + Clients.Count + " clients present.");

                    UDP.SendUDPMessage("welcome!", sender);
                }
                else if (message.StartsWith("PADDLE_POSITION")) 
                {
                    // Traiter la position du paddle
                    string json = message.Split('|')[1];
                    PaddleState paddleState = JsonUtility.FromJson<PaddleState>(json);

                    // Mettre à jour la position du paddle côté serveur
                    PaddlePositions[paddleState.PlayerID] = paddleState.Position;

                    // Diffuser la position mise à jour à tous les clients
                    BroadcastUDPMessage(message);

                    Debug.Log($"[SERVER] Paddle {paddleState.PlayerID} position updated: {paddleState.Position}");
                }
            };
    }

    public void BroadcastUDPMessage(string message) {
        foreach (KeyValuePair<string, IPEndPoint> client in Clients) {
            UDP.SendUDPMessage(message, client.Value);
        }
    }

    [System.Serializable]
    public class PaddleState
    {
        public int PlayerID; // ID du joueur (1 pour gauche, 2 pour droite)
        public Vector3 Position; // Position actuelle du paddle
    }
}
