using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Steamworks;
using UnityEngine.SceneManagement;

public class SteamManager : MonoBehaviour, IListener
{


    public static SteamManager Instance { get; private set; }

    public uint appId;
    public UnityEvent OnSteamFailed;


    //les joueurs connectes au lobby
    public List<SteamId> players;

    public static List<int> allocatedId;

    const int MAX_VIEW_IDS = 1000;
    public static int instantiatedCount = 0;
    public static bool isMessageQueueRunning = true;
    public static bool loadingLevelAndPausedNetwork = false;

    private void Awake()
    {

        allocatedId = new List<int>();

        DontDestroyOnLoad(this);

        try
        {
            Steamworks.SteamClient.Init(appId, true);
            SteamDebug.Log("Steam is up and running!");

            MessageHandlerNetwork.AddListener(MessageType.INSTANTIATE, this);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
            SteamDebug.Log(e.Message);
            OnSteamFailed.Invoke();
        }


        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

    }

    // called second
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isMessageQueueRunning = true;
        loadingLevelAndPausedNetwork = false;
    }

    public static int getPlayerId()
    {
        return SteamLobbyManager.Instance.friendsViewId[ new SteamIdData(SteamClient.SteamId)];
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
        if (prefabGo.GetComponent<SteamView>() == null)
        {
            Debug.LogError("Failed to Instantiate prefab:" + prefabName + "must have view");
        }

        int[] viewIDs = new int[prefabGo.GetComponent<SteamView>().GetSteamViewInChildren().Length];
        for (int i = 0; i < viewIDs.Length; i++)
        {
            //Debug.Log("Instantiate prefabName: " + prefabName + " player.ID: " + player.ID);
            viewIDs[i] = AllocateViewID(SteamLobbyManager.Instance.friendsViewId[new SteamIdData(SteamClient.SteamId)]);
        }

        InstantiateData idata = new InstantiateData(prefabName, position, rotation, viewIDs, new SteamIdData(SteamClient.SteamId));
        Debug.Log("WE SEND INSTANTIATE");
        // Send to others, create info
        P2PSend.SendInstantiate(idata);
        DoInstantiate(idata, prefabGo);
    }

    private static GameObject DoInstantiate(InstantiateData data, GameObject prefabGo)
    {
        if (prefabGo == null)
            prefabGo = (GameObject)Resources.Load(data.prefabName, typeof(GameObject));

        Debug.Log("WE RECEIVE INSTANTIATE");

        int i = 0;
        foreach (SteamView sv in prefabGo.GetComponent<SteamView>().GetSteamViewInChildren())
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

    /*
    protected internal void SetLevelInPropsIfSynced(object levelId)
    {
        if (!PhotonNetwork.automaticallySyncScene || !PhotonNetwork.isMasterClient || PhotonNetwork.room == null)
        {
            return;
        }
        if (levelId == null)
        {
            Debug.LogError("Parameter levelId can't be null!");
            return;
        }

        // check if "current level" is already set in props
        if (PhotonNetwork.room.customProperties.ContainsKey(NetworkingPeer.CurrentSceneProperty))
        {
            object levelIdInProps = PhotonNetwork.room.customProperties[NetworkingPeer.CurrentSceneProperty];
            if (levelIdInProps is int && Application.loadedLevel == (int)levelIdInProps)
            {
                return;
            }
            if (levelIdInProps is string && Application.loadedLevelName.Equals((string)levelIdInProps))
            {
                return;
            }
        }

        // current level is not yet in props, so this client has to set it
        Hashtable setScene = new Hashtable();
        if (levelId is int) setScene[NetworkingPeer.CurrentSceneProperty] = (int)levelId;
        else if (levelId is string) setScene[NetworkingPeer.CurrentSceneProperty] = (string)levelId;
        else Debug.LogError("Parameter levelId must be int or string!");

        PhotonNetwork.room.SetCustomProperties(setScene);
        this.SendOutgoingCommands();    // send immediately! because: in most cases the client will begin to load and not send for a while
    }
    */


    public static void LoadLevel(string levelName)
    {
        isMessageQueueRunning = false;
        loadingLevelAndPausedNetwork = true;
        SceneManager.LoadScene(levelName);
    }

    public void AddNextStage()
    {
       SteamDebug.Log("Change to new scene");
    }

    private void LateUpdate()
    {
        if (SteamClient.IsValid)
        {
            SteamClient.RunCallbacks();
            MessageHandlerNetwork.HandleMessages();
        }
    }


    private void OnApplicationQuit()
    {
        try
        {
            Steamworks.SteamClient.Shutdown();

        }
        catch
        {

        }
    }

    public void OnNotify(Message message)
    {
        if (message.MESSAGE_TYPE == MessageType.INSTANTIATE)
        {
            Debug.Log("asking for instantiation");
            DoInstantiate((InstantiateData)message._data, null);
        }
    }
}
