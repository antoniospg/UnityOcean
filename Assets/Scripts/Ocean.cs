using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using FourierUtils;
using System.IO;
using  Diag = System.Diagnostics;

namespace Ocean{

  class OceanUtils{
    //constante gravitacional
    const float g = 9.81f;

    //constantes
    public float A;
    public Vector2 Wind = new Vector2(0f,0f);
    private int N;
    private float L;
    private float lambda;
    private float Jm;

    //matrizes pre calculadas
    public Vector2[,] ph0;
    public Vector2[,] ph0_conj;
    public float[,] disp_table0;
    public float[,] disp_table0_conj;
    public Texture2D frTable;

    //matrizes atualizadas a cada iteracao
    public float[,] hMap;
    public Vector2 [,] dispMap;
    float[,] jacobian;
    public Texture2D foldingTable;
    public Vector3 [,] normalMap;

    public OceanUtils(float A, int N, float L, Vector2 Wind, float lambda, float Jm){
      this.A = A;
      this.N = N;
      this.L = L;
      this.Wind = Wind;
      this.lambda = lambda;
      this.Jm = Jm;

      ph0 = new Vector2[N,N];
      ph0_conj = new Vector2[N,N];;
      disp_table0 = new float[N,N];
      disp_table0_conj = new float[N,N];

      frTable = new Texture2D(512, 1, TextureFormat.Alpha8, false);
      frTable.filterMode = FilterMode.Bilinear;
      frTable.wrapMode = TextureWrapMode.Clamp;
      frTable.anisoLevel = 0;

      foldingTable = new Texture2D(N, N, TextureFormat.RGBAFloat, false);
      //foldingTable.filterMode = FilterMode.Bilinear;
      //foldingTable.wrapMode = TextureWrapMode.Clamp;
      //foldingTable.anisoLevel = 0;

      hMap = new float[N,N];
      dispMap = new Vector2[N,N];
      jacobian = new float[N,N];
      normalMap = new Vector3[N,N];

      Init();
      FresnelTable();
    }

    //criar tabela com os coeficientes de reflexão para cada teta incidente
    //colocar numa textura para usar no shader
    public void FresnelTable(){
      //indices de refração
      float n2 = 1.34f;
      float n1 = 1f;

      for(int x = 0; x < 512; x++){
				float fresnel = 0.0f;
				float costhetai = (float)x/511.0f;
				float thetai = Mathf.Acos(costhetai);
				float sinthetat = Mathf.Sin(thetai)*n1/n2;
				float thetat = Mathf.Asin(sinthetat);

        float F0 = Mathf.Pow(((n2-n1)/(n2+n1)),2);
        fresnel = F0 + (1-F0)*Mathf.Pow(1-costhetai,5);
/*
//INITIAL EXPRESSION USED
				if(thetai == 0.0f){
					fresnel = (n2 - n1)/(n2 + n1);
					fresnel = fresnel * fresnel;
				}
				else{
					float fs = Mathf.Sin(thetat - thetai) / Mathf.Sin(thetat + thetai);
					float ts = Mathf.Tan(thetat - thetai) / Mathf.Tan(thetat + thetai);
					fresnel = 0.5f * ( fs*fs + ts*ts );
				}
*/
				frTable.SetPixel(x, 0, new Color(fresnel,fresnel,fresnel,fresnel));
			}

			frTable.Apply();
    }

    //Distribuição gaussiana média 0 e desvio padrão 1 usando Marsaglia polar method
    public static Vector2 NextGaussian() {
      float v1, v2, s;
      do {
          v1 = 2.0f * Random.Range(0f,1f) - 1.0f;
          v2 = 2.0f * Random.Range(0f,1f) - 1.0f;
          s = v1 * v1 + v2 * v2;
      } while (s >= 1.0f || s == 0f);

      s = Mathf.Sqrt((-2.0f * Mathf.Log(s)) / s);

      return  new Vector2(v1*s, v2*s);
    }

    //Dispersion relation profundidade muito grande
    public static float DispRelDeep(float kLen){

      float w_0 = 2.0f * Mathf.PI / 200.0f;
      float w = Mathf.Floor(Mathf.Sqrt(g*kLen)/w_0)*w_0;
      return w;
    }

    public Vector2 Complex_Product(Vector2 a, Vector2 b){
      return new Vector2(a.x*b.x - a.y*b.y, a.x*b.y + a.y*b.x);
    }

    public Vector2 ObtainPolar(float r, float teta){
      return new Vector2(r*Mathf.Cos(teta), r*Mathf.Sin(teta));
    }

    //Dispersion relation em função da profundidade
    public static float DispRelCom(float kLen, float depth){
      float tgh = (Mathf.Exp(kLen*depth) - Mathf.Exp(-kLen*depth))/
                   (Mathf.Exp(kLen*depth) + Mathf.Exp(-kLen*depth));

      return Mathf.Sqrt(g*kLen* tgh);
    }

    //Calcular Phillips Spectrum
    public float PhillSpec(Vector2 K){

      float w_length = Wind.magnitude;
      Vector2 m_windDirection = new Vector2(Wind.x, Wind.y);
      m_windDirection.Normalize();

      float k_length  = K.magnitude;
			if (k_length < 0.000001f) return 0.0f;

			float k_length2 = k_length  * k_length;
			float k_length4 = k_length2 * k_length2;

			Vector2 K_dir = new Vector2(K.x, K.y);
      K_dir.Normalize();

			float k_dot_w   = Vector2.Dot(K_dir, m_windDirection);
			float k_dot_w2  = k_dot_w * k_dot_w * k_dot_w * k_dot_w * k_dot_w * k_dot_w;

			float L         = w_length * w_length / g;
			float L2        = L * L;

			float damping   = 0.001f;
			float l2        = L2 * damping * damping;

      float ph = A * Mathf.Exp(-1.0f / (k_length2 * L2)) / k_length4* k_dot_w2 * Mathf.Exp(-k_length2 * l2);
			return ph;

    }

    public Vector2 FSpectrum(Vector2 K){

      Vector2 random = NextGaussian();
      float ph = Mathf.Sqrt(PhillSpec(K)/2.0f);
      return random*ph;
    }

    public Vector2 calcWaveVector(int n, int m){

      float Kx = Mathf.PI*2*((2*n - N)/2)/L;
      float Kz = Mathf.PI*2*((2*m - N)/2)/L;
      return new Vector2(Kx, Kz);
    }

    public Vector2 FAmplitudes(Vector2 spec, Vector2 spec_conj,float disp, float t){

      Vector2 z = ObtainPolar(1,disp*t);
      Vector2 z_conj = ObtainPolar(1,disp*-t);

      return Complex_Product(spec,z) + Complex_Product(spec_conj, z_conj);
    }

    public void Init(){

      for(int i = 0; i < N; i++){
        for(int j = 0; j < N; j++){

          Vector2 K = calcWaveVector(i, j);
          Vector2 K_conj = calcWaveVector(-i, -j);

          Vector2 spec = FSpectrum(K);
          Vector2 spec_conj =FSpectrum(K_conj);
          spec_conj.y *= -1;

          ph0[i,j] = spec;
          ph0_conj[i,j] = spec_conj;

          disp_table0[i,j] = DispRelDeep(K.magnitude);
        }
      }
    }

    public void Sample(float t){

      //consertar os valores gerados pela fft
      float[] signs = new float[]{ 1.0f, -1.0f };

      Vector2[,] spec_hmap = new Vector2[N,N];
      Vector2[,] spec_disp_x = new Vector2[N,N];
      Vector2[,] spec_disp_z = new Vector2[N,N];

      int cont = 0;

      for(int i = 0; i < N; i++){
        for(int j = 0; j < N; j++){

          //heigh map compontents
          Vector2 spec = ph0[i,j];
          Vector2 spec_conj =  ph0_conj[i,j];

          float disp = disp_table0[i,j];

          Vector2 h = FAmplitudes(spec, spec_conj, disp, t);
          spec_hmap[i,j] = h;

          //dispMap components
          //calculando unitario do  waveVector
          Vector2 u_waveVector = calcWaveVector(i,j);
          u_waveVector.Normalize();

          //atualizando os valores da matriz
          spec_disp_x[i,j] = Complex_Product(spec_hmap[i,j], ObtainPolar((u_waveVector.x),3*Mathf.PI/2));
          spec_disp_z[i,j] = Complex_Product(spec_hmap[i,j], ObtainPolar((u_waveVector.y),3*Mathf.PI/2));
        }
      }

      //calular a fft para cada componente
      Vector2[,] hMap = new Vector2[N,N];
      Vector2[,] dispMap_x = new Vector2[N,N];
      Vector2[,] dispMap_z = new Vector2[N,N];

      //rows
      Vector2[] fl_hMap = new Vector2[N];
      Vector2[] fl_dispMap_x = new Vector2[N];
      Vector2[] fl_dispMap_z = new Vector2[N];
      for (int i =0; i < N; i++){
        for(int j =0; j< N ; j++){
          fl_hMap [j] = spec_hmap[i,j];
          fl_dispMap_x [j] = spec_disp_x[i,j];
          fl_dispMap_z [j] = spec_disp_z[i,j];
        }

        Fourier.iFFT(fl_hMap );
        Fourier.iFFT(fl_dispMap_x );
        Fourier.iFFT(fl_dispMap_z );

        for(int j = 0; j< N; j++){
          hMap[i,j] = fl_hMap [j];
          dispMap_x[i,j] = fl_dispMap_x [j];
          dispMap_z[i,j] = fl_dispMap_z [j];
        }

      }

      //collums
      for (int i =0; i < N; i++){
        for(int j =0; j< N ; j++){

          fl_hMap [j] = hMap[j,i];
          fl_dispMap_x [j] = dispMap_x[j,i];
          fl_dispMap_z [j] = dispMap_z[j,i];
        }

        Fourier.iFFT(fl_hMap );
        Fourier.iFFT(fl_dispMap_x );
        Fourier.iFFT(fl_dispMap_z );

        //atualizar os maps com os ultimos valores escritos das linhas(apenas parte real)
        //obs -> dispMap n eh um numero complexo, apenas guarda o displacement para x e y
        //já obter os valores finais de cada matriz, inclusive multiplicando pelo lambda
        for(int j = 0; j< N; j++){
          //consertar sinal do valor gerado pela fft
          int sign = (int)signs[(i + j) & 1];

          this.hMap[j,i] = fl_hMap[j].x*sign;
          this.dispMap[j,i].x = fl_dispMap_x[j].x*sign*lambda;
          this.dispMap[j,i].y = fl_dispMap_z [j].x*sign*lambda;
        }
      }
      CalculateFolding();
    }

    public void CalculateFolding(){
      //step
      float dr = L/N;
      //armazenar valor para as derivadas parciais para cada ponto(displacement)
      float der_x = 0;
      float der_z = 0;
      float der_zx = 0;
      //valor das derivadas (heightmap)
      float h_der_x = 0;
      float h_der_z = 0;

      //main for, calcular derivadas parciais do displacement map, usando derivação central
      for(int i =0; i< N; i++){
        for(int j = 0; j< N; j++){

          //calcular derivada parcial de Dx em rel a x
          //se tiver nas bordas, usar diferenciação comum de 1 ordem
          if(i == 0){
            //derivada do dispMap
            float dD = dispMap[i+1,j].x - dispMap[i,j].x;
            //derivada do heightmap
            float dH = hMap[i+1,j] - hMap[i,j];
            float dx = dr;
            der_x = dD/dx;
            h_der_x=dH/dx;
          }
          //se tiver na outra extremidade, usar diferenciação comum
          else if(i == N-1){
            float dD = dispMap[i,j].x - dispMap[i-1,j].x;
            float dH = hMap[i,j] - hMap[i-1,j];
            float dx = dr;
            der_x = dD/dx;
            h_der_x = dH/dx;
          }
          //para qualquer outro lugar, usar diferenciação central
          else{
            float dD = dispMap[i+1,j].x - dispMap[i-1,j].x;
            float dH = hMap[i+1,j] - hMap[i-1,j];
            float dx = 2*dr;
            der_x = dD/dx;
            h_der_x = dH/dx;
          }

          //calcular derivada parcial de Dz em rel a z
          //se tiver nas bordas, usar diferenciação comum de 1 ordem
          if(j == 0){
            float dD = dispMap[i,j+1].y - dispMap[i,j].y;
            float dH = hMap[i,j+1] - hMap[i,j];
            float dz = dr;
            der_z = dD/dz;
            h_der_z = dH/dz;
          }
          //se tiver na outra extremidade, usar diferenciação comum
          else if(j == N-1){
            float dD = dispMap[i,j].y - dispMap[i,j-1].y;
            float dH = hMap[i,j] - hMap[i,j-1];
            float dz = dr;
            der_z = dD/dz;
            h_der_z = dH/dz;
          }
          //para qualquer outro lugar, usar diferenciação central
          else{
            float dD = dispMap[i,j+1].y - dispMap[i,j-1].y;
            float dH = hMap[i,j+1] - hMap[i,j-1];
            float dz = 2*dr;
            der_z = dD/dz;
            h_der_z = dH/dz;
          }

          //calcular derivada parcial de Dx em rel a z
          //sera igual a derivada parcial de Dz em rel a x
          //se tiver nas bordas, usar diferenciação comum de 1 ordem
          if(j == 0){
            float dD = dispMap[i,j+1].x - dispMap[i,j].x;
            float dz = dr;
            der_zx = dD/dz;
          }
          //se tiver na outra extremidade, usar diferenciação comum
          else if(j == N-1){
            float dD = dispMap[i,j].x - dispMap[i,j-1].x;
            float dz = dr;
            der_zx = dD/dz;
          }
          //para qualquer outro lugar, usar diferenciação central
          else{
            float dD = dispMap[i,j+1].x - dispMap[i,j-1].x;
            float dz = 2*dr;
            der_zx = dD/dz;
          }

          //atualizar jacobiano com o valor de cada ponto
          float value = (1+der_x)*(1+der_z) - (der_zx)*(der_zx);
          jacobian[i,j] =Mathf.Clamp01((Jm-value))*2;
          //atualizando textura
          foldingTable.SetPixel(i,j, new Color(jacobian[i,j], jacobian[i,j], jacobian[i,j], 1f));
          //calculando normal
          Vector3 gradient = new Vector3(h_der_x, 0f, h_der_z);
          Vector3 vertical = new Vector3(0f, 1f, 0f);
          Vector3 normal = (vertical - gradient);
          normal.Normalize();
          normalMap[i,j] = normal;
          }
        }
        foldingTable.Apply();
      }



  }
}
