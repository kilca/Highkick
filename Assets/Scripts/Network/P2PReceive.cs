using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class P2PReceive
{

    public static void ReadPacketReceived()
    {
        while (SteamNetworking.IsP2PPacketAvailable())
        {
            HDebug.Log("Packet received");

            var p2packet = SteamNetworking.ReadP2PPacket();

            if (p2packet.HasValue)
            {
                try
                {
                    Message m = ObjectSerializationExtension.Deserialize<Message>(p2packet.Value.Data);
                    HDebug.Log("Receive : " + m.MESSAGE_TYPE);
                    MessageHandlerNetwork._queue.Enqueue(m);
                }
                catch (Exception e)
                {
                    HDebug.LogError("Error in receiving packet");
                }
            }
            else
            {
                HDebug.LogError("packet has no value");
            }
        }
    }

}
