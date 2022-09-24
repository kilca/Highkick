using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HTransformView : MultiplayerBehavior
{

    private HView view;

    private Vector3 realPosition = Vector3.zero;
    private Quaternion realRoation = Quaternion.identity;

    void Awake()
    {
        view = GetComponent<HView>();
    }

    private void Update()
    {
        
        if (view.isMine)
        {

        }
        else
        {
            //for more complete sync : + realVelocity*time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, realPosition, 0.1f);
            transform.rotation = Quaternion.Lerp(transform.rotation, realRoation, 0.1f);
        }
        
    }


    public override void OnReadingView(ViewMessage obs)
    {
        if (view.isMine)
        {
            return;
        }

        Debug.Log("set position");
        realRoation = (QuaternionS)obs.ReceiveValue("rotation");
        realPosition = (Vector3S)obs.ReceiveValue("position");
    }

    public override void OnWritingView(ViewMessage obs)
    {
        if (view.isMine)
        {
            obs.SendValue("position", new Vector3S(transform.position));
            obs.SendValue("rotation", new QuaternionS(transform.rotation));
        }
        
    }

}
