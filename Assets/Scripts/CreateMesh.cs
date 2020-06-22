using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using Ocean;
using FourierUtils;
using  Diag = System.Diagnostics;

public class CreateMesh : MonoBehaviour
{
    OceanUtils ocean_utils;
    int N;
    int L;
    float lambda;
    Vector3[] verts;
    int[] triangles;
    Vector3[] normals;
    Vector2[] uvs;
    volatile bool done = true;
    Renderer m_Renderer;

    Mesh mesh;
    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        N = 64;
        L = 128;
        lambda = 1f;
        ocean_utils = new OceanUtils(0.0002f, N, L, new Vector2(16.0f,16.0f), lambda);
        m_Renderer = GetComponent<Renderer> ();
        m_Renderer.material.EnableKeyword("_FresnelTable");
        m_Renderer.material.EnableKeyword("_FoldingTable");
        m_Renderer.material.SetTexture("_FresnelTable", ocean_utils.frTable);
    }

    // Update is called once per frame
    void Update()
    {
      if(!done) return;

      done = false;

      Nullable<float> time = Time.realtimeSinceStartup;
      RunOnPool(time);
      //ThreadPool.QueueUserWorkItem(new WaitCallback(RunOnPool), time);

      /*
      byte[] bytes = hMap.EncodeToPNG();

      File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", bytes);
      */

      m_Renderer.material.SetTexture("_FoldingTable", ocean_utils.foldingTable);
      byte[] bytes = ocean_utils.foldingTable.EncodeToPNG();

      File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", bytes);

      verts = new Vector3[(N)*(N)];
      normals = new Vector3[(N)*(N)];
      uvs = new Vector2[(N)*(N)];
      //calcular vertices
      int i =0;
      for(int z = 0; z < N; z++){
        for(int x = 0; x< N; x++){
          //calcualar coordenadas baseado no hMap e dispMap
          float y_new = ocean_utils.hMap[x,z];
          float x_new = (float)(x-N/2)*L/N + ocean_utils.dispMap[x,z].x;
          float z_new = (float)(z-N/2)*L/N + ocean_utils.dispMap[x,z].y;
          //atualizar o valor do vertice
          verts[i] = new Vector3(x_new,y_new,z_new);
          //atualizar valores das normais
          normals[i] = ocean_utils.normalMap[x,z];
          //atualizar valor do uv map
          uvs[i] = new Vector2((float)x/((float)(N-1)), (float)z/((float)(N-1)));
          i++;
        }
      }

      //calcular triangulos
      int[] triangles = new int[(N-1)*(N-1)*6];
      int vert = 0;
      int tris = 0;
      for(int z = 0; z < N-1; z++){
        for(int x =0; x < N-1; x++){

          triangles[0 + tris] = vert +0;
          triangles[1 + tris] = vert + (N-1) +1;
          triangles[2 + tris] = vert + 1;
          triangles[3 + tris] = vert + 1;
          triangles[4 + tris] = vert + (N-1) + 1;
          triangles[5 + tris] = vert + (N-1) + 2;

          vert++;
          tris += 6;
        }
        vert++;
      }

      //atualizar malha
      mesh.Clear();
      mesh.vertices = verts;
      mesh.triangles = triangles;
      mesh.normals = normals;
      mesh.uv = uvs;
      //mesh.RecalculateNormals();

    }

    void RunOnPool(object o){
      Nullable<float> time  = o as Nullable<float>;
      ocean_utils.Sample(time.Value);
      done = true;
    }

}
