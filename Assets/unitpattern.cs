using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class unitpattern : MonoBehaviour
{
    //좆구린 유닛 자동 행동 체계
    public Unit u;

    private void Start()
    {
        if(u == null)
        {
            u = gameObject.GetComponent<Unit>();          
        }
    }

    private void FixedUpdate()
    {
        proc();
    }


    void proc()
    {
        if(u == null)
        {
            return;
        }

        switch (u.type)
        {
            case Unit._type.unit:
                {
                    unitproc();
                }
                break;

            case Unit._type.building:
                {
                    buildingproc();
                }
                break;
        }
    }

    //attackdown 등의 주변 정보가 필요한 action
    public class paction
    {
        public enum typelist : int
        {
            attackdown = 1
        }

        public bool started = false;

        public typelist type;
        public int[] i;
        public float[] f;
        public string[] s;        


        public paction(typelist t, int[] idata,float[] fdata, string[] strdata)
        {
            type = t;
            if(idata != null)
            {
                i = (int[])idata.Clone();
            }

            if (fdata != null)
            {
                f = (float[])fdata.Clone();
            }

            if (strdata != null)
            {
                s = (string[])strdata.Clone();
            }
        }
    }
    public paction currentpaction;


    public Unit target;
    public float searchrange = 1;
    public int wpindex = -1; //이거 너무ㅡ 오래끌면 이 무기만 쓸려하지않을까? 고민
    void unitproc() //20210510기준 이동 자체가 끝난 시점에서 처리를 하기 때문에 서로 가까워지려고 좌우로 움직이면 이동이 끝난 시점에 좌우가 뒤바뀌어서 다시 반복하게 됨. 수정이 필요할듯 <= 0512 : 대충랜덤
    {
        //어차피 자동적으로 action을 생성 후 추가하기 위한 거니까 행동불가거나 하고있는게 있으면 안 함.
        if(currentpaction != null)
        {
            if(pactionproc(currentpaction))
            {
                currentpaction = null;
            }
            return;
        }

        if(!cancommand())
        {
            return;
        }

        
        if(target == null)
        {
            target = findenemy(searchrange);
            if(target == null) //찾아도 없으면
            {
                
            }
        }
        else
        {
            //타겟 유효성
            if (!system.isin(target.x,target.y,u.x - searchrange, u.y - searchrange, u.x + searchrange, u.y + searchrange) || u.team == target.team)
            {
                target = null;                
            }   
            else
            {
                if(wpindex >= 0) //이미 선택한 무기가 있을 경우
                {
                    weapon wp = u.findweapon(wpindex);
                    if(Mathf.Abs(u.x - target.x) + Mathf.Abs(u.y - target.y)  <= wp.range)
                    {
                        Debug.Log("( " + target.x + " , " + target.y + " )");
                        u.addaction(unitaction.typelist.useweapon, new int[] { wpindex }, new float[] { target.x, target.y }, null);
                        wpindex = -1;
                    }
                    else //접근 
                    {
                        u.addaction(unitaction.typelist.approach, new int[] { target.ix, target.iy }, new float[] { wp.range });
                    }                                        
                }
                else
                {
                    bool cantattack;

                    List<weapon> wplist = new List<weapon>(u.GetComponents<weapon>()); //unit에서 index로 weapon을 찾는 건 getcomponents로 하는걸로 동일하므로 여기서도 동일하게 하면 될듯

                    if (wplist.Count <= 0)
                    {
                        cantattack = true;
                    }
                    else
                    {
                        Dictionary<int, int> prdic = new Dictionary<int, int>();
                        //wp 선택
                        int psum = 0;
                        int i = 0;
                        foreach (weapon wp in wplist) 
                        {
                            if (wp.available() && wp.priority > 0)
                            {
                                psum += wp.priority;
                                prdic.Add(i, wp.priority);
                            }
                            i++;
                        }

                        cantattack = psum <= 0;

                        psum = UnityEngine.Random.Range(1, psum + 1);
                        foreach(int prkey in prdic.Keys) //psum 이 0 이하면 실행안됨
                        {
                            psum -= prdic[prkey];
                            if(psum <= 0)
                            {
                                wpindex = prkey;
                                break;
                            }
                        }

                    }

                    if(cantattack)
                    {

                    }
                }

                
                                                
            }
            
        }
    }



    public List<Unit> myunits = new List<Unit>(), defenceunitlist = new List<Unit>();
    public Dictionary<Unit, Unit> attackunitlist = new Dictionary<Unit, Unit>();//myunit,enemy
    public int defenceunitcount = 4 , maxunitcount = 10; // 주둔 유닛 수 , 최대 유닛 수
    public int defendrange = 2;//적이 이 범위안에오면 자신 방어 행동
    public List<Unit> targetlist = new List<Unit>(); //여러 타겟 
    void buildingproc() //생산건물 전용 패턴.
    {
        //어차피 자동적으로 action을 생성 후 추가하기 위한 거니까 행동불가거나 하고있는게 있으면 안 함.
        if (!u.canaction || u.currentaction != null || u.actionlist.Count > 0 || u.pushedaction != null)
        {
            return;
        }

        system sys = system.findsystem();
        if (sys == null)
        {
            return;
        }

        //타겟리스트
        targetlist = findenemies(defendrange);
        if(targetlist.Count < 1)
        {
            targetlist = findenemies(searchrange);
        }




        //유닛 생산
        Unitbuilder ub = GetComponent<Unitbuilder>();

        if (ub != null)
        {
            if (ub.current == null && ub.list.Count <= 0) //생산중인 유닛이 없을때
            {
                //생산할 유닛 선택
                Unitbuildinfo[] ubinfolist = GetComponents<Unitbuildinfo>();
                if (ubinfolist != null)
                {
                    ub.request(ubinfolist.Length);
                }
            }



            //생산한 유닛들을 나중에 명령 내릴 수 있도록 리스트에 저장
        }

        List<Unit> removelist = new List<Unit>();
        //myunit처리
        foreach(Unit myunit in myunits)
        {
            //유효성
            if(myunit == null)
            {
                removelist.Add(myunit);
                continue;
            }
            else if(u.team != myunit.team)
            {
                removelist.Add(myunit);
                continue;
            }


            
            //일단 주둔시키는 유닛 부터 우선으로 해야
            if (defenceunitlist.Count < defenceunitcount)
            {
                defenceunitlist.Add(myunit);
                removelist.Add(myunit);
            }
            else
            {
                attackunitlist.Add(myunit , null);
                removelist.Add(myunit);
            }
                
        }

        //유효하지 않거나 다른 쪽으로 할당된 myunits 제거
        foreach(Unit removeunit in removelist)
        {
            myunits.Remove(removeunit);
        }


        //attackunit, defencenunit 처리
        foreach (Unit atku in attackunitlist.Keys)
        {
            //기용 가능한 상황인지 체크
            //조건 : 행동가능,패턴존재,타겟이없음, 나중에 더 추가할수도있음
            if(atku == null)
            {
                continue;
            }            

            unitpattern atkuptrn = atku.GetComponent<unitpattern>();
            if (atkuptrn != null)
            {
                if(atkuptrn.target != attackunitlist[atku])
                {
                    attackunitlist[atku] = target;
                }

                if(atkuptrn.cancommand() && attackunitlist[atku] == null)
                {
                    //보통은 공격 유닛을 보내지 않은 타겟 우선적으로 할려했지만 일단은 랜덤으로 박아넣기로함. 변수설계가 복잡해져서
                    
                    atkuptrn.pactionrequest(new unitpattern.paction(paction.typelist.attackdown, new int[] { }))
                }                                

            }

        }


        foreach(Unit defu in defenceunitlist)
        {

        }




        //대응 시작
        if (target != null)
        {
            
        }
    }

    public bool pactionrequest(paction dest) //이미있으면안함
    {
        if(dest == null || currentpaction != null)
        {
            return false;
        }

        currentpaction = dest;
        return true;
    }

    public void pactionset(paction dest) //강제
    {
        currentpaction = dest;
    }


    private bool pactionproc(paction dest)
    {
        if(dest == null)
        {
            return true;
        }
        //cancommand 쓰면안됨


        bool complete = false;

        switch (dest.type)
        {
            case paction.typelist.attackdown:
                {
                    if(!dest.started)
                    {
                        if(dest.i.Length < 2)
                        {
                            complete = true;
                            break;
                        }
                    }

                    if(system.tilex(u.x) == dest.i[0] && system.tiley(u.y) == dest.i[1])
                    {  
                        complete = true;
                        break;
                    }


                    if(target != null)
                    {
                        if (system.isin(target.x, target.y, u.x - searchrange, u.y - searchrange, u.x + searchrange, u.y + searchrange))
                        {
                            target = null;
                        }

                    }

                    if(target == null)
                    {
                        target = findenemy(searchrange);
                        if(target == null)
                        {
                            //암것도 안하고있으면 목적지 이동
                            if(cancommand())
                            {
                                u.addaction(unitaction.typelist.movedest, new int[] { dest.i[0], dest.i[1] }, null, null);
                            }

                            complete = false;
                            break;
                        }
                    }


                    complete = false;
                    //target이 있는 상태니까 그냥 unitproc으로넘겨줌
                }
                break;
        }

        if(!dest.started)
        {
            dest.started = true;
        }
        return complete;
    }


    public bool cancommand() //unitpattern 컴포넌트 기준에서 행동 가능한 상태인지 판별
    {
        if(u == null)
        {
            return false;
        }

        return currentpaction != null && u.canaction && u.actionlist.Count <= 0 && u.currentaction == null && u.pushedaction == null;
    }


    private Unit findenemy(float range)
    {
        system sys = system.findsystem();
        if (sys == null)
        {
            return null;
        }

        List<Unit> candidates = new List<Unit>();
        foreach(Unit c in sys.findunit(u.x, u.y, range, new Unit[] { u }))
        {
            if(c.team != u.team)
            {
                candidates.Add(c);
            }
        }

        if(candidates.Count < 1)
        {
            return null;
        }

        //일단은 랜덤 리턴으로
        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }

    private List<Unit> findenemies(float range)
    {
        system sys = system.findsystem();
        if (sys == null)
        {
            return null;
        }

        List<Unit> r = new List<Unit>();
        foreach (Unit c in sys.findunit(u.x, u.y, range, new Unit[] { u }))
        {
            if (c.team != u.team)
            {
                r.Add(c);
            }
        }

        return r;
    }
}
