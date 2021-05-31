using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class system : MonoBehaviour
{
    public List<Unit> unitlist = new List<Unit>();

    private void Start()
    {       
        astar = new Astar(left, bottom, top, right);
    }



    //gridsize = 1;
    public int left = -20, right = 20, top = 20, bottom = -20;
    public List<Block> blocklist = new List<Block>();
    public Astar astar;

    public static system findsystem()
    {
        GameObject sysobj = GameObject.Find("system");
        if(sysobj != null)
        {
            return sysobj.GetComponent<system>();
        }

        return null;
    }


    public static int tilex(float x)
    {
        int leftx;
        if(x >= 0)
        {
            leftx = (int)x;            
        }
        else
        {            
            leftx = (int)x - 1;
        }

        if(Mathf.Abs(x - leftx) >= 0.5f)
        {
            return leftx + 1;
        }
        else
        {
            return leftx;
        }

    }

    public static int tiley(float y)
    {
        int downy;
        if (y >= 0)
        {
            downy = (int)y;
        }
        else
        {
            downy = (int)y - 1;
        }

        if (Mathf.Abs(y - downy) >= 0.5f)
        {
            return downy + 1;
        }
        else
        {
            return downy;
        }
    }


    public bool isblocked(int x ,int y)
    {
        if(x < left || x > right || y > top || y < bottom)
        {
            return true;
        }

        //블록 체크. 
        return blockcheck(x, y);
    }

    public bool blockcheck(int x, int y)
    {
        foreach(Block b in blocklist)
        {
            if(b == null)
            {
                continue;
            }

            if(b.x == x && b.y == y)
            {
                return true;
            }
        }

        return false;
    }

    public Unit._direction[] findway(int sx, int sy,int dx, int dy)
    {
        astar.clearblock();
        foreach(Block b in blocklist)
        {
            astar.setblock(b.x, b.y, true);
        }

        node[] nodes = astar.getway(sx, sy, dx, dy);
        if(nodes == null)
        {
            return null;
        }


        node cur = new node(false, sx, sy, null); //astar.getway는 시작점의 노드는 주지 않기 때문에 이렇게 해 줘야한다..
        List<Unit._direction> r = new List<Unit._direction>();
        foreach(node n in nodes)
        {
            if(cur == n)
            {
                continue;
            }

            Unit._direction direc = 0;
            if(cur.gx != n.gx)
            {
                direc = cur.gx > n.gx ? Unit._direction.left : Unit._direction.right;
            }
            else if(cur.gy != n.gy)
            {
                direc = cur.gy > n.gy ? Unit._direction.down : Unit._direction.up;
            }

            cur = n;
            r.Add(direc);
        }

        foreach(Unit._direction d in r)
        {
            Debug.Log(d);
        }

        return r.ToArray();
    }

    public static bool isin(float x, float y, float x1, float y1, float x2, float y2) => (x - x1) * (x - x2) <= 0 && (y - y1) * (y - y2) <= 0;

    public Unit[] findunit(float x1, float y1, float x2, float y2 , Unit[] exlist)
    {
        GameObject[] uobjs = GameObject.FindGameObjectsWithTag("Unit");

        if(uobjs == null)
        {
            return null;
        }

        List<Unit> r = new List<Unit>() , exceptlist = new List<Unit>(exlist);               
        foreach(GameObject uobj in uobjs)
        {
            Unit u = uobj.GetComponent<Unit>();
            if(u == null)
            {
                continue;
            }
            
            if(exceptlist.Contains(u))
            {
                continue;
            }


            if (isin(u.x, u.y, x1, y1, x2, y2))
            {
                r.Add(u);
            }
        }

        return r.ToArray();
    }

    public Unit[] findunit(float x, float y, float range, Unit[] exlist) => findunit(x - range, y - range, x + range, y + range, exlist);



    public bool moveunit(Unit u , float distance, float direction) 
    {
        if(u == null || distance <= 0 )
        {
            return false;
        }

        float xd = Mathf.Cos(direction) * distance, yd = Mathf.Sin(direction) * distance;
        u.x += xd;
        u.y += yd;
        u.positionupdate();

        return true;
    }

    

    public const int unitplacelimit = 2;
    public const float unitplacepushdistance = 0.1f;
    public void unitplaceproc() //겹치기 처리. 일정이상 겹치면 다른곳으로 보내는 식으로 해야할듯..근데 생각해보면 골치아픈 점이 많음 좀더 생각을 해야..
    {
        //
        foreach (Unit u in unitlist)
        {
            if (u == null)
            {
                continue;
            }

            if(u.state != Unit.statetypes.move)
            {
                if (countunit(u.ix, u.iy) >= unitplacelimit)
                {
                    if(countunit(u.x,u.y) >= unitplacelimit)
                    {
                        moveunit(u, unitplacepushdistance, UnityEngine.Random.Range(0, 360) * Mathf.Deg2Rad);
                        Debug.Log("unit replaced");
                    }                                        
                }
            }
            
        }
    }

    public int countunit(int x, int y)
    {
        if(!isin(x,y,left,bottom,right,top))
        {
            return 0;
        }

        int r = 0;
        foreach(Unit u in unitlist)
        {
            if(tilex(u.x) == x && tiley(u.y) == y)
            {
                r++;
            }             
        }

        return r;
    }
    public int countunit(float x, float y)
    {
        if (!isin(x, y, left, bottom, right, top))
        {
            return 0;
        }

        int r = 0;
        foreach (Unit u in unitlist)
        {
            if (Mathf.Approximately(u.x, x) && Mathf.Approximately(u.y, y))
            {
                r++;
            }
        }

        return r;
    }



    private void FixedUpdate()
    {
        unitplaceproc();
    }
}

