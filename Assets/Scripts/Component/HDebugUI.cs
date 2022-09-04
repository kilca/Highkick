using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class HDebugUI : MonoBehaviour
{
    private Text text;


    void Start()
    {
        text = GetComponent<Text>();
        HDebug.ui = this;
    }

    public void NotifyUpdate(string logs)
    {
        text.text = logs;
    }

}
