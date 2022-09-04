using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MessageHandlerNetwork
{
    //listener specific to basic message
    public static Dictionary<MessageType,List<IListener>> listeners = new Dictionary<MessageType, List<IListener>>();

    //listener specific to View message
    public static Dictionary<int,IListener> viewListeners = new Dictionary<int, IListener>();

    public static List<Message> _queue = new List<Message>();

    public static void AddListener(MessageType mtype, IListener listener)
    {
        if (listeners.ContainsKey(mtype))
        {
            listeners[mtype].Add(listener);
        }
        else
        {
            List<IListener> l = new List<IListener>();
            l.Add(listener);
            listeners.Add(mtype,l);
        }
    }

    public static void AddViewListener(int id, IListener listener)
    {
        if (viewListeners.ContainsKey(id))
        {
            HDebug.LogError("view listener already exist");
        }
        else
        {
            viewListeners.Add(id, listener);
        }
    }

    public static void HandleViewMessages(Message m)
    {
        ViewMessage vm = (ViewMessage)m._data;
        if (viewListeners.ContainsKey(vm.viewId)){
            viewListeners[((ViewMessage)m._data).viewId].OnNotify(m);
        }
        else
        {
            Debug.LogError("key : " +vm.viewId + ", not present");
        }

    }

    public static void HandleBasicMessages(Message m)
    {
        if (!listeners.ContainsKey(m.MESSAGE_TYPE))
        {
            HDebug.LogError("key : " + m.MESSAGE_TYPE + ", not handled");
            return;
        }
        for (int i = 0; i < listeners[m.MESSAGE_TYPE].Count; i++)
        {

            listeners[m.MESSAGE_TYPE][i].OnNotify(m);
        }
    }


    public static void HandleMessages()
    {

        if (HNetwork.loadingLevelAndPausedNetwork)
        {
            return;
        }

        foreach (Message m in _queue)
        {

            if (m.MESSAGE_TYPE == MessageType.NETWORK_VIEW)
            {
                HandleViewMessages(m);
            }
            else
            {
                HandleBasicMessages(m);
            }

            
        }

        _queue.Clear();
    }
}
