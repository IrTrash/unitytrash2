using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class projectiledata : MonoBehaviour
{
    //projectile object를 생성하기위한 컴포넌트

    public GameObject original; //사용할 투사체 오브젝트


    public float speed = 1f, hitsize = 0.5f, vardirec = 0 ,vardistance = 0; //vardirec방향으로 vardistance만큼 위치가 변경되서 생성
    public int life = 40, bounddelay = 1 , boundlimit = 1, boundlife = 1;

    public List<effect> elist = new List<effect>(); 

    public GameObject create(GameObject ownerunit , float dx, float dy)
    {
        if (ownerunit == null)
        {
            return null;
        }

        Unit u = ownerunit.GetComponent<Unit>();
        if (u == null)
        {
            return null;
        }

        float direc;
        if(Mathf.Approximately(dx,u.x))
        {
            direc = dy > u.y ? 90 * Mathf.Deg2Rad : 270 * Mathf.Deg2Rad;
        }
        else
        {
            direc = Mathf.Atan2(dy - u.y, dx - u.x);
            
        }


        return create(ownerunit, direc);
    }



    public GameObject create(GameObject ownerunit, float direction)
    {
        if(ownerunit == null)
        {
            return null;
        }

        Unit u = ownerunit.GetComponent<Unit>();
        if(u == null)
        {
            return null;
        }

        return create(u.team, u.x, u.y, direction);
    }

    public GameObject create(int team, float x, float y, float direction)
    {
        if(original == null)
        {
            return null;
        }

        if(vardistance != 0)
        {
            x = x + vardistance * Mathf.Cos(direction);
            y = y + vardistance * Mathf.Sin(direction);
        }
        
        GameObject buf = Instantiate(original, new Vector3(x, y, 0), Quaternion.identity); //z는 기본적으로 다 0으로
        projectile proj = buf.GetComponent<projectile>();
        if(proj == null)
        {
            proj = buf.AddComponent<projectile>();
        }
        proj.life = life;
        proj.bounddelay = bounddelay;
        proj.boundlimit = boundlimit;
        proj.boundlife = boundlife;
        proj.speed = speed;
        proj.hitsize = hitsize;
        proj.direction = direction;
        proj.team = team;

        foreach(effect e in proj.elist)
        {
            Destroy(e);
        }
        proj.elist.Clear();

        if(elist.Count > 0)
        {
            foreach(effect e in elist)
            {
                if(e == null)
                {
                    continue;
                }
                effect ebuf = buf.AddComponent<effect>();
                ebuf.copy(e);
                proj.elist.Add(ebuf);
            }
        }
        

        return buf;
    }
}
