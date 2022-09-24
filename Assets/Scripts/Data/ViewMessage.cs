using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ViewMessage 
{
    //peut etre une queue de queue
    public List<Dictionary<string,object>> datas;
    public int reference = -1;

    public int viewId;

    public void PrepareNextMessage()
    {
        reference++;
        datas.Add( new Dictionary<string, object>());
    }

    public void SendValue(string key, object val)
    {
        datas[reference].Add(key,val);
    }

    public object ReceiveValue(string s)
    {
        try
        {
            return datas[reference][s];
        }catch(Exception e)
        {
            Debug.LogError("View Message, data error for : " + s);
            return null;
        }    
    }

    public Dictionary<string, object> ReceiveNext()
    {
        return datas[reference];
    }

    public ViewMessage(int viewId)
    {
        this.viewId = viewId;
        datas = new List<Dictionary<string, object>>();
        reference = -1;
    }
}
