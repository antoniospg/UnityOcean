using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FourierUtils;
using System.IO;

namespace Ocean{

  class OceanUtils{
    //constante gravitacional
    const float g = 9.81f;

    //atributos

    //constantes do espectro
    public float A;
    public Vector2 Wind = new Vector2(0f,0f);
    private int N;
    private float L;

    //matrizes pre calculadas
    public Complex[,] ph0;
    public Complex[,] ph0_conj;
    public float[,] disp_table0;
    public float[,] disp_table0_conj;

    //matrizes atualizadas a cada iteracao
    public Complex[,] hMap;
    public Complex [,] dispMap_x;
    public Complex [,] dispMap_z;

    public OceanUtils(float A, int N, float L, Vector2 Wind){
      this.A = A;
      this.N = N;
      this.L = L;
      this.Wind = Wind;

      ph0 = new Complex[N,N];
      ph0_conj = new Complex[N,N];;
      disp_table0 = new float[N,N];
      disp_table0_conj = new float[N,N];

      hMap = new Complex[N,N];
      dispMap_x = new Complex[N,N];
      dispMap_z = new Complex[N,N];

      Init();
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

    public Complex FSpectrum(Vector2 K){

      Vector2 random = NextGaussian();
      Complex rand = new Complex(random.x, random.y);
      float ph = Mathf.Sqrt(PhillSpec(K)/2.0f);
      Complex samp = new Complex(ph*rand.real, ph*rand.img);
      return samp;
    }

    public Vector2 calcWaveVector(int n, int m){

      float Kx = Mathf.PI*2*((2*n - N)/2)/L;
      float Kz = Mathf.PI*2*((2*m - N)/2)/L;
      return new Vector2(Kx, Kz);
    }

    public Complex FAmplitudes(Complex spec, Complex spec_conj,float disp, float t){

      Complex h =  spec*Complex.convertToPolar(1, disp * t)+
                    spec_conj*Complex.convertToPolar(1,disp*(-t));

      return h;
    }

    public void Init(){

      for(int i = 0; i < N; i++){
        for(int j = 0; j < N; j++){

          Vector2 K = calcWaveVector(i, j);
          Vector2 K_conj = calcWaveVector(-i, -j);

          Complex spec = FSpectrum(K);
          Complex spec_conj =FSpectrum(K_conj);
          spec_conj.img *= -1;

          ph0[i,j] = spec;
          ph0_conj[i,j] = spec_conj;

          disp_table0[i,j] = DispRelDeep(K.magnitude);
        }
      }
    }

    public void Sample(float t){

      Complex[,] Kvalues = new Complex[N,N];
      int cont = 0;
      for(int i = 0; i < N; i++){
        for(int j = 0; j < N; j++){

          //heigh map compontents
          Complex spec = ph0[i,j];
          Complex spec_conj =  ph0_conj[i,j];

          float disp = disp_table0[i,j];

          Complex h = FAmplitudes(spec, spec_conj, disp, t);

          Kvalues[i,j] = new Complex(h.real, h.img);

          //dispMap components
        }
      }
      hMap =  getHeightMap(Kvalues);
      getDisplacement(Kvalues);
    }

    public Complex[,] getHeightMap(Complex[,] spec){

      Complex[,] hMap = new Complex[N,N];
      //rows

      for (int i =0; i < N; i++){
        Complex[] fLines = new Complex[N];
        for(int j =0; j< N ; j++){
          fLines[j] = new Complex(spec[i,j].real, spec[i,j].img);
        }

        fLines = Fourier.iFFT(fLines);

        for(int j = 0; j< N; j++){
          hMap[i,j] = fLines[j];
        }

      }
      //collums

      for (int i =0; i < N; i++){
        Complex[] fLines = new Complex[N];

        for(int j =0; j< N ; j++){
          fLines[j] = new Complex(hMap[j,i].real, hMap[j,i].img);
        }

        fLines = Fourier.iFFT(fLines);

        for(int j = 0; j< N; j++){
          hMap[j,i] = fLines[j];
        }
      }
      return hMap;
    }

    //Vector2(x,z)
    public void getDisplacement(Complex[,] spec){
      Complex[,] spec_disp_x = new Complex[N,N];
      Complex[,] spec_disp_z = new Complex[N,N];

      //atualizando a matriz com os valores p a fft
      for(int i =0; i < N; i++){
        for(int j = 0; j< N; j++){
          //calculando unitario do  waveVector
          Vector2 u_waveVector = calcWaveVector(i,j);
          u_waveVector.Normalize();

          //atualizando os valores da matriz
          spec_disp_x[i,j] = spec[i,j]*Complex.convertToPolar((u_waveVector.x),-Mathf.PI);
          spec_disp_z[i,j] = spec[i,j]*Complex.convertToPolar((u_waveVector.y),-Mathf.PI);
        }
      }

      //calculando iFFT nas duas matrizes
      dispMap_x = getHeightMap(spec_disp_x);
      dispMap_z = getHeightMap(spec_disp_z);
    }

  }
}
