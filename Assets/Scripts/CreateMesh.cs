using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using Ocean;
using FourierUtils;

public class CreateMesh : MonoBehaviour
{
    OceanUtils ocean_utils;
    int N;
    int L;
    float lambda;
    Vector3[] verts;
    int[] triangles;

    volatile bool done = true;

    Mesh mesh;
    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        N = 64;
        L = 32;
        lambda = -1f;
        ocean_utils = new OceanUtils(0.0002f, N, L, new Vector2(32.0f,32.0f));
    }

    // Update is called once per frame
    void Update()
    {
      //se nao tiver terminado a execucao, n executa o codigo
      if(!done) return;

      //iniciar novos calculos
      done = false;

      Nullable<float> time = Time.realtimeSinceStartup;
      RunOnPool(time);

      /*
      byte[] bytes = hMap.EncodeToPNG();

      File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", bytes);
      */

      verts = new Vector3[(N)*(N)];
      int sign;
			float[] signs = new float[]{ 1.0f, -1.0f };

      //calcular vertices
      int i =0;
      for(int z = 0; z < N; z++){
        for(int x = 0; x< N; x++){
          // calcular os sinais do displacement (não sei direito)
          sign = (int)signs[(x + z) & 1];

          //calcualar coordenadas baseado no hMap e dispMap
          float y_new = ocean_utils.hMap[x,z].real*sign;
          float x_new = (float)(x-N/2)*L/N + lambda*ocean_utils.dispMap_x[x,z].real*sign;
          float z_new = (float)(z-N/2)*L/N + lambda*ocean_utils.dispMap_z[x,z].real*sign;
          //atualizar o valor do vertice
          verts[i] = new Vector3(x_new,y_new,z_new);
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
      mesh.RecalculateNormals();
    }

    void RunOnPool(object o){
      Nullable<float> time  = o as Nullable<float>;

      ocean_utils.Sample(time.Value);
      done = true;

    }

}
