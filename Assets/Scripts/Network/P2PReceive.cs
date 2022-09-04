using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P2PReceive : MonoBehaviour
{

    public static SteamManager Instance { get; private set; }

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }


    public void FixedUpdate()
    {
        while (SteamNetworking.IsP2PPacketAvailable())
        {
            SteamDebug.Log("Packet received");

            var p2packet = SteamNetworking.ReadP2PPacket();

            if (p2packet.HasValue)
            {
                try
                {
                    Message m = ObjectSerializationExtension.Deserialize<Message>(p2packet.Value.Data);
                    SteamDebug.Log("Receive : " + m.MESSAGE_TYPE);
                    MessageHandlerNetwork._queue.Add(m);
                }
                catch (Exception e)
                {
                    SteamDebug.LogError("Error in receiving packet");
                }
            }
            else
            {
                SteamDebug.LogError("packet has no value");
            }
        }
    }
}
