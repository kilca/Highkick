using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformView : MultiplayerBehavior
{

    public float receivedDeltaTime;
    private SteamView view;

    void Awake()
    {
        view = GetComponent<SteamView>();
    }

    public override void OnReadingView(ViewMessage obs)
    {
        if (view.isMine)
        {
            return;
        }

        Debug.Log("set position");
        receivedDeltaTime = (float)obs.ReceiveValue("time");
        transform.position = (Vector3S)obs.ReceiveValue("position");
    }

    public override void OnWritingView(ViewMessage obs)
    {
        if (view.isMine)
        {
            obs.SendValue("position", new Vector3S(transform.position));
            obs.SendValue("time", Time.deltaTime);
        }
        
    }

}
