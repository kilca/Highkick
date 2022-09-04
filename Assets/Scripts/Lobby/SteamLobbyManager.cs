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
    public static Lobby currentLobby;
    public static bool UserInLobby;
    public UnityEvent OnLobbyCreated;
    public UnityEvent OnLobbyJoined;
    public UnityEvent OnLobbyLeave;


    public GameObject InLobbyFriend;
    public Transform content;

    public Dictionary<SteamId, GameObject> inLobby = new Dictionary<SteamId, GameObject>();


    public Transform playButton;

    [SerializeField]
    public Dictionary<SteamIdData, int> friendsViewId;

    public Text playerInformation;

    public static SteamLobbyManager Instance { get; private set; }

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

    }

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
        SteamDebug.Log($"{friend.Name} invited you to his lobby.");
    }


    private void OnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId id)
    {

    }

    private async void OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        SteamDebug.Log($"{friend.Name} joined the lobby");
        GameObject obj = Instantiate(InLobbyFriend, content);
        obj.GetComponentInChildren<Text>().text = friend.Name;
        obj.GetComponentInChildren<RawImage>().texture = await SteamFriendsManager.GetTextureFromSteamIdAsync(friend.Id);
        inLobby.Add(friend.Id, obj);


        SendHello();
    }

    void OnLobbyMemberDisconnected(Lobby lobby, Friend friend)
    {
        SteamDebug.Log($"{friend.Name} left the lobby");
        SteamDebug.Log($"New lobby owner is {currentLobby.Owner}");
        if (inLobby.ContainsKey(friend.Id))
        {
            Destroy(inLobby[friend.Id]);
            inLobby.Remove(friend.Id);
        }
    }



    void OnChatMessage(Lobby lobby, Friend friend, string message)
    {
        SteamDebug.Log($"incoming chat message from {friend.Name} : {message}");
    }

    async void OnGameLobbyJoinRequest(Lobby joinedLobby, SteamId id)
    {
        RoomEnter joinedLobbySuccess = await joinedLobby.Join();
        if (joinedLobbySuccess != RoomEnter.Success)
        {
            SteamDebug.Log("failed to join lobby : " + joinedLobbySuccess);
        }
        else
        {
            currentLobby = joinedLobby;
        }
    }

    void OnLobbyCreatedCallBack(Result result, Lobby lobby)
    {
        if (result != Result.OK)
        {
            SteamDebug.Log("lobby creation result not ok : " + result);
        }
        else
        {
            OnLobbyCreated.Invoke();
            SteamDebug.Log("lobby creation result ok");
        }
    }


    async void OnLobbyEntered(Lobby lobby)
    {
        SteamDebug.Log("Client joined the lobby");
        UserInLobby = true;
        foreach (var user in inLobby.Values)
        {
            Destroy(user);
        }
        inLobby.Clear();

        GameObject obj = Instantiate(InLobbyFriend, content);
        obj.GetComponentInChildren<Text>().text = SteamClient.Name;
        obj.GetComponentInChildren<RawImage>().texture = await SteamFriendsManager.GetTextureFromSteamIdAsync(SteamClient.SteamId);

        inLobby.Add(SteamClient.SteamId, obj);

        foreach (var friend in currentLobby.Members)
        {
            if (friend.Id != SteamClient.SteamId)
            {
                GameObject obj2 = Instantiate(InLobbyFriend, content);
                obj2.GetComponentInChildren<Text>().text = friend.Name;
                obj2.GetComponentInChildren<RawImage>().texture = await SteamFriendsManager.GetTextureFromSteamIdAsync(friend.Id);

                inLobby.Add(friend.Id, obj2);
            }
        }
        OnLobbyJoined.Invoke();
        SendHello();
    }


    public async void CreateLobbyAsync()
    {
        bool result = await CreateLobby();
        if (!result)
        {
            //Invoke a error message.
        }
    }

    public static async Task<bool> CreateLobby()
    {
        try
        {
            var createLobbyOutput = await SteamMatchmaking.CreateLobbyAsync();
            if (!createLobbyOutput.HasValue)
            {
                SteamDebug.Log("Lobby created but not correctly instantiated.");
                return false;
            }
            currentLobby = createLobbyOutput.Value;

            currentLobby.SetPublic();
            //currentLobby.SetPrivate();
            currentLobby.SetJoinable(true);

            return true;
        }
        catch(System.Exception exception)
        {
            SteamDebug.Log("Failed to create multiplayer lobby : " + exception);
            return false;
        }
    }

    Dictionary<SteamIdData, int> GenerateFriendsId()
    {
        Dictionary<SteamIdData, int> ids = new Dictionary<SteamIdData, int>();
        if (!IsLobbyHost(SteamClient.SteamId))
        {
            SteamDebug.LogError("only the host can generate ids");

        }
        ids.Add(new SteamIdData(SteamClient.SteamId),0);
        int i = 1;
        foreach (var friend in currentLobby.Members)
        {
            if (friend.Id != SteamClient.SteamId)
            {
                ids.Add(new SteamIdData(friend.Id),i);
                i++;
            }
        }
        return ids;
    }


    public void LeaveLobby()
    {
        try
        {
            UserInLobby = false;
            currentLobby.Leave();
            OnLobbyLeave.Invoke();
            foreach (var user in inLobby.Values)
            {
                Destroy(user);
            }
            inLobby.Clear();
            List<SteamId> keyList = new List<SteamId>(inLobby.Keys);
        }
        catch
        {

        }
    }

    public void RequestingP2PSession(SteamId requesterID)
    {
        SteamDebug.Log("incoming p2p request from: " + requesterID);

        if (IsLobbyHost(requesterID))
        {
            SteamDebug.Log("accepting request from server host: " + GetMemberName(requesterID));
            SteamNetworking.AcceptP2PSessionWithUser(requesterID);
        }
        else if (IsLobbyMember(requesterID))
        {
            SteamDebug.Log("accepting request from lobby member: " + GetMemberName(requesterID));
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
        if (IsLobbyHost(SteamClient.SteamId))
        {
            button.SetActive(true);
        }
    }

    public bool IsLobbyMember(SteamId steamID)
    {
        IEnumerable<Friend> e = currentLobby.Members;
        IEnumerator f = e.GetEnumerator();

        while (f.MoveNext())
        {
            Friend friend = (Friend)f.Current;

            if (friend.Id.Value == steamID.Value)
            {
                return true;
            }
        }

        return false;
    }

    public string GetMemberName(SteamId steamID)
    {
        IEnumerable<Friend> e = currentLobby.Members;
        IEnumerator f = e.GetEnumerator();

        while (f.MoveNext())
        {
            Friend friend = (Friend)f.Current;

            if (friend.Id.Value == steamID.Value)
            {
                return friend.Name;
            }
        }

        return "non member";
    }

    public bool IsLobbyHost(SteamId steamID)
    {
        if (currentLobby.Owner.Id.Value == steamID.Value)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public List<SteamId> getPlayers()
    {
        return new List<SteamId>(inLobby.Keys);
    }


    public void SendHello()
    {
        SteamDebug.Log("Envoi de hello");

        Message m = new Message(MessageType.HANDSHAKE, "hello");
        P2PSend.SendPacket(m);
    }

    public void SendId()
    {
        SteamDebug.Log("Envoi de id");
        if (IsLobbyHost(SteamClient.SteamId))
        {
            friendsViewId = GenerateFriendsId();
            Message m = new Message(MessageType.UPDATE_PLAYERS_ID, friendsViewId);
            P2PSend.SendPacket(m);
            SetPlayerInformationText();
        }
        else
        {
            SteamDebug.LogWarning("is not host");
        }
    }

    public void StartGame()
    {
        SteamDebug.Log("Envoi de Start game");
        Message m = new Message(MessageType.GAME_START_INITIATED, "");
        P2PSend.SendPacket(m);
        SteamManager.LoadLevel("PlayScene");
        //TEST EXEMPLE

    }

    public void SetPlayerInformationText()
    {
        if (friendsViewId.ContainsKey(new SteamIdData(SteamClient.SteamId)))
        {
            playerInformation.text = SteamClient.SteamId + "/" + friendsViewId[new SteamIdData(SteamClient.SteamId)];
        }
    }

    public void OnNotify(Message message)
    {
        switch (message.MESSAGE_TYPE)
        {
            case MessageType.UPDATE_PLAYERS_ID:
                friendsViewId = (Dictionary<SteamIdData, int>)message._data;
                SetPlayerInformationText();
                break;
            case MessageType.GAME_START_INITIATED:
                SteamManager.LoadLevel("PlayScene");
                SteamDebug.Log("CHANGE SCENE");
                break;

        }
    }
}
