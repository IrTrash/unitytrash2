using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class node
{
    public bool blocked = false;
    public int gx, gy, g = 0, h = 0; //g는 시작점에서 이 노드까지 걸리는 거리, h는 목적지까지의 예상 거리
    public node parent;

    public node(bool _blocked, int _gx, int _gy, node _parent)
    {
        blocked = _blocked;
        gx = _gx;
        gy = _gy;
        parent = _parent;
    }

    public int f => g + h; 
}


public class Astar 
{
    List<node> nodes = new List<node>();

    public const int dist = 10, disdia = 14;

    public int left, right, top, bottom;
    public Astar(int l, int b, int r, int t)
    {
        
        if(l > r)
        {
            r = l + r; //둘이 합치고
            l = r - l; //둘의 합에서 자신을 빼면 상대방이 되고
            r = r - l; //상대방을 뺀 것은 자신이 되므로 서로 뒤바뀐다
        }

        if(b > t)
        {
            t = b + t;
            b = t - b;
            t = t - b;
        }

        left = l;
        right = r;
        top = t;
        bottom = b;

        if(left == right && top == bottom) //칸이 한개인곳에서 굳이 길을 찾을 필요가있는것인가?? 한줄(일방통행)인 곳에서도 딱히 의미가 없어보여서 &&가 아닌 ||로할려다 그냥 &&로해둠
        {
            return;
        }

        for (int n = left; n <= right; n++)
        {
            for(int m = bottom; m <= top; m++)
            {
                nodes.Add(new node(false, n, m, null));
            }
        }
    }

    
    public node getnode(int x, int y)
    {
        if(!isin(x,y))
        {
            return null;
        }

        foreach(node n in nodes)
        {
            if(n.gx == x && n.gy == y)
            {
                return n;
            }
        }

        return null;
    }

    public bool setblock(int x,int y, bool b)
    {
        node n = getnode(x, y);
        if(n == null)
        {
            return false;
        }

        n.blocked = b;
        return true;
    }


    public bool isin(int x, int y) => x >= left && x <= right && y >= bottom && y <= top; 

    public void clear()
    {
        foreach(node n in nodes)
        {
            n.parent = null;
            n.g = 0;
            n.h = 0;
        }
    }

    public void clearblock()
    {
        foreach( node n in nodes)
        {
            n.blocked = false;
        }
    }

    public node[] getway(int sx, int sy, int dx, int dy)
    {
        
        node start = getnode(sx, sy), end = getnode(dx, dy);
        if(start == null || end == null)
        {
            return null;
        }
        else if(start.blocked || end.blocked)
        {
            return null;
        }
        List<node> opened, closed , result;
        node cur = null;
        opened = new List<node>();
        closed = new List<node>();
        result = new List<node>();
        
        opened.Add(start);
        while(true)
        {
            if(opened.Count <= 0)
            {
                return null;
            }

            cur = null;
            foreach(node n in opened)
            {
                if(cur == null)
                {
                    cur = n;
                    continue;
                }

                if(cur != n)
                {
                    if(cur.f > n.f)
                    {
                        cur = n;
                    }
                    else if(cur.f == n.f)
                    {
                        if(cur.h > n.h)
                        {
                            cur = n;
                        }
                    }

                }

            }

            opened.Remove(cur);
            closed.Add(cur);

            if(cur == end)
            {
                break;
            }

            //4ways
            List<Vector2Int> checklist = new List<Vector2Int>() , checklist2 = new List<Vector2Int>();
            checklist2.Add(new Vector2Int(cur.gx - 1, cur.gy));
            checklist2.Add(new Vector2Int(cur.gx + 1, cur.gy));
            checklist2.Add(new Vector2Int(cur.gx, cur.gy - 1));
            checklist2.Add(new Vector2Int(cur.gx, cur.gy + 1));

            //무조건 이 순서대로 먼저 하기 때문에 맞는길이 여러개여도 한가지만 정해져서 좀 골치아프긴함. 랜덤으로 섞는 방법 생각해야
            //checklist를 섞음
            while(checklist2.Count > 0)
            {
                Vector2Int buf = checklist2[UnityEngine.Random.Range(0,checklist2.Count)];
                checklist.Add(buf);
                checklist2.Remove(buf);
            }
           

            foreach(Vector2Int c in checklist)
            {
                node cnode = getnode(c.x, c.y);
                if(cnode == null)
                {
                    continue;
                }

                if(closed.Contains(cnode) || cnode.blocked)
                {
                    continue;
                }

                int movecost = cur.g + dist; //4방향이므로 대각이동을 배제해서 무조건 타일크기(dist)씩 이동하게됨
                if(!opened.Contains(cnode) || movecost < cnode.g)
                {
                    cnode.g = movecost;
                    cnode.h = (Mathf.Abs(end.gx - cnode.gx) + Mathf.Abs(end.gy - cnode.gy)) * dist; //dist(타일당의 크기)를 곱해야하는거같은데...원래했던건 안했네
                    cnode.parent = cur;
                    opened.Add(cnode);
                }
            }
        }
        


        node nbuf = end;
        while(nbuf.parent != null)
        {
            result.Add(nbuf);
            nbuf = nbuf.parent;
        }
        result.Reverse();
        clear();

        return result.ToArray();
    }
}
