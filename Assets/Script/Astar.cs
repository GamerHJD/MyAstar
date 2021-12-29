/****************************************************
    文件：Astar.cs
	作者：HJD  
    邮箱: 491799022@qq.com
    日期：2021/6/8
	功能：Astar寻路系统
    备注：Init创建地图格子信息，ShowRange显示范围，FindPath寻找路径+MoveUpdate行走距离每帧更新
*****************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Astar : MonoBehaviour
{
    private GameObject[] players;
    private Cell[,] cells ;
    private List<Cell> cellList = new List<Cell>();
    private List<Cell> AttackList = new List<Cell>();
    private List<Cell> RangeList = new List<Cell>();
    private List<Cell> MoveList = new List<Cell>();

    private List<Cell> EnemyCellList = new List<Cell>();
    private Coroutine MoveCor = null;
    public float moveSpeed;
    private int mapHeight;
    private int mapWith;
         
    public static Astar Instance = null;

    private void Awake()
    {
     if (Instance == null)
        {
            Instance = this;
        }   
    }

    private void Start()
    {
        InitMap();
        players = GameObject.FindGameObjectsWithTag("Player");
    }
    /// <summary>
    /// 初始化网格地图
    /// </summary>
    private void InitMap()
    {
        
        //取最大的XY，然后赋值给Cells的NewCell[]
        GameObject[] cs = GameObject.FindGameObjectsWithTag("Cell");
        int[] Xcount=new int[cs.Length+1];
        int[] Ycount = new int[cs.Length + 1];


        for (int i = 0; i < cs.Length; i++)
        {
            Cell cel = cs[i].GetComponent<Cell>();
            //Z is X, X is Y 
            if (cel != null)
            {
                int X = Mathf.Abs((int)cs[i].transform.position.z);
                if (X > 0)
                {
                    X -= (X / 2) * 1;
                }
                int Y = Mathf.Abs((int)cs[i].transform.position.x);
                if (Y > 0)
                {
                    Y -= (Y / 2) * 1;
                }
                //纳入数组
                Xcount[i] = X;
                Ycount[i] = Y;
                cel.InitCell(X, Y);//初始化格子属性
              
                cellList.Add(cel);
              

            }
        }
        //------地图属性赋值------
        int maxX= MaxValue<int>(Xcount)+1;
        int maxY = MaxValue<int>(Ycount)+1;
        UnityEngine.Debug.Log(maxX + "-" + maxY);
        mapHeight = maxY;
        mapWith = maxX;
        //-----------
        cells = new Cell[maxX, maxY];
        foreach (var cel in cellList)
        {
            cells[cel.X, cel.Y] = cel;
        }
    }



    public Transform GetPlayer(Vector3 hit)
    {

        Transform player = null;
        foreach (var p in players)
        {

            if (Vector3.Distance(p.transform.position, hit) < 1f)
            {
                player = p.transform;
                break;
            }
        }


        if (player != null)
        {


            return player;
        }

        return null;
    }
    public Ctrl GetPlayer(Cell cell)
    {
        Ctrl playerctrl = null;
        foreach (var p in players)
        {

            if (Vector3.Distance(p.transform.position, cell.transform.position) < 1f)
            {
                playerctrl = p.GetComponent<Ctrl>();
                break;
            }
        }
        if (playerctrl != null)
        {
            return playerctrl;
        }
        return null;
    }

    #region 行进过程
    //计算实体行走的携程
    IEnumerator MoveUpdate(Transform enity, List<Cell> cellList)
    {
        Ctrl ctrl = enity.GetComponent<Ctrl>();
        int index = 0;
        while (true)
        {
            yield return new WaitForEndOfFrame();
            if (index >= cellList.Count)
            {
               // ctrl.isMovingOver = true;
                ctrl.ChangeAnimation("idle");
               // ShowRangeOff();
                break;
            }
            var cellPos = cellList[index];
            var endPos = new Vector3(cellPos.transform.position.x, cellPos.transform.position.y, cellPos.transform.position.z);
            float speed = moveSpeed * Time.deltaTime;
            var currentDistance = Vector3.Distance(enity.transform.position, endPos);//当前距离
            var remainingDistance = currentDistance - speed; //剩下距离
            var timer3 = 1f;
            if (remainingDistance <= 0)
            {//重置
                remainingDistance = 0;
                timer3 = 1f;
            }
            else
            {
                float timer1 = currentDistance / speed;
                float timer2 = remainingDistance / speed;
                timer3 = 1 - (timer2 / timer1);
            }
            if (timer3 == 1)
            {
                index += 1;
            }
            Vector3 outPos = Vector3.Lerp(enity.transform.position, endPos, timer3);
            enity.transform.LookAt(outPos);    
            enity.transform.position = outPos;
        }
    }
    public void FindPath(Ctrl enity, Cell start, Cell end)
    {
        //计时器
        int conut = 0;
        //开启列表 关闭列表
        List<Cell> closeList = new List<Cell>();
        List<Cell> openList = new List<Cell>();
        if (start != end)
        {
            openList.Add(start); //OPEN+1
            while (openList.Count > 0)
            {

                Cell point = FindMinFOfPoint(openList, start, end);//第二遍开始计算起点周围的9个点
                openList.Remove(point);  //OPEN-1
                point.ChangeCellColor(3);
                conut++;
                //Debug.Log(conut + "步:" + point.X + "_" + point.Y);

                closeList.Add(point);    //CLOSE+1

                bool check = CheckCellEnemy(end,2);
                List<Cell> surroundPoints = GetRroundNode(point, check);

                PointsFilter(surroundPoints, closeList);//滤除已Close过的点，第一步会跳过，
                                                        //------------核心判断----------------
                foreach (Cell surroundPoint in surroundPoints)
                {
                    if (openList.IndexOf(surroundPoint) > -1)//第一步为false,第二遍foreach开始执行此处
                    {
                        float nowG = CalcG(surroundPoint, point);
                        if (nowG < surroundPoint.G)  //此时得到周围最小G值的格子
                        {
                            surroundPoint.UpdateParent(point, nowG);
                        }
                    }
                    else
                    {
                        surroundPoint.Parent = point;   //第一步将起点作为Parent
                        CalcF(start, surroundPoint, end); //计算F=G+H
                        openList.Add(surroundPoint);    //第一步时OpenlIsT为0， 第一遍Foreach过去后，所有点加入Openlist
                    }
                }

                //判断是否到达
                if (openList.IndexOf(end) > -1)
                {
                    closeList.Add(end);
                    openList.Clear();

                    break;
                }
            }
            //开始行走携程
            if (MoveCor != null) StopCoroutine(MoveCor);
            MoveCor = StartCoroutine(MoveUpdate(enity.transform, closeList));
        }
    }

    private float CalcG(Cell now, Cell parent)
    {
        return Vector2.Distance(new Vector2(now.X, now.Y), new Vector2(parent.X, parent.Y)) + parent.G;
    }

    private void PointsFilter(List<Cell> surroundPoints, List<Cell> closeList)
    {
        foreach (var p in closeList)
        {
            if (surroundPoints.IndexOf(p) > -1)
            {
                surroundPoints.Remove(p);
            }
        }
    }
    //计算F值
    private void CalcF(Cell start, Cell now, Cell end)
    {
        float g = 0;
        float h = Mathf.Abs(end.Y - now.Y) + Mathf.Abs(end.X - now.X);
        if (now.Parent == null)
        {
            g = 0;
        }
        else
        {
            g = Vector2.Distance(new Vector2(now.X, now.Y), new Vector2(now.Parent.X, now.Parent.Y)) + now.Parent.G;
        }
        float f = g + h;

        now.F = f;
        now.G = g;
        now.H = h;

    } 
    #endregion




    #region 鼠标点击获取格子
    public Cell GetNodeByPosition(Vector3? hit)
    {
        foreach (var cell in cellList)
        {
            Vector3 pos = cell.transform.position;
            if (Vector3.Distance(pos, (Vector3)hit) < 0.8f)
            {
                return cell;
            }
        }
        return null;
    }
    public Cell GetNodeByPosition(Vector3 playerpos)
    {
        foreach (var cell in cellList)
        {
            Vector3 cpos = cell.transform.position;
            if (Vector3.Distance(cpos, playerpos) < 0.8f)
            {
                return cell;
            }
        }
        return null;
    }
    #endregion

    #region 算法核心  
  public void ShowRangeOff()
    {

        for (int i = 0; i < cellList.Count; i++)
        {
            cellList[i].ResetCell();
        }
        AttackList.Clear();
        EnemyCellList.Clear();
        MoveList.Clear();
        
        
    }

    ////开始计算可行走范围
    //public void FindMoveRange(int x, int y, int rest,int color=0)
    //{

    //    //----------------------------------
    //    RangeList.Clear();
    //    cells[x, y].rest = rest;
       

    //    RangeList.Add(cells[x, y]);
    //    GoNextStep(MoveList,color);
        
    //    cells[x, y].isInMoveLst = false;
    //    cells[x, y].ChangeCellColor(color);
    //    //----------------------------------

    //}
 
    List<Cell> tmplist = new List<Cell>();
    //从周围四格向外遍历
    private void GoNextStep(List<Cell> lst,int color)
    {

        tmplist.Clear();

        foreach (var item in RangeList)
        {
            int xx = 0;
            int yy = 0;


            //---------------------------
            xx = item.X - 1;
            yy = item.Y;
            if (xx > 0)
            {

               CheckNode(xx, yy,lst,color);              
            }

            //---------------------------
            xx = item.X + 1;
            yy = item.Y;
            if (xx < mapWith)
            {

             CheckNode(xx, yy,  lst, color);

            }

            //---------------------------
            xx = item.X;
            yy = item.Y + 1;
            if (yy < mapWith)
            {

              CheckNode(xx, yy, lst, color);
                

                    
              
            }
            //---------------------------
            xx = item.X;
            yy = item.Y - 1;
            if (yy > 0)
            {

                    CheckNode(xx, yy, lst, color);


            }
            //---------------------------
        }

        RangeList.Clear();
        if (tmplist.Count > 0)
        {

            RangeList.AddRange(tmplist);
            GoNextStep(lst,color);
        }
    }

    //检测格子是否可移动
    private void CheckNode(int x, int y,List<Cell> lst,int color)
    {

        if (mapHeight > y)
        {
            if (mapWith > x)
            {

                var n = cells[x, y];
                //checkEnemy(n); //判定格子上是否有敌人,无则继续
                if (n != null&&n.cellType!=2)
                {

                    if ( n.canMove == true)    //判断是否为canMove
                    {                      
                            n.isInMoveLst = true;
                            lst.Add(n);                                                     
                            //到此处就判断为可行走的格子
                            n.ChangeCellColor(color);
                            tmplist.Add(n);//加入下个计算                       
                    }
                }

            }
        }
    }
   

    #endregion

    /// <summary>
    /// 检索周围九宫格
    /// </summary>
    /// <param name="point">当前检测点</param>
    /// <param name="checkEnemy">是否检测障碍物</param>
    /// <returns></returns>
    private List<Cell> GetRroundNode(Cell point,bool checkRoadblock)
    {
        List<Cell> lst = new List<Cell>();
        Cell up = null, down = null, left = null, right = null;
        Cell lu = null, ru = null, ld = null, rd = null;

        if (point.Y < mapHeight - 1)
        {
            up = cells[point.X, point.Y + 1];
        }
        if (point.Y > 0)
        {
            down = cells[point.X, point.Y - 1];
        }
        if (point.X > 0)
        {
            left = cells[point.X - 1, point.Y];
        }
        if (point.X < mapWith - 1)
        {
            right = cells[point.X + 1, point.Y];
        }

        if (up != null && left != null)
        {
            lu = cells[point.X - 1, point.Y + 1];
        }
        if (up != null && right != null)
        {
            ru = cells[point.X + 1, point.Y + 1];
        }
        if (down != null && left != null)
        {
            ld = cells[point.X - 1, point.Y - 1];
        }
        if (down != null && right != null)
        {
            rd = cells[point.X + 1, point.Y - 1];
        }

        if (up != null)
        {
             if (up.canMove && up.isInMoveLst ) { lst.Add(up); }
        }
        if (down != null)
        {
              if (down.canMove && down.isInMoveLst  ) {lst.Add(down); }
        }
        if (left != null)
        {
             if (left.canMove && left.isInMoveLst ) { lst.Add(left);}
        }
        if (right != null)
        {if (right.canMove && right.isInMoveLst  ) {lst.Add(right); }

        }
        if (lu != null)
        {
            if (lu.canMove && lu.isInMoveLst) { lst.Add(lu); }

        }
        if (ru != null)
        {
            if (ru.canMove && ru.isInMoveLst) { lst.Add(ru); }

        }
        if (ld != null)
        {
            if (ld.canMove && ld.isInMoveLst) { lst.Add(ld); }

        }
        if (rd != null)
        {
            if (rd.canMove && rd.isInMoveLst) { lst.Add(rd); }

        }

        if (checkRoadblock)
        {
           CheckCellGroundEnemy(lst, 2);
        }
        
        return lst;
    }
    /// <summary>
    /// 检索周围四宫格
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    private List<Cell>GetRroundFourNode(Cell point)
    {
        List<Cell> lst = new List<Cell>();
        Cell up = null, down = null, left = null, right = null;
        if (point.Y <= mapHeight - 2)
        {
            up = cells[point.X, point.Y + 1];       
           
        }
        if (point.Y > 0)
        {
            down = cells[point.X, point.Y - 1];
           
        }
        if (point.X > 0)
        {
            left = cells[point.X - 1, point.Y];
          
        }
        if (point.X <= mapWith - 2)
        {
            right = cells[point.X + 1, point.Y];

        }
        if (up != null)
        {
            if (up.canMove ) { lst.Add(up); }
        }
        if (down != null)
        {
            if (down.canMove) { lst.Add(down); }
        }
        if (left != null)
        {
            if (left.canMove) { lst.Add(left); }
        }
        if (right != null)
        {
            if (right.canMove) { lst.Add(right); }
        }

        return lst;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="openList"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    private Cell FindMinFOfPoint(List<Cell> openList,Cell start,Cell end)
    {
        float f = float.MaxValue;
        
        Cell temp1 = null;
        Cell temp2 = null;

        foreach (Cell p in openList)
        {

             if (p.F < f)
            {
                temp1 = p;
                f = p.F;
               
            }
            if (temp1 != null && p.F <= temp1.F)
            {
                temp2 = p;
                
            }

        }
        if (temp1 != temp2&& temp2 != null)
        {
            double absx=Math.Sqrt(Math.Pow(( end.X-temp1.X ), 2) + Math.Pow((+end.X-temp2.X ),2));
            double absy = Math.Sqrt(Math.Pow((  end.Y-temp1.Y), 2) + Math.Pow((  end.Y-temp2.Y), 2));
            if (absx -absy>=0)//先走X轴
            {
                
                if(Math.Abs(end.X-temp1.X)< Math.Abs((end.X - temp2.X))){
                    Debug.Log(temp1.X + "--" + temp2.X);
                    return temp1;
                }
                else
                {
                    Debug.Log(temp1.X + "--" + temp2.X);
                    return temp2;
                }
            }
            else
            {
                if (Math.Abs(end.Y - temp1.Y) < Math.Abs((+end.Y - temp2.Y)))
                {
                    return temp1;
                }
                else
                {
                    return temp2;
                }
            }
            
        }
        return temp1;
    }
    //检索列表中格子周围的敌人数量
    private void CheckCellGroundEnemy(List<Cell> groundCells,int EnemyNum)
    {
        int index = 0;
        
        //------如果P点周围四个正点有2个以上敌人格子，则不考虑-------------

        for (int i = 0; i < groundCells.Count; i++)
        {
            List<Cell> lst = GetRroundFourNode(groundCells[i]);
            if (lst.Count != 0)
            {
                for (int j = 0; j < lst.Count; j++)
                {
                    if (lst[j].cellType == 2)
                    {
                        index += 1;
                    }
                    if (index > EnemyNum)
                    {
                        
                        index = 0;
                        MoveList.Remove(groundCells[i]);
                        groundCells.Remove(groundCells[i]);
                          

                    }

                }
            }

            
        }
          
        //-----------------------------------------------------------

    }
    //检索单个格子周围的敌人数量
    private bool CheckCellEnemy(Cell cell, int EnemyNum)
    {
            int index = 0;
            List<Cell> lst = GetRroundFourNode(cell);
            if (lst.Count != 0)
           {
            for (int i = 0; i < lst.Count; i++)
            {
                if (lst[i].cellType == 2)
                {
                    index++;
                    
                }
                
            }
           }
        if (index >= EnemyNum)
        {
            return false;
        }
        return true;
        
    }
    //检测格子是否在可移动列表内
    public bool NodeisInMoveRange(Cell cell)
    {
        if (MoveList .Contains(cell))
        {
            return true;
        }

        return false;
    }

    ////检测当前格子是否有敌人或友军
    //public void checkEnemy(Cell cell)
    //{
      
        
    //        var trans = GetPlayer(cell.transform.position);
    //        if (trans != null)
    //       {
    //            Ctrl ctrl = trans.GetComponent<Ctrl>();
    //            if (ctrl.camp != Battlemgr.Instance.currentCamp)//如果不是自己同伙
    //            {
    //                cell.ChangeCellColor(0);
    //                EnemyCellList.Add(cell);
    //                cell.cellType = 2; //判定为敌人占格


    //            }
    //             else
    //            {
    //            cell.ChangeCellColor(0);
    //            cell.cellType = 1;
    //            }

    //    }
        
        
        
    //}


 




    #region 待独立出去的工具方法
    //传入一个数组,求出一个数组的最大值
    public static T MaxValue<T>(T[] arr) where T : IComparable<T>
    {
        var i_Pos = 0;
        var value = arr[0];
        for (var i = 1; i < arr.Length; ++i)
        {
            var _value = arr[i];
            if (_value.CompareTo(value) > 0)
            {
                value = _value;
                i_Pos = i;
            }
        }
        return value;
    } 
    #endregion

}
