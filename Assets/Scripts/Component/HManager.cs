using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Steamworks;
using UnityEngine.SceneManagement;

public class HManager : MonoBehaviour, IListener
{


    public static HManager Instance { get; private set; }

    public UnityEvent OnSteamFailed;

    private void Awake()
    {

        HNetwork.allocatedId = new List<int>();

        DontDestroyOnLoad(this);

        try
        {
            Steamworks.SteamClient.Init(HNetwork.appId, true);
            HDebug.Log("Steam is up and running!");

            MessageHandlerNetwork.AddListener(MessageType.INSTANTIATE, this);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
            HDebug.Log(e.Message);
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
        HNetwork.isMessageQueueRunning = true;
        HNetwork.loadingLevelAndPausedNetwork = false;
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
            HNetwork.DoInstantiate((InstantiateData)message._data, null);
        }
    }
}
