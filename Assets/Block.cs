using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    //벽돌이에요~~
    public int x => (int)gameObject.transform.position.x;
    public int y => (int)gameObject.transform.position.y;

    private void Start()
    {
        system sys = system.findsystem();
        if(sys != null)
        {

            //막힘처리
            if(!sys.blocklist.Contains(this))
            {
                sys.blocklist.Add(this);
            }
        }
    }


    private void OnDestroy()
    {
        system sys = system.findsystem();
        if (sys != null)
        {
            sys.blocklist.Remove(this);
        }
    }
}
