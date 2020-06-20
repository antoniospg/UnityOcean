using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FourierUtils{

  class Complex{
    public float real;
    public float img;

    public Complex(){
      this.real = 0f;
      this.img  = 0f;
    }

    public Complex(float real, float img){
      this.real = real;
      this.img = img;
    }

    public string ToString(){
      string txt = real.ToString() + " " + img.ToString() + "j";
      return txt;
    }

    public static Complex convertToPolar(float r, float arg){
      return new Complex (r * Mathf.Cos(arg), r * Mathf.Sin(arg));
    }

    public static Complex operator +(Complex a, Complex b){
      Complex data = new Complex(a. real + b.real , a. img + b.img );
      return data;
     }

     public static Complex operator -(Complex a, Complex b){
       Complex data = new Complex(a. real - b.real , a. img - b.img );
       return data;
     }

     public static Complex operator *(Complex a, Complex b){
       Complex data = new Complex((a. real * b.real ) - (a.img * b.img ),
       (a. real * b.img + (a.img * b.real )));

       return data;
     }

     public float magnitude{
     get {return Mathf.Sqrt(Mathf.Pow(real, 2) + Mathf.Pow(img, 2));}
     }

     public float phase{
     get {return Mathf.Atan( img / real);}
     }

     public static Complex conjugate(Complex a){
       return new Complex(a.real, (-1)*a.img);
     }

     public float Length(){
       return Mathf.Sqrt(this.real*this.real + this.img*this.img);
     }
  }


  class Fourier{

    public static Complex[] FFT(Complex[] x){
        int N = x.Length;
        Complex[] X = new Complex[N];
        Complex[] d, D, e, E;

        if (N == 1){
          X[0] = x[0];
          return X;
        }

        int k;
        e = new Complex[N / 2];
        d = new Complex[N / 2];
        for (k = 0; k < N / 2; k++){
          e[k] = new Complex(x[2*k].real, x[2*k].img);
          d[k] = new Complex(x[2 * k + 1].real, x[2 * k + 1].img);
        }

        D = FFT(d);
        E = FFT(e);

        for (k = 0; k < N / 2; k++){
          Complex temp = Complex.convertToPolar(1, -2 * Mathf.PI * k / N);
          D[k] *= temp;
        }

        for (k = 0; k < N / 2; k++){
          X[k] = E[k] + D[k];
          X[k + N / 2] = E[k] - D[k];
        }

        return X;
      }

      public static Complex[] iFFT(Complex[] x){
          int N = x.Length;
          Complex[] X = new Complex[N];
          Complex[] d, D, e, E;
          if (N == 1){
            X[0] = x[0];
            return X;
          }
          int k;
          e = new Complex[N / 2];
          d = new Complex[N / 2];
          for (k = 0; k < N / 2; k++){
            e[k] = x[2 * k];
            d[k] = x[2 * k + 1];
          }
          D = iFFT(d);
          E = iFFT(e);

          for (k = 0; k < N / 2; k++){
            Complex temp = Complex.convertToPolar(1, 2 * Mathf.PI * k / N);
            D[k] *= temp;
          }

          for (k = 0; k < N / 2; k++){
            X[k] = E[k] + D[k];
            X[k + N / 2] = E[k] - D[k];
          }
          //COLOCAR O 1/N DPS
          return X;
        }
  }
}
