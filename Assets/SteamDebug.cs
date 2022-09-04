using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class SteamDebug
{


    private const int MAX_LINE = 10;
    private const int TRUNCATE_NUMBER = 48;


    //TODO remplacer par log custom
    private static List<string> logs = new List<string>();
    private static StringBuilder sb = new StringBuilder();


    public static SteamDebugUI ui;

    //========================

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }

    private static void SendLogs()
    {
        int beginIndex = logs.Count < MAX_LINE ? 0 : logs.Count - MAX_LINE;
        if (sb == null)
            sb = new StringBuilder();
        sb.Clear();
        for (int i = beginIndex; i < logs.Count; i++)
        {
            sb.AppendLine(logs[i]);
        }
        if (ui != null)
            ui.NotifyUpdate(sb.ToString());
    }

    public static void Log(object s)
    {
        if (logs == null)
        {
            logs = new List<string>();
        }
        logs.Add(Truncate(s.ToString(), TRUNCATE_NUMBER));;
        SendLogs();
    }

    public static void LogError(object s)
    {
        logs.Add(Truncate("ERROR | " +s.ToString(), TRUNCATE_NUMBER));
        SendLogs();
    }

    public static void LogWarning(object s)
    {
        logs.Add(Truncate("WARN | " + s.ToString(), TRUNCATE_NUMBER));
        SendLogs();
    }



}
