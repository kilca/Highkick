using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public static class P2PSend
{

    public static List<Message> messages = new List<Message>();

    public static void SendInstantiate(InstantiateData data)
    {
        Message m = new Message(MessageType.INSTANTIATE,data);
        SendPacket(m);
    }

    private static void PrepareAndSend(Message mes)
    {
        byte[] packet = ObjectSerializationExtension.SerializeToByteArray(mes);
        foreach (SteamId id in SteamLobbyManager.Instance.getPlayers())
        {


            if(id == SteamClient.SteamId)
            {
                continue;
            }
            Debug.Log("sending : " + mes.MESSAGE_TYPE + ", to :" + id);

            SteamDebug.Log("Sending packet to " + id);
            bool sent = SteamNetworking.SendP2PPacket(id, packet);
            if (!sent)
            {
                SteamDebug.LogError("ERROR in sending package");
            }
        }
    }

    public static void SendPacket(Message m)
    {
        messages.Add(m);
        if (!SteamManager.isMessageQueueRunning)
        {
            return;
        }

        foreach(Message mes in messages)
        {
            Debug.Log("we depile");
            /*
            foreach (Message mes in messages)
            {
                PrepareAndSend(mes);
            }
            messages.Clear();
            */
            PrepareAndSend(mes);
        }
        messages.Clear();

    }

}
