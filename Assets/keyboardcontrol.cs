using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class keyboardcontrol : MonoBehaviour
{
    public Unit u;


    private void Start()
    {
        if(u == null)
        {
            u = gameObject.GetComponent<Unit>();
        }



    }


    void Update()
    {
        if(Input.GetMouseButtonDown(0)) //left
        {
            Vector3 mpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            u.addaction(unitaction.typelist.useweapon, new int[] { 0 }, new float[] { mpos.x, mpos.y });
        }

        if(Input.GetMouseButtonDown(1)) // right, 2는 middle(아마 휠?)
        {
            Vector3 mpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            u.addaction(unitaction.typelist.movedest, new int[] { system.tilex(mpos.x), system.tiley(mpos.y) }, null, null);
        }


        Unit._direction direc = 0;
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if(u != null)
            {
                direc = Unit._direction.left;                
            }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (u != null)
            {
                direc = Unit._direction.right;
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (u != null)
            {
                direc = Unit._direction.up;
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (u != null)
            {
                direc = Unit._direction.down;
            }
        }

        if(direc != 0)
        {
            u.addaction(unitaction.typelist.movedirec, new int[] { (int)direc, 60 }, null);
        }
        
    }
}
