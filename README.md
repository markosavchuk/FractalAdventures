## Fractal Adventures

<b>Mandelbrot set</b> implementation with animated colors using <b>Compute shaders</b> for Mac (Metal) and Windows (DirectX). Multithreading advantage of compute shaders provides excellent performance.

![Screenshots1](https://github.com/markosavchuk/FractalAdventures/blob/main/FractalAdventures/Screenshots/Screenshots4.png)

![Screenshots1](https://github.com/markosavchuk/FractalAdventures/blob/main/FractalAdventures/Screenshots/Screenshots5.png)

![Screenshots1](https://github.com/markosavchuk/FractalAdventures/blob/main/FractalAdventures/Screenshots/Screenshots6.png)

### The floating point problem

Mandelbrot-like fractals need infinite precision for infinite zooming. GPUs are not designed to work with this task so we are usually limited to floating-point precision. Some computer graphic API can provide double float precision (for example DirectX). However, this will significantly reduce performance.

So in this solution, we have two compute shader functions: `CSMainFloat` for float precise calculation and `CSMainDouble` for double float precise calculation. It switches shaders on the spot when it riches floating precise limitation.

### Details

Unity vestion: tested on 2020.1.6 but could be used significant lover version, you need only compute shaders support.

Platforms: Mac (Metal API), Windows (DirectX API)
