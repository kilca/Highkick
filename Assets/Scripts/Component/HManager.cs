using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Steamworks;
using UnityEngine.SceneManagement;

public class HManager : MonoBehaviour, IListener
{


    public static HManager Instance { get; private set; }

    public UnityEvent OnSteamFailed;

    public float networkTick = 0.1f;


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
        InvokeRepeating("UpdateNetworkIn", 0f, networkTick);
        InvokeRepeating("UpdateNetworkOut", 0f, networkTick);

    }

    private void UpdateNetworkIn()
    {
        if (SteamClient.IsValid)
        {
            SteamClient.RunCallbacks();
            if (!HNetwork.isNetworkRunningIn)
            {
                return;
            }
            P2PReceive.ReadPacketReceived();
            MessageHandlerNetwork.HandleMessages();
        }
    }


    public void UpdateNetworkOut()
    {

        if (!HNetwork.isNetworkRunningOut)
        {
            return;
        }

        P2PSend.SendMessages();
        foreach (HView v in HNetwork.hViews)
        {
            v.SendMessages();
        }

    }

    // called second
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HNetwork.isNetworkRunningIn = true;
        HNetwork.isNetworkRunningOut = true;
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
