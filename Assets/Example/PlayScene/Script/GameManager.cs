using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Test for checking player Ready
 * Not implemented
 */
public class GameManager : MonoBehaviour, IListener
{

    List<SteamId> playersReady = new List<SteamId>();

    public void OnNotify(Message message)
    {
        SteamIdData data = (SteamIdData)message._data;
        playersReady.Add(data.AccountId);
    }

    protected void CheckEveryoneReady()
    {
        if (isEveryoneReady())
        {

        }
    }

    protected bool isEveryoneReady()
    {
        foreach (var friend in HNetwork.currentLobby.Members)
        {
            if (!playersReady.Contains(friend.Id))
            {
                return false;
            }
        }
        return true;
    }

    // Start is called before the first frame update
    void Start()
    {
        MessageHandlerNetwork.AddListener(MessageType.GAME_READY, this);
        Message m = new Message(MessageType.GAME_READY, new SteamIdData(SteamClient.SteamId));
        playersReady.Add(SteamClient.SteamId);
        P2PSend.SendPacket(m);
        CheckEveryoneReady();
    }

}
