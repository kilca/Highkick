using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class MultiplayerBehavior : MonoBehaviour
{
    public abstract void OnWritingView(ViewMessage obs);

    public abstract void OnReadingView(ViewMessage obs);
}
