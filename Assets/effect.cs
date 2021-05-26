using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class effect : MonoBehaviour
{
    public enum typelist : int
    {
        damage = 1,
    }
    public typelist type;

    public int[] i;
    public float[] f;   




    public void work(Unit dest)
    {
        if (dest == null)
        {
            return;
        }


        switch (type)
        {
            case typelist.damage:
                {
                    dest.affect(Unit.factortype.damage, i, null, null);
                }
                break;
        }

    }

    public void copy(effect dest)
    {
        if(dest == null)
        {
            return;
        }

        type = dest.type;
        if(dest.i != null)
        {
            i = (int[])dest.i.Clone();
        }
        if(dest.f != null)
        {
            f = (float[])dest.f.Clone();
        }
    }
}
