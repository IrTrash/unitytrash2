using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class projectile : MonoBehaviour
{
    public float direction,speed; //rad
    Vector2 movement;

    public GameObject owner;
    public int team, bounddelay = 1, boundlimit = 1, boundlife = 1, life = 40;
    public float hitsize = 0.5f;
    public List<effect> elist = new List<effect>();

    Rigidbody2D rb;

    
    public class boundpair
    {
        public int delay, count;

        public boundpair(int _delay, int _count)
        {
            delay = _delay;
            count = _count;
        }
    }

    public Dictionary<int, boundpair> boundlist = new Dictionary<int, boundpair>();  //키 : 부딪힌 오브젝트의 인스턴스ID


    private void Start()
    {   
        updatedirection();
    }


    private void FixedUpdate()
    {
        if(life <= 0 || boundlife <= 0)
        {
            Destroy(gameObject);
            return;
        }
        life--;

        GameObject[] objlist = boundcheck();
        if(objlist != null)
        {            
            foreach(GameObject obj in objlist)
            {
                Debug.Log(gameObject.name + " hitted " + obj.name);
                Unit u = obj.GetComponent<Unit>();
                if (u == null)
                {
                    continue;
                }

                if(u.team == team)
                {
                    continue; //우호적인 투사체 같은건 다른 컴포넌트로 할거같음
                }

                if (boundlist.ContainsKey(obj.GetInstanceID()))
                {
                    boundpair b = boundlist[obj.GetInstanceID()];
                    if(b.count >= boundlimit)
                    {
                        continue;
                    }
                    else
                    {
                        b.count++;
                    }
                }
                else
                {
                    boundlist.Add(obj.GetInstanceID(), new boundpair(bounddelay, 1)); //기준을 unit이 아닌 gameobject로 한다면 하나의 unit을 공유하는 여러 gameobject를 하면 약간 다관절 같은것도 가능할려나 근데 너무 큰 꿈이니 패스
                }



                //충돌 처리 시작
                foreach (effect e in elist)
                {
                    e.work(u);
                }

                if (--boundlife <= 0)
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }

        List<int> removekeylist = new List<int>();
        foreach(int k in boundlist.Keys)
        {
            boundpair b = boundlist[k];
            if(b == null)
            {
                removekeylist.Add(k);
                continue;
            }

            if(b.delay > 0)
            {
                b.delay--;
            }            
        }

        foreach(int k in removekeylist)
        {
            boundlist.Remove(k);
        }


        //방향에 의한 스프라이트 회전


        //이동
        if(speed > 0)
        {
            rb.MovePosition(rb.position +  movement * speed * Time.fixedDeltaTime);
        }
    }




    public GameObject[] boundcheck()
    {
        List<Collider2D> clist = new List<Collider2D>(Physics2D.OverlapCircleAll(new Vector2(x, y), hitsize));

        if(clist.Count <= 0)
        {
            return null;
        }

        Debug.Log("asdf");

        List<GameObject> r = new List<GameObject>();
        foreach(Collider2D c in clist)
        {
            if(c.gameObject == gameObject || c.gameObject == owner)
            {
                continue;
            }

            Debug.Log("qwer");
            r.Add(c.gameObject);
        }

        return r.ToArray();
    }


    public float x => gameObject.transform.position.x;
    public float y => gameObject.transform.position.y;


    public void updatedirection()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.rotation = direction * Mathf.Rad2Deg - 90;
        movement = new Vector2(Mathf.Cos(direction), Mathf.Sin(direction));
    }


    public void setdirection(float dx, float dy)
    {
        
    }
}
