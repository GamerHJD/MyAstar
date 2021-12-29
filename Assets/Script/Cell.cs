/****************************************************
    文件：Cell.cs
	作者：Plane
    邮箱: 1785275942@qq.com
    日期：#CreateTime#
	功能：Nothing
*****************************************************/

using UnityEngine;

public class Cell : MonoBehaviour
{
    public enum CellTag
    {
        justice,
        Enemy,
        none,
    }

    public Cell Parent = null;
    
    public  int cellType=0;  //0-None,1-Player,2-Enemy
    public int rest = 0;
    public int attackrest = 0;
    public int cost = 1;
    public float F;
    public float G;
    public float H;

    public int X;
    public int Y;
    public bool canMove;
    public bool isInMoveLst = false;
    public bool isSkillTarget = false;
    

    public Material redMat;
    public Material blueMat;
    public Material whiteMat;
    public Material greenMat;
    public void  InitCell(int x, int y, Cell parent = null)
    {
        this.X = x;
        this.Y = y;
        this.Parent = parent;
       
    }
    
    public void UpdateParent(Cell parent, float g)
    {
        this.Parent = parent;
        this.G = g;
        F = G + H;
    }
    public void ResetCell()
    {
        this.Parent = null;
        this.cellType = 0;
        this.isInMoveLst = false;
        this.isSkillTarget = false;
        this.rest = 0;
        this.attackrest = 0;
        this.F = 0;
        this.G = 0;
        this.H = 0;
        this.ChangeCellColor(0);
    }
    public void ChangeCellColor(int index)
    {
        switch (index)
        {
            case 0:
                this.GetComponent<MeshRenderer>().material = whiteMat;
                break;
            case 1:
                this.GetComponent<MeshRenderer>().material = blueMat;
                break;
            case 2:
                this.GetComponent<MeshRenderer>().material = redMat;
                break;
            case 3:
                this.GetComponent<MeshRenderer>().material = greenMat;
                break;
        }
       
    }
    public void SetMateril()
    {
        if (this.canMove)
        {
            this.GetComponent<MeshRenderer>().material = whiteMat;
        }
        else
        {
            this.GetComponent<MeshRenderer>().material = redMat;
        }
    }
}