using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class UDPSender : MonoBehaviour
{
    public int DestinationPort = 25000;
    public string DestinationIP = "192.168.190.102";

    public UDPReceiver.UDPMessageReceive OnMessageReceived;

    UdpClient udp;
    IPEndPoint localEP;

    public void SendUDPMessage(string message) {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);
        SendUDPBytes(bytes);
    }

    public void Close() {
        CloseUDP();
    }

    private void SendUDPBytes(byte[] bytes) {
        if (udp == null) {
            udp = new UdpClient();
            localEP = new IPEndPoint(IPAddress.Any, 0);
            udp.Client.Bind(localEP);
        }

        try {
            IPEndPoint destEP = new IPEndPoint(IPAddress.Parse(DestinationIP), DestinationPort);
            udp.Send(bytes, bytes.Length, destEP);
            
        } catch (SocketException e)
        {
            Debug.LogWarning(e.Message);
        }
    }

    void OnDisable() {
        CloseUDP();
    }

    private void CloseUDP() {
        if (udp != null) {
            udp.Close();
            udp = null;
        }
    }

    void Update() {
        ReceiveUDP();
    }


    private void ReceiveUDP() {
        if (udp == null) { return; }

        while (udp.Available > 0)
		{
            IPEndPoint sourceEP = new IPEndPoint(IPAddress.Any, 0);
			byte[] data = udp.Receive(ref sourceEP);

			try
			{
				ParseString(data, sourceEP);
			}
			catch (System.Exception ex)
			{
				Debug.LogWarning("Error receiving UDP message: " + ex.Message);
			}
		}
    }

    private void ParseString(byte[] bytes, IPEndPoint sender) {
        string message = System.Text.Encoding.UTF8.GetString(bytes);

        OnMessageReceived(message, sender);
    }

}
