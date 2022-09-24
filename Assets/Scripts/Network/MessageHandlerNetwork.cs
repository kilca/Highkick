using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MessageHandlerNetwork
{
    //listener specific to basic message
    public static Dictionary<MessageType,List<IListener>> listeners = new Dictionary<MessageType, List<IListener>>();

    //listener specific to View message
    public static Dictionary<int,IListener> viewListeners = new Dictionary<int, IListener>();

    public static Queue<Message> _queue = new Queue<Message>();

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

    /**
     * Handle message for everything inheriting HView components
     */
    public static void HandleViewMessages(Message m)
    {
        ViewMessage vm = (ViewMessage)m._data;
        if (viewListeners.ContainsKey(vm.viewId)){
            viewListeners[((ViewMessage)m._data).viewId].OnNotify(m);
        }
        else
        {
            Debug.LogError("key : " +vm.viewId + ", not present for message : "+m.MESSAGE_TYPE);
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


        while(_queue.Count > 0)
        {
            Message m = _queue.Dequeue();
            if (m.MESSAGE_TYPE == MessageType.NETWORK_VIEW)
            {
                HandleViewMessages(m);
            }
            else
            {
                HandleBasicMessages(m);
            }
        }
    }
}
