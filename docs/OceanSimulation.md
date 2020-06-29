---


---

<h1 id="realistic-ocean-simulation-selva">Realistic Ocean Simulation Selva</h1>
<p><img src="img/overview.png" alt="overview"></p>
<p><strong>Figure 1:</strong> Simulated water surface with a realistic surface shader in a grid of 256x256 and a total size of 100 meters.</p>
<h2 id="introduction">Introduction</h2>
<p>In this project, I will implement the well known statistical wave model from Tessendorf’s paper[1] on simulating height fields for ocean waves.<br>
This approach recreates a piece of the ocean surface from a Fast Fourier Transform (FFT) prescription, with user-controllable size and resolution, and which can be tiled seamlessly over a larger domain.<br>
Each point contains a large sum of sine waves with different amplitudes and phases; however, these functions comes from using statistical-empirical models of the ocean, based on oceanographic research.</p>
<p>In the first part of this article, I will be explaining how to model and animate the structure of ocean surface, while in the second one how to create illumination effects and foam.</p>
<h2 id="references">References</h2>

