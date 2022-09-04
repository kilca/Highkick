using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HView : MonoBehaviour, IListener
{

    public SteamIdData ownerId=0;
    public int instantiationId;
    [SerializeField]
    public List<MultiplayerBehavior> observedComponent;
    [HideInInspector]
    public ViewMessage vMessages;
    public Message message;

    public bool isMine
    {
        get => ( ownerId.Equals(SteamClient.SteamId));
    } 

    public void OnNotify(Message message)
    {
        Debug.Log("Notify HView");
        ViewMessage vm = (ViewMessage) message._data;
        int i = 0;
        foreach(MultiplayerBehavior o in observedComponent)
        {
            vm.reference = i;
            o.OnReadingView(vm);
            i++;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("CREATION OF STAMVIEW WITH ID :" + instantiationId);
        MessageHandlerNetwork.AddViewListener(instantiationId, this);
    }

    public HView[] GetHViewInChildren()
    {
        return transform.GetComponentsInChildren<HView>();
    }

    void SendMessages()
    {
        vMessages = new ViewMessage(instantiationId);
        foreach (MultiplayerBehavior o in observedComponent)
        {
            vMessages.PrepareNextMessage();
            o.OnWritingView(vMessages);
        }

        message = new Message(MessageType.NETWORK_VIEW, vMessages);
        P2PSend.SendPacket(message);
        HDebug.Log("HView send packet");
    }

    // Update is called once per frame
    void Update()
    {
        SendMessages();
    }
}
