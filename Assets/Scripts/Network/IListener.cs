﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public interface IListener
    {
        void OnNotify(Message message);
    }