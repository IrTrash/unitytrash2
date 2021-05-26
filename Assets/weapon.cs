using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weapon : MonoBehaviour
{
    public string weaponname = "noname";

    public GameObject owner; //사용자. null이면 start에서 자기자신 오브젝트가됨
    private Unit ownerunit;

    public int charge,casting,delay,cooldown; //(프레임단위) 충전, 시전동작시간, 후딜, 쿨타임
    public float destx, desty; //목적지

    public int range = 2; //실제 사정거리가 아닌 명시적인 사정거리임. 이걸 바탕으로 유닛이 공격을 함
    public int priority = 1; //unitpattern이 적을 공격할때 선택하는 우선순위.. 무조건 높은게 걸리는게아닌 높은게 더 골라질 확률이 높음.그리고 반드시 1 이상이어야함. 산출에 문제가생기기때문에
    public enum statelist : int
    {
        available=1, charge, cooldown
    }
    public statelist state = statelist.available;
    public int statetime = 0;

    //총알 정보
    //투사체정보(게임오브젝트),  조정값(투사체속도, 피격범위 등)
    public List<projectiledata> projdatalist = new List<projectiledata>();


    private void Start()
    {
        if(owner == null)
        {
            owner = gameObject;
        }

    }


    private void FixedUpdate()
    {        
        if(ownerunit == null)
        {
            ownerunit = gameObject.GetComponent<Unit>();
        }


        if(statetime > 0)
        {
            statetime--;
        }

        switch (state)
        {
            case statelist.charge:
                {
                    if(statetime <= 0)
                    {
                        if(ownerunit != null)
                        {
                            if(ownerunit.state != Unit.statetypes.charge) //유닛이 차지동작중이 아니면 캔슬을 했다는거니까 걍 끝냄
                            {
                                state = statelist.available;
                                statetime = 0;
                                //쿨다운 돌리는건 좀 그런거같으니 걍 캔슬로만
                                break;
                            }
                        }

                        //발사!
                        foreach(projectiledata p in projdatalist)
                        {
                            if(p == null)
                            {
                                continue;                                
                            }
                            p.create(owner, destx, desty);
                        }

                        //쿨다운상태 전환
                        statetime = cooldown;
                        state = statelist.cooldown;

                        //유닛은 공격중 동작으로 + 날린 방향으로 유닛 방향 조정(4방향)
                        if(ownerunit != null)
                        {
                            ownerunit.state = Unit.statetypes.cast;
                            ownerunit.statetime = casting;
                            ownerunit.direction = getdirec(ownerunit.x, ownerunit.y, destx, desty);
                        }
                    }
                }
                break;

            case statelist.cooldown:
                {
                    if(statetime <= 0)
                    {
                        //사용가능으로 복귀
                        state = statelist.available;
                        statetime = 0;
                    }
                }
                break;
        }

    }

    public bool available()
    {
        return state == statelist.available;
    }

    public bool activate(float dx, float dy)        
    {
        if(!available())
        {
            return false;
        }

        destx = dx;
        desty = dy;
        state = statelist.charge;
        statetime = charge;

        if(ownerunit != null)
        {
            ownerunit.state = Unit.statetypes.charge;
            ownerunit.statetime = charge;
        }
        return true;
    }


    public static Unit._direction getdirec(float sx, float sy, float dx, float dy)
    {
        if(Mathf.Approximately(sx,dx) )
        {
            if(Mathf.Approximately(sy, dy))
            {
                return 0;
            }


            return sy > dy ? Unit._direction.down : Unit._direction.up;
        }
        else
        {
            float inclination = (dy - sy) / (dx - sx);
           
            if(Mathf.Abs(inclination) <= 1)
            {
                return sx > dx ? Unit._direction.left : Unit._direction.right;
            }
            else
            {
                return sy > dy ? Unit._direction.down : Unit._direction.up;
            }            
        }
        
    }
}
