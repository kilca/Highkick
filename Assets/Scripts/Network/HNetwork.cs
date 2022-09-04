using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HNetwork{

    //============== APP INFO==================

    public static HManager Instance { get; private set; }    
    //480 => SpaceGame
    //4000 => Gmod
    public static uint appId = 480;

    const int MAX_VIEW_IDS = 1000;
    public static int instantiatedCount = 0;
    public static bool isMessageQueueRunning = true;
    public static bool loadingLevelAndPausedNetwork = false;

    //============ Lobby====================



    [SerializeField]
    public static Dictionary<SteamIdData, int> friendsViewId;
    //les joueurs connectes au lobby
    public static List<SteamId> players;

    public static List<int> allocatedId;


    #region LOBBY

    public static Lobby currentLobby;
    public static bool UserInLobby;
    public static Dictionary<SteamId, GameObject> inLobby = new Dictionary<SteamId, GameObject>();




    public static async Task<bool> CreateLobby()
    {
        try
        {
            var createLobbyOutput = await SteamMatchmaking.CreateLobbyAsync();
            if (!createLobbyOutput.HasValue)
            {
                HDebug.Log("Lobby created but not correctly instantiated.");
                return false;
            }
            currentLobby = createLobbyOutput.Value;

            currentLobby.SetPublic();
            //currentLobby.SetPrivate();
            currentLobby.SetJoinable(true);

            return true;
        }
        catch (System.Exception exception)
        {
            HDebug.Log("Failed to create multiplayer lobby : " + exception);
            return false;
        }
    }

    public static Dictionary<SteamIdData, int> GenerateFriendsId()
    {
        Dictionary<SteamIdData, int> ids = new Dictionary<SteamIdData, int>();
        if (!HNetwork.IsLobbyHost(SteamClient.SteamId))
        {
            HDebug.LogError("only the host can generate ids");

        }
        ids.Add(new SteamIdData(SteamClient.SteamId), 0);
        int i = 1;
        foreach (var friend in HNetwork.currentLobby.Members)
        {
            if (friend.Id != SteamClient.SteamId)
            {
                ids.Add(new SteamIdData(friend.Id), i);
                i++;
            }
        }
        return ids;
    }

    public static bool IsLobbyMember(SteamId steamID)
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



    public static string GetMemberName(SteamId steamID)
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

    public static bool IsLobbyHost(SteamId steamID)
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

    public static List<SteamId> getPlayers()
    {
        return new List<SteamId>(inLobby.Keys);
    }

    #endregion



    public static int getPlayerId()
    {
        return friendsViewId[new SteamIdData(SteamClient.SteamId)];
    }

    public static void Instantiate(string prefabName, Vector3S position, QuaternionS rotation, object[] data)
    {

        GameObject prefabGo;
        prefabGo = (GameObject)Resources.Load(prefabName, typeof(GameObject));

        if (prefabGo == null)
        {
            Debug.LogError("Failed to Instantiate prefab: " + prefabName + ", not in resource folder");
        }

        // a scene object instantiated with network visibility has to contain a PhotonView
        if (prefabGo.GetComponent<HView>() == null)
        {
            Debug.LogError("Failed to Instantiate prefab:" + prefabName + "must have view");
        }

        int[] viewIDs = new int[prefabGo.GetComponent<HView>().GetHViewInChildren().Length];
        for (int i = 0; i < viewIDs.Length; i++)
        {
            //Debug.Log("Instantiate prefabName: " + prefabName + " player.ID: " + player.ID);
            viewIDs[i] = AllocateViewID(friendsViewId[new SteamIdData(SteamClient.SteamId)]);
        }

        InstantiateData idata = new InstantiateData(prefabName, position, rotation, viewIDs, new SteamIdData(SteamClient.SteamId));
        Debug.Log("WE SEND INSTANTIATE");
        // Send to others, create info
        P2PSend.SendInstantiate(idata);
        DoInstantiate(idata, prefabGo);
    }

    public static GameObject DoInstantiate(InstantiateData data, GameObject prefabGo)
    {
        if (prefabGo == null)
            prefabGo = (GameObject)Resources.Load(data.prefabName, typeof(GameObject));


        int i = 0;
        foreach (HView sv in prefabGo.GetComponent<HView>().GetHViewInChildren())
        {
            sv.instantiationId = data.viewIDs[i];
            sv.ownerId = data.owner;
            i++;
        }
        GameObject g = GameObject.Instantiate(prefabGo, data.position, data.rotation);

        return g;
    }

    public static int AllocateViewID(int viewOwnerId)
    {
        int manualId = AllocateViewID(getPlayerId(), viewOwnerId);
        allocatedId.Add(manualId);
        return manualId;
    }

    public static int AllocateViewID(int playerId, int viewOwnerId)
    {

        int ownerIdOffset = (viewOwnerId * MAX_VIEW_IDS) + instantiatedCount;
        instantiatedCount++;
        if (playerId != viewOwnerId)
        {
            Debug.LogWarning("todo after");
        }
        return ownerIdOffset;


    }


    public static void LoadLevel(string levelName)
    {
        isMessageQueueRunning = false;
        loadingLevelAndPausedNetwork = true;
        SceneManager.LoadScene(levelName);
    }

    public void AddNextStage()
    {
        HDebug.Log("Change to new scene");
    }
}
