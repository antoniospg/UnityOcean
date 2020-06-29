---


---


<h1 id="realistic-ocean-simulation-silvaa">Realistic Ocean Simulation Silvaa</h1>
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
<p><span class="katex--inline"><span class="katex"><span class="katex-mathml"><math><semantics><mrow><mi>y</mi><mo>=</mo><mover accent="true"><mi>a</mi><mo>⃗</mo></mover></mrow><annotation encoding="application/x-tex">y = \vec{a}</annotation></semantics></math></span><span class="katex-html" aria-hidden="true"><span class="base"><span class="strut" style="height: 0.625em; vertical-align: -0.19444em;"></span><span class="mord mathdefault" style="margin-right: 0.03588em;">y</span><span class="mspace" style="margin-right: 0.277778em;"></span><span class="mrel">=</span><span class="mspace" style="margin-right: 0.277778em;"></span></span><span class="base"><span class="strut" style="height: 0.714em; vertical-align: 0em;"></span><span class="mord accent"><span class="vlist-t"><span class="vlist-r"><span class="vlist" style="height: 0.714em;"><span class="" style="top: -3em;"><span class="pstrut" style="height: 3em;"></span><span class="mord"><span class="mord mathdefault">a</span></span></span><span class="" style="top: -3em;"><span class="pstrut" style="height: 3em;"></span><span class="accent-body" style="left: -0.2355em;"><span class="overlay" style="height: 0.714em; width: 0.471em;"><svg width="0.471em" height="0.714em" style="width:0.471em" viewBox="0 0 471 714" preserveAspectRatio="xMinYMin"><path d="M377 20c0-5.333 1.833-10 5.5-14S391 0 397 0c4.667 0 8.667 1.667 12 5
3.333 2.667 6.667 9 10 19 6.667 24.667 20.333 43.667 41 57 7.333 4.667 11
10.667 11 18 0 6-1 10-3 12s-6.667 5-14 9c-28.667 14.667-53.667 35.667-75 63
-1.333 1.333-3.167 3.5-5.5 6.5s-4 4.833-5 5.5c-1 .667-2.5 1.333-4.5 2s-4.333 1
-7 1c-4.667 0-9.167-1.833-13.5-5.5S337 184 337 178c0-12.667 15.667-32.333 47-59
H213l-171-1c-8.667-6-13-12.333-13-19 0-4.667 4.333-11.333 13-20h359
c-16-25.333-24-45-24-59z"></path></svg></span></span></span></span></span></span></span></span></span></span></span></p>
<p>gaiola</p>
<h2 id="references">References</h2>

