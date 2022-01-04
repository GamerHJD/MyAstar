

using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.UI;

public class CellCreator : EditorWindow
{
    private Text text;
    private string mWidth="1";
    private string mHeight = "1";
    private string mgap = "1";
    
    

    [MenuItem("Cell/CellCreat")]
    static void AddWindow()
    {
        //创建窗口
        Rect wr = new Rect(0, 0, 500, 200);
        CellCreator window = (CellCreator)EditorWindow.GetWindowWithRect(typeof(CellCreator), wr, true, "widow name");
        window.Show();

    }

    private static void CreatMesh()
    {
         GameObject quad = new GameObject();
         MeshFilter filter= quad.AddComponent<MeshFilter>();
         MeshRenderer mr = quad.AddComponent<MeshRenderer>();
         // 为网格创建顶点数组
         Vector3[] vertices = new Vector3[4]{
                     new Vector3(1, 1, 0),
                     new Vector3(-1, 1, 0),
                     new Vector3(1, -1, 0),
                     new Vector3(-1, -1, 0)
           };
         // 通过顶点为网格创建三角形
         int[] triangles = new int[2 * 3]{
            0, 3, 1, 0, 2, 3
          };
        
          //并新建一个mesh给它
           Mesh mesh = new Mesh();
           mesh.vertices = vertices;
           mesh.triangles = triangles;
           filter.sharedMesh = mesh;
        //Todo 赋予材质
       // mr.material = material;

        quad.transform.eulerAngles = new Vector3(90, 0, 0);
    }

   
    public static void CreatMap(int width,int height,int gap)
    {
        CreatMesh();

        Vector3 creatPos;
        for (int i = 0; i < height; i++)//列
        {
            creatPos = new Vector3(0, 0, i);
            for (int j = 0; j < width; j++)//行
            {
                  CreatMesh(size,i.ToString()+"-"+j.ToString());
            }
        }

    }


    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("行:", GUILayout.Width(45));
        mWidth= GUILayout.TextField(mWidth);
        GUILayout.Label("列:", GUILayout.Width(50));
        mHeight = GUILayout.TextField(mHeight);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("间隙:", GUILayout.Width(50));
        mgap = GUILayout.TextField(mgap);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Creat"))
        {
            var width = int.Parse(mWidth);
            var height = int.Parse(mHeight);
            var gap = int.Parse(mgap);
            CreatMap(width, height,gap);

            Close();
        }
    }
}