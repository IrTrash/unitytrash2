using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrisonerCarrier : MonoBehaviour
{
    public prisoner currentprisoner;
    public corruptor destcorruptor;

    private void FixedUpdate()
    {

        proc();
    }


    void proc()
    {
        if(currentprisoner != null)
        {

        }
    }



    bool sendprisoner(corruptor dest)
    {
        if(dest == null || currentprisoner == null)
        {
            return false;
        }


        return true;
    }
}
