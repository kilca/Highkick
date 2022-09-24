using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MessageType
{
    NONE,
    HANDSHAKE,
    UPDATE_PLAYERS_ID,
    GAME_START_INITIATED,
    NETWORK_VIEW,
    INSTANTIATE,
    GAME_READY//WHEN THE PLAYER HAS LOADED THE GAME
}

