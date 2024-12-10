using UnityEngine;

public class PaddleSyncServer : MonoBehaviour
{
    ServerManager ServerMan;
    public GameObject PaddleLeft; // Paddle contrôlé par le joueur de gauche
    public GameObject PaddleRight; // Paddle contrôlé par le joueur de droite
    private float NextUpdateTimeout = -1;

    void Awake()
    {
        if (!Globals.IsServer)
        {
            enabled = false; // Désactive ce script si ce n'est pas un serveur
        }
    }

    void Start()
    {
        ServerMan = FindObjectOfType<ServerManager>();

        if (ServerMan == null)
        {
            Debug.LogError("[PaddleSyncServer] ServerManager non trouvé !");
            enabled = false;
        }
    }

    void Update()
    {
        if (Time.time > NextUpdateTimeout)
        {
            // Mettre à jour les paddles
            if (PaddleLeft != null)
            {
                BroadcastPaddlePosition(1, PaddleLeft.transform.position);
            }

            if (PaddleRight != null)
            {
                BroadcastPaddlePosition(2, PaddleRight.transform.position);
            }

            NextUpdateTimeout = Time.time + 0.03f; // Envoyer toutes les 30ms
        }
    }

    private void BroadcastPaddlePosition(int playerID, Vector3 position)
    {
        PaddleState state = new PaddleState
        {
            PlayerID = playerID,
            Position = position
        };

        string json = JsonUtility.ToJson(state);
        string message = $"PADDLE_UPDATE|{json}";

        ServerMan.BroadcastUDPMessage(message);

        Debug.Log($"[PaddleSyncServer] Position Paddle {playerID} diffusée : {position}");
    }
}
