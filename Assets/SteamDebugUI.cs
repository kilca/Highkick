using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class SteamDebugUI : MonoBehaviour
{
    private Text text;


    void Start()
    {
        text = GetComponent<Text>();
        SteamDebug.ui = this;
    }

    public void NotifyUpdate(string logs)
    {
        text.text = logs;
    }

}
