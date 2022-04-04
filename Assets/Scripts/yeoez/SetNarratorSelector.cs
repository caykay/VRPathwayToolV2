﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SetNarratorSelector : PokeSelector
{
    private void OnTriggerEnter(Collider collision)
    {
        FindPokingPlayerHead(collision);
        pokingPlayerHead.GetComponent<PhotonView>().RPC("SetIsNarrator", RpcTarget.AllBuffered, true);
    }
}
