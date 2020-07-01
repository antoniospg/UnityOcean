<script src="https://cdn.mathjax.org/mathjax/latest/MathJax.js?config=TeX-AMS-MML_HTMLorMML" type="text/javascript"></script>

# Realistic Ocean Simulation Silvaa
![overview](img/overview.png)

**Figure 1:** Simulated water surface with a realistic surface shader in a grid of 256x256 and a total size of 100 meters. 

## Introduction
In this project, I will implement the statistical wave model from the equations in Tessendorf's paper[1] on simulating ocean water. The ocean rendering technique in this sample applies heightmap generation by summing a vast number of waves using the Fast Fourier Transform, with user-controllable size and resolution, and which can be tiled seamlessly over a larger domain.
The main principle of Ocean rendering is that it can be modeled very well by thinking of it a sum of "infinite" waves at different amplitudes traveling in different directions. These waves aren't randomly chosen; it comes from using statistical-empirical models of the ocean, based on oceanographic research. In this article, I will show how to recreate and animate the ocean surface and add critical visual features like foam and illumination.

## Waves and the Fourier Tansform
This technique consists of doing an inverse Fourier transform on the frequency spectrum of the ocean height field to get the space domain representation of this same field, at each time frame. For every frame, we calculate, for each point (x,z) in a rectangular grid, the y component of this point, which represents the ocean height at that point.
Given the ocean height field function in the spatial frequency domain $$ \tilde h(\pmb{k}, t) $$, to find the original function in the spatial domain, we need to perform the inverse Fourier Transform, that is, evaluate the integral above (note that we supress the twiddle factor):

$$
\int_{-\infty}^{\infty}  \tilde h(\pmb{k},t).exp(i\pmb{k.x})d\pmb{k}
$$

To perform this calculation, its necessary to sample the signal in a discreate interval over a support domain. So the integral becomes a summation, expressed by:

$$
h(\pmb{x}, t) = \sum_{\pmb{k}} \tilde h(\pmb{x},t).exp(i\pmb{k.x})
$$

where **k** is the wave vector, and can be defined as:

$$
\pmb{k} = (k_x, k_z)
\\~\\
k_x = \frac{2\pi n}{L_x}
\\~\\
k_z = \frac{2\pi m}{L_z}
$$

where:

$$
-\frac{N}{2} \leq  n \leq \frac{N}{2}
\\~\\
-\frac{M}{2} \leq  m \leq \frac{M}{2}
$$

but, for most of our work, we deal with variables i and j in different domains:

$$
0 \leq i <N
\\
0 \leq j <M
$$

so, we can make the transformation:

$$
n = i - \frac{N}{2}
\\~\\
m = j - \frac{M}{2}
$$

The fft process generates the height field at discrete points:

$$
 \pmb{x} = (\frac{nL_x}{N}, \frac{mL_z}{M})
$$

## The FFT
To compute the Fourier Transform, we need to calculate the summation in the previous section; this requires a complexity of O(n²) for a 1-D Fourier Transform, which is terrible, especially when we need to perform 2-D operations in a real-time system. The best approach is using the FFT(Fast Fourier Transform) algorithm, which implements the calculation with an O(nlog(n)) complexity for 1-D data. The algorithm is quite complicated, but essentially, for a RADIX-2 FFT, it splits recursively into half the data, performing calculations using the cyclic property of the n-th roots of unity, this way, avoiding unnecessary calculations. Above is a simple example of the algorithm for 1-D data with eight elements; the peculiar structure of this graph also gives the name the Butterfly algorithm to the FFT.

![](img/fftsample.gif) 
**Figure 2:** Butterfly Algorithm.

At this point, we can ask ourselves how big the grid should be? The answer is that it depends if you make these calculations on the GPU or CPU. On GPU, especially implementing it on a shader, the calculations can be made much faster due to the massive parallelization power of the GPU, for those, in a real-time system, a grid between 128x128 and 512x512 is enough. If you want to do this in the CPU, the grid's resolution can be quite limited. In my implementation, 64x64 was the best resolution I could get. 





$$
P_h(\pmb{k}) = \langle |\tilde h^*(\pmb{k},t)|^2 \rangle
$$

$$
P_h(\pmb{k}) = A\frac{exp(-1/(kL)^2)}{k^4} |\hat{\pmb{k}}.\hat{\pmb{v}}|^2
$$



$$
h(\pmb{x}, t) = \sum_{\pmb{k}} \tilde h(\pmb{x},t).exp(i\pmb{k.x})
$$



# Useful stuff
$$
exp(-(kl)²)
$$
$$
\omega(k) = \sqrt{gk}
$$
$$
\tilde h_o(\pmb{k}) = \frac{(\xi_r + i\xi_i)}{\sqrt{2}} \sqrt{P_h(\pmb{k})}
$$
$$
\tilde h(\pmb{k},t) = \tilde h_o(\pmb{k})exp(i\omega(\pmb{k})t) + \tilde h_o^*(\pmb{-k})exp(i\omega(\pmb{k})t)
$$
$$
\pmb{D}(\pmb{x},t) = \sum_{\pmb{k}}-i\frac{\pmb{k}}{k}\tilde h(\pmb{k},t)exp(i\pmb{k.x})
$$
$$
\pmb{x_f} = \pmb{x} + \lambda\pmb{D}(\pmb{x},t)
$$
$$
J_{xx} = 1 + \lambda\frac{\partial{\pmb{D}_x(\pmb{x})}}{\partial{x}}
\\~\\
J_{zz} = 1 + \lambda\frac{\partial{\pmb{D}_z(\pmb{x})}}{\partial{z}}
\\~\\
 J_{xz} = J_{{zx}} = \lambda\frac{\partial{\pmb{D}_x(\pmb{x})}}{\partial{z}}
$$
$$
J(\pmb{x}) = J_{xx}J_{zz} - J_{xz}J_{zx}
\\
J < 0
$$
$$
\epsilon(\pmb{x},t) = \nabla h(\pmb{x},t)
$$
$$
\hat{n}_s(\pmb{x},t) = \frac{\hat{y} - \epsilon(\pmb{x},t)}{\sqrt{1+\epsilon^2(\pmb{x},t)}}
$$
$$
\hat{n}_r(\pmb{x},t) = \hat{n}_i - 2\hat{n}_s(\pmb{x},t)(\hat{n}_s(\pmb{x},t).\hat{n}_i)
$$
$$
sin(\theta_i) = |\hat{n}_i\times\hat{n}_s|
$$
$$
sin(\theta_t) = |\hat{n}_t\times\hat{n}_s|
$$
$$
n_t sin(\theta_t) = n_i sin(\theta_i) 
$$
$$
R+T=1
$$
$$
R(\hat{n}_i, \hat{n}_r) = \frac{1}{2}\{  \frac{sin^2(\theta_t+\theta_i) }{sin^2(\theta_t-\theta_i)} + \frac{tan^2(\theta_t+\theta_i) }{tan^2(\theta_t-\theta_i)} \}
$$
$$
R_o = (\frac{n_1-n_2}{n_1+n_2})^2
$$
$$
R(\theta) = R_o + (1-R_o)(1-cos(\theta))^5
$$

## References
