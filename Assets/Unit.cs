using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public enum _type : int
    {
        unit = 1, building, obj
    }

    public _type type;
    public float x, y;
    public int team,hp = 10;
    public int pretx, prety;

    public enum _direction : int
    {
        left = 1, up, right, down
    }
    public _direction direction = _direction.down;
    public float speed = 2;

    public enum statetypes : int
    {
        idle = 1 , move , charge, cast, delay , stun
    }

    public statetypes state = statetypes.idle;
    public int statetime = 0;
    public bool canaction = true, canmove = true; //행동 가능,이동 가능

    public unitaction currentaction = null, pushedaction = null; //pushed가 우선.
    public List<unitaction> actionlist = new List<unitaction>();
    public const int actionmax = 16;

    public float colsize = 0.5f;

    public Unitbuilder ub;

    public unitpattern ownerup;

    // Start is called before the first frame update
    void Start()
    {
        if(type == 0)
        {
            type = _type.unit;
        }

        if(state == 0)
        {
            state = statetypes.idle;
        }

        x = gameObject.transform.position.x;
        y = gameObject.transform.position.y;
        positionupdate();

        BoxCollider2D cd = GetComponent<BoxCollider2D>();
        if(cd == null)
        {
            cd = gameObject.AddComponent<BoxCollider2D>();
        }
        cd.size = new Vector2(colsize, colsize);


        system sys = system.findsystem();
        if (sys != null)
        {
            if(sys.unitlist != null)
            {
                sys.unitlist.Add(this);
            }
        }


        if(GetComponent<unitpattern>() == null)
        {
            unitpattern up = gameObject.AddComponent<unitpattern>();
            
        }
    }

    private void OnDestroy()
    {
        system sys = system.findsystem();
        if (sys != null)
        {
            if (sys.unitlist != null)
            {
                sys.unitlist.Remove(this);
            }
        }

        if(ub != null)
        {
            ub.buildedunits.Remove(this);            
        }

        if(ownerup != null)
        {
            ownerup.myunits.Remove(this);
        }
    }


    private void FixedUpdate()
    {
        if(statetime <= 0)
        {
            state = statetypes.idle;
        }
        else
        {
            statetime--;
        }

        //행동 가능 판정
        canaction = state == statetypes.idle;
        canmove = canaction && type != _type.obj; //이동 가능이라는 것은 새로운 쪽으로 이동이 가능하냐는 뜻임. 즉 이미 이동하는 중에는 false

        switch (state)
        {
            case statetypes.move : 
                {
                    if(speed <= 0 )
                    {
                        break;
                    }

                    float deltaspeed = speed * Time.fixedDeltaTime;                   
                    float x2 = x, y2 = y;
                    switch (direction)
                    {
                        case _direction.left:
                            {
                                x2 -= deltaspeed;
                            }
                            break;

                        case _direction.right:
                            {
                                x2 += deltaspeed;
                            }
                            break;

                        case _direction.up:
                            {
                                y2 += deltaspeed;
                            }
                            break;

                        case _direction.down:
                            {
                                y2 -= deltaspeed;
                            }
                            break;

                        default:
                            state = statetypes.idle;
                            break;
                    }

                    
                    bool tilemoved = (Mathf.Abs(pretx - x2) >= 1) || (Mathf.Abs(prety - y2) >= 1);

                    if (tilemoved) //하나라ㅣ도 이동하면 멈추게 하고. 연속이동의 경우는 다른 것을이용해 구현
                    {
                        x = system.tilex(x2);
                        y = system.tiley(y2);

                        //무빙정지
                        stopmove();
                    }
                    else
                    {
                        x = x2;
                        y = y2;
                    }

                    positionupdate();
                }
                break;
        
        }


        //unitaction
        actionproc();

        Animator ani = GetComponent<Animator>();
        if (ani != null)
        {
            ani.SetInteger("state", (int)state);
            ani.SetInteger("direcint", (int)direction);
        }
    }



    public int ix => (int)x;
    public int iy => (int)y;




    public enum factortype : int
    {
        damage = 1 , heal , stun, moveposition , setposition
    }

    public void affect(factortype ftype, int[] i, float[] f, string[] s)
    {
        switch (ftype)
        {
            case factortype.damage:
                {
                    if(i == null)
                    {
                        return;
                    }

                    if (i[0] <= 0)
                    {
                        return;
                    }

                    hpupdate(hp - i[0]);
                }
                break;
        }

    }


    public void hpupdate(int dest)
    {
        hp = dest;

        if(hp <= 0)
        {
            death();
        }
    }


    public void death()
    {


        Destroy(gameObject);
    }









    public bool startmove(_direction direc , int time)
    {
        if(state != statetypes.idle) //이동 가능 상태 확인
        {
            return false;
        }

        //가려는 방향이 막혀있으면 그냥 false 리턴
        system sys = system.findsystem();
        if(sys != null)
        {

            int x2, y2;
            switch (direc)
            {
                case _direction.left:
                    {
                        x2 = system.tilex(x - 1);
                        y2 = system.tiley(y);
                    }
                    break;

                case _direction.right:
                    {
                        x2 = system.tilex(x + 1);
                        y2 = system.tiley(y);
                    }
                    break;

                case _direction.up:
                    {
                        x2 = system.tilex(x);
                        y2 = system.tiley(y + 1);
                    }
                    break;

                case _direction.down:
                    {
                        x2 = system.tilex(x);
                        y2 = system.tiley(y - 1);
                    }
                    break;
                default:
                    x2 = system.tilex(x);
                    y2 = system.tiley(y);
                    break;
            }

            if(sys.isblocked(x2,y2))
            {
                Debug.Log("갈 곳이 막힘 : ( " + x2 + " , " + y2 + " ) direc : " + direc + " 시작위치 : " + x + " , " + y);
                return false;
            }

        }


        state = statetypes.move;
        statetime = time;
        direction = direc;


        pretx = system.tilex(x);
        prety = system.tilex(y);
        return true;
    }

    public void stopmove()
    {
        if(state == statetypes.move)
        {
            state = statetypes.idle;
            statetime = 0;
        }
    }

    public void positionupdate()
    {
        gameObject.transform.position = new Vector3(x, y, transform.position.z);
    }


    private void actionproc()
    {
        if(!canaction)
        {
            return;
        }

        if(currentaction == null)
        {
            if(pushedaction != null)
            {
                currentaction = pushedaction;
                pushedaction = null;
            }
            else if(actionlist.Count > 0)
            {
                currentaction = actionlist[actionlist.Count - 1];
                actionlist.Remove(actionlist[actionlist.Count - 1]); 
            }
        }
        else
        {
            if(currentaction.execute())
            {
                Destroy(currentaction);
                currentaction = null;
            }
            
        }
    }


    public bool addaction(unitaction.typelist desttype, int[] i, float[] f)
    {
        if(actionlist.Count >= actionmax)
        {
            return false;
        }

        if(currentaction == null)
        {
            currentaction = gameObject.AddComponent<unitaction>();
            currentaction.set(desttype, i, f);
        }
        else
        {
            unitaction buf = gameObject.AddComponent<unitaction>();
            buf.set(desttype, i, f);
            actionlist.Add(buf);
        }

        return true;
    }

    public bool addaction(unitaction.typelist desttype, int[] i, float[] f, string[] s)
    {
        if (actionlist.Count >= actionmax)
        {
            return false;
        }

        if (currentaction == null)
        {
            currentaction = gameObject.AddComponent<unitaction>();
            currentaction.set(desttype, i, f, s);
        }
        else
        {
            unitaction buf = gameObject.AddComponent<unitaction>();
            buf.set(desttype, i, f, s);
            actionlist.Add(buf);
        }

        return true;
    }
    
    public bool pushaction(unitaction.typelist desttype, int[] i, float[] f, string[] s)
    {
        //currentaction을 pushed로 보내고 push할 action을 current로 보내 먼저 처리. 그 후  currentaction이 끝날떄 pushed가 있으면 그게 pushed로, 아니면 actionlist를 참조

        if(pushedaction != null)
        {
            return false;
        }


        pushedaction = currentaction;

        currentaction = gameObject.AddComponent<unitaction>();
        currentaction.set(desttype, i, f, s);

        return true;
    }

    public weapon findweapon(int destindex)
    {
        if(destindex < 0)
        {
            return null;
        }


        weapon[] wps = gameObject.GetComponents<weapon>();
        if(wps == null)
        {
            return null;
        }
        else if(wps.Length <= destindex)
        {
            return null;
        }

        return wps[destindex];
    }

    public weapon findweapon(string destname)
    {
        if(destname == null)
        {
            return null;
        }

        foreach(weapon w in gameObject.GetComponents<weapon>())
        {
            if(w == null)
            {
                continue;
            }

            if(w.weaponname == destname)
            {
                return w;
            }
        }

        return null;
    }



}
