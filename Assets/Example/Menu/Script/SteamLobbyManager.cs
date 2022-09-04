using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SteamLobbyManager : MonoBehaviour, IListener
{
    public UnityEvent OnLobbyCreated;
    public UnityEvent OnLobbyJoined;
    public UnityEvent OnLobbyLeave;


    public GameObject InLobbyFriend;
    public Transform content;
    public Transform playButton;

    public Text playerInformation;


    private void Start()
    {
        DontDestroyOnLoad(this);

        SteamMatchmaking.OnLobbyCreated += OnLobbyCreatedCallBack;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnChatMessage += OnChatMessage;
        SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnected;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberDisconnected;
        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequest;
        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
        SteamNetworking.OnP2PSessionRequest += RequestingP2PSession;

        MessageHandlerNetwork.AddListener(MessageType.GAME_START_INITIATED, this);
        MessageHandlerNetwork.AddListener(MessageType.UPDATE_PLAYERS_ID, this);
        MessageHandlerNetwork.AddListener(MessageType.HANDSHAKE, this);

    }

    #region CALLBACK

    void OnLobbyInvite(Friend friend, Lobby lobby)
    {
        HDebug.Log($"{friend.Name} invited you to his lobby.");
    }


    private void OnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId id)
    {

    }

    private async void OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        HDebug.Log($"{friend.Name} joined the lobby");
        GameObject obj = Instantiate(InLobbyFriend, content);
        obj.GetComponentInChildren<Text>().text = friend.Name;
        obj.GetComponentInChildren<RawImage>().texture = await SteamFriendsManager.GetTextureFromSteamIdAsync(friend.Id);
        HNetwork.inLobby.Add(friend.Id, obj);


        SendHello();
    }

    void OnLobbyMemberDisconnected(Lobby lobby, Friend friend)
    {
        HDebug.Log($"{friend.Name} left the lobby");
        HDebug.Log($"New lobby owner is {HNetwork.currentLobby.Owner}");
        if (HNetwork.inLobby.ContainsKey(friend.Id))
        {
            Destroy(HNetwork.inLobby[friend.Id]);
            HNetwork.inLobby.Remove(friend.Id);
        }
    }



    void OnChatMessage(Lobby lobby, Friend friend, string message)
    {
        HDebug.Log($"incoming chat message from {friend.Name} : {message}");
    }

    async void OnGameLobbyJoinRequest(Lobby joinedLobby, SteamId id)
    {
        RoomEnter joinedLobbySuccess = await joinedLobby.Join();
        if (joinedLobbySuccess != RoomEnter.Success)
        {
            HDebug.Log("failed to join lobby : " + joinedLobbySuccess);
        }
        else
        {
            HNetwork.currentLobby = joinedLobby;
        }
    }

    void OnLobbyCreatedCallBack(Result result, Lobby lobby)
    {
        if (result != Result.OK)
        {
            HDebug.Log("lobby creation result not ok : " + result);
        }
        else
        {
            OnLobbyCreated.Invoke();
            HDebug.Log("lobby creation result ok");
        }
    }


    async void OnLobbyEntered(Lobby lobby)
    {
        HDebug.Log("Client joined the lobby");
        HNetwork.UserInLobby = true;
        foreach (var user in HNetwork.inLobby.Values)
        {
            Destroy(user);
        }
        HNetwork.inLobby.Clear();

        GameObject obj = Instantiate(InLobbyFriend, content);
        obj.GetComponentInChildren<Text>().text = SteamClient.Name;
        obj.GetComponentInChildren<RawImage>().texture = await SteamFriendsManager.GetTextureFromSteamIdAsync(SteamClient.SteamId);

        HNetwork.inLobby.Add(SteamClient.SteamId, obj);

        foreach (var friend in HNetwork.currentLobby.Members)
        {
            if (friend.Id != SteamClient.SteamId)
            {
                GameObject obj2 = Instantiate(InLobbyFriend, content);
                obj2.GetComponentInChildren<Text>().text = friend.Name;
                obj2.GetComponentInChildren<RawImage>().texture = await SteamFriendsManager.GetTextureFromSteamIdAsync(friend.Id);

                HNetwork.inLobby.Add(friend.Id, obj2);
            }
        }
        OnLobbyJoined.Invoke();
        SendHello();
    }


    public void LeaveLobby()
    {
        try
        {
            HNetwork.UserInLobby = false;
            HNetwork.currentLobby.Leave();
            OnLobbyLeave.Invoke();
            foreach (var user in HNetwork.inLobby.Values)
            {
                Destroy(user);
            }
            HNetwork.inLobby.Clear();
            List<SteamId> keyList = new List<SteamId>(HNetwork.inLobby.Keys);
        }
        catch
        {

        }
    }

    public void RequestingP2PSession(SteamId requesterID)
    {
        HDebug.Log("incoming p2p request from: " + requesterID);

        if (HNetwork.IsLobbyHost(requesterID))
        {
            HDebug.Log("accepting request from server host: " + HNetwork.GetMemberName(requesterID));
            SteamNetworking.AcceptP2PSessionWithUser(requesterID);
        }
        else if (HNetwork.IsLobbyMember(requesterID))
        {
            HDebug.Log("accepting request from lobby member: " + HNetwork.GetMemberName(requesterID));
            SteamNetworking.AcceptP2PSessionWithUser(requesterID);
        }
    }

    public void QuitGame()
    {
        Steamworks.SteamClient.Shutdown();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #elif UNITY_WEBPLAYER
             Application.OpenURL("http://google.com");
        #else
            Application.Quit();
        #endif
    }


    #endregion

    //================= Not callback

    public void ShowPlayButton(GameObject button)
    {
        //if is host
        if (HNetwork.IsLobbyHost(SteamClient.SteamId))
        {
            button.SetActive(true);
        }
    }


    public void SendHello()
    {
        HDebug.Log("Envoi de hello");

        Message m = new Message(MessageType.HANDSHAKE, "hello");
        P2PSend.SendPacket(m);
    }

    public void SendId()
    {
        HDebug.Log("Envoi de id");
        if (HNetwork.IsLobbyHost(SteamClient.SteamId))
        {
            HNetwork.friendsViewId = HNetwork.GenerateFriendsId();
            Message m = new Message(MessageType.UPDATE_PLAYERS_ID, HNetwork.friendsViewId);
            P2PSend.SendPacket(m);
            SetPlayerInformationText();
        }
        else
        {
            HDebug.LogWarning("is not host");
        }
    }

    public void StartGame()
    {
        HDebug.Log("Envoi de Start game");
        Message m = new Message(MessageType.GAME_START_INITIATED, "");
        P2PSend.SendPacket(m);
        HNetwork.LoadLevel("PlayScene");
        //TEST EXEMPLE

    }

    public void SetPlayerInformationText()
    {
        if (HNetwork.friendsViewId.ContainsKey(new SteamIdData(SteamClient.SteamId)))
        {
            playerInformation.text = SteamClient.SteamId + "/" + HNetwork.friendsViewId[new SteamIdData(SteamClient.SteamId)];
        }
    }

    public void OnNotify(Message message)
    {
        switch (message.MESSAGE_TYPE)
        {
            case MessageType.UPDATE_PLAYERS_ID:
                HNetwork.friendsViewId = (Dictionary<SteamIdData, int>)message._data;
                SetPlayerInformationText();
                break;
            case MessageType.GAME_START_INITIATED:
                HNetwork.LoadLevel("PlayScene");
                HDebug.Log("CHANGE SCENE");
                break;

        }
    }
}
