<script type="text/javascript" src="http://cdn.mathjax.org/mathjax/latest/MathJax.js?config=TeX-AMS-MML_HTMLorMML"></script>

# Realistic Ocean Simulation Silvaa
![overview](img/overview.png)

**Figure 1:** Simulated water surface with a realistic surface shader in a grid of 256x256 and a total size of 100 meters. 

## Introduction
In this project, I will implement the well known statistical wave model from Tessendorf's paper[1] on simulating height fields for ocean waves.
This approach recreates a piece of the ocean surface from a Fast Fourier Transform (FFT) prescription, with user-controllable size and resolution, and which can be tiled seamlessly over a larger domain.
Each point contains a large sum of sine waves with different amplitudes and phases; however, these functions comes from using statistical-empirical models of the ocean, based on oceanographic research. 
In the first part of this article, I will be explaining how to model and animate the structure of ocean surface, while in the second one how to create illumination effects and foam. 

## Statistical Wave Model
Statistical models are based on the concept that ocean height h(**x**, t) is a random variable in each horizontal position **x** and time t.  They rely on the fact that we can decompose the height field h(**x**, t) into a sum of sines and cosines, the coefficients that multiply each of these functions are obtained through the Fourier transform, as well as the original height field can be obtained using the inverse transform. The computation uses Fast Fourier Transforms (ffts), which are a rapid method of evaluating the sum.
So, we can write the height field as a sum of time-dependent amplitudes, with complex values.

![](img/eq1.png)

$y = \vec{a}$

gaiola

## References
