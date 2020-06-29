---


---

<h1 id="realistic-ocean-simulation-selva">Realistic Ocean Simulation Selva</h1>
<p><img src="img/overview.png" alt="overview"></p>
<p><strong>Figure 1:</strong> Simulated water surface with a realistic surface shader in a grid of 256x256 and a total size of 100 meters.</p>
<h2 id="introduction">Introduction</h2>
<p>In this project, I will implement the well known statistical wave model from Tessendorf’s paper[1] on simulating height fields for ocean waves.<br>
This approach recreates a piece of the ocean surface from a Fast Fourier Transform (FFT) prescription, with user-controllable size and resolution, and which can be tiled seamlessly over a larger domain.<br>
Each point contains a large sum of sine waves with different amplitudes and phases; however, these functions comes from using statistical-empirical models of the ocean, based on oceanographic research.<br>
In the first part of this article, I will be explaining how to model and animate the structure of ocean surface, while in the second one how to create illumination effects and foam.</p>
<h2 id="statistical-wave-model">Statistical Wave Model</h2>
<p>Statistical models are based on the concept that ocean height h(<strong>x</strong>, t) is a random variable in each horizontal position <strong>x</strong> and time t.  They rely on the fact that we can decompose the height field h(<strong>x</strong>, t) into a sum of sines and cosines, the coefficients that multiply each of these functions are obtained through the Fourier transform, as well as the original height field can be obtained using the inverse transform. The computation uses Fast Fourier Transforms (ffts), which are a rapid method of evaluating the sum.<br>
So, we can write the height field as a sum of time-dependent amplitudes, with complex values.</p>
<p><img src="img/eq1.png" alt=""></p>
<h2 id="references">References</h2>

