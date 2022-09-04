using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class FriendObject : MonoBehaviour
{
    public SteamId steamid;

    public async void Invite()
    {
        if (HNetwork.UserInLobby)
        {
            HNetwork.currentLobby.InviteFriend(steamid);
            Debug.Log("Invited " + steamid);
        }
        else
        {
            bool result = await HNetwork.CreateLobby();
            if (result)
            {
                HNetwork.currentLobby.InviteFriend(steamid);
                Debug.Log("Invited " + steamid + " Created a new lobby");
            }
        }
    }

}
