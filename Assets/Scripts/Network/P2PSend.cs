using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public static class P2PSend
{

    public static Queue<Message> messages = new Queue<Message>();

    public static void SendInstantiate(InstantiateData data)
    {
        Message m = new Message(MessageType.INSTANTIATE,data);
        SendPacket(m);
    }

    private static void PrepareAndSend(Message mes)
    {
        byte[] packet = ObjectSerializationExtension.SerializeToByteArray(mes);
        foreach (SteamId id in HNetwork.getPlayers())
        {


            if(id == SteamClient.SteamId)
            {
                continue;
            }
            Debug.Log("sending : " + mes.MESSAGE_TYPE + ", to :" + id);

            Debug.Log("Sending packet to " + id);
            bool sent = SteamNetworking.SendP2PPacket(id, packet);
            if (!sent)
            {
                HDebug.LogError("ERROR in sending package");
            }
        }
    }

    public static void SendMessages()
    {
        while (messages.Count > 0)
        {
            Message m = messages.Dequeue();
            PrepareAndSend(m);
        }

    }
    public static void ClearQueue()
    {
        messages.Clear();
    }

    public static void SendPacket(Message m)
    {
        messages.Enqueue(m);
    }

}
