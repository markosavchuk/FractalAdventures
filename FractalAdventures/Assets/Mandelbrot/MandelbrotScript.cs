using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MandelbrotScript : MonoBehaviour
{
    private const int NumberOfThreads = 16;

    private const string FloatKernel = "CSMainFloat";
    private const string DoubleKernel = "CSMainDouble";


    [Header("Shader Objects")]
    [SerializeField] private ComputeShader shader;
    [SerializeField] private RawImage image;
    [SerializeField] private Texture2D colorTexture;

    [Header("Madelbrot Properties")]
    [SerializeField] private double width;
    [SerializeField] private double realStart, imaginaryStart;
    [SerializeField] private int iterations, incrementIterations, maxIterations, minIterations;
    [SerializeField] private double zoomSpeed, moveSpeed, changeColorSpeed;
    [SerializeField] private double maxWidthZoom, minWidthFloatZoom, minWidthDoubleZoom;

    [Header("UI Update")]    
    [SerializeField] private Text zoomInfoText;

    private double height;
    private double zoomDelta;

    private ComputeBuffer buffer;
    private RenderTexture texture;

    private int kernelHangle;

    private bool isFloatKernelActive= true;

    public struct MandelbrotDoubleShaderData
    {
        public double width, height, real, imaginary;
        public int screenWidth, screenHeight;
        public double time;
    }

    MandelbrotDoubleShaderData[] mandelbrotDoubleShaderData;

    public struct MandelbrotFloatShaderData
    {
        public float width, height, real, imaginary;
        public int screenWidth, screenHeight;
        public float time;
    }

    MandelbrotFloatShaderData[] mandelbrotFloatShaderData;

    private void Start()
    {
        InitData();

        UpdateMandelbrot();        
    }

    private void Update()
    {
        ReadInput();

        if (isFloatKernelActive)
        {
            mandelbrotFloatShaderData[0].time = (float)(Time.time * changeColorSpeed);
        }
        else
        {
            mandelbrotDoubleShaderData[0].time = Time.time * changeColorSpeed;
        }

        UpdateMandelbrot();
    }

    private void InitData()
    {
        height = width * Screen.height / Screen.width;
        zoomDelta = 1 / width;
        UpdateZoomInfoText();

        texture = new RenderTexture(Screen.width, Screen.height, 0);
        texture.enableRandomWrite = true;
        texture.Create();

        InitFloatKernel();
    }

    private void InitFloatKernel()
    {
        isFloatKernelActive = true;

        mandelbrotFloatShaderData = new MandelbrotFloatShaderData[1];
        mandelbrotFloatShaderData[0] = new MandelbrotFloatShaderData
        {
            width = (float)width,
            height = (float)height,
            real = (float)realStart,
            imaginary = (float)imaginaryStart,
            screenWidth = Screen.width,
            screenHeight = Screen.height
        };

        buffer = new ComputeBuffer(mandelbrotFloatShaderData.Length, 28);

        kernelHangle = shader.FindKernel(FloatKernel);

        shader.SetTexture(kernelHangle, "Result", texture);
        shader.SetTexture(kernelHangle, "ColorTexture", colorTexture);
    }

    private void InitDoubleKernel()
    {
        isFloatKernelActive = false;

        mandelbrotDoubleShaderData = new MandelbrotDoubleShaderData[1];
        mandelbrotDoubleShaderData[0] = new MandelbrotDoubleShaderData
        {
            width = width,
            height = height,
            real = realStart,
            imaginary = imaginaryStart,
            screenWidth = Screen.width,
            screenHeight = Screen.height
        };

        buffer = new ComputeBuffer(mandelbrotFloatShaderData.Length, 48);

        kernelHangle = shader.FindKernel(DoubleKernel);

        shader.SetTexture(kernelHangle, "Result", texture);
        shader.SetTexture(kernelHangle, "ColorTexture", colorTexture);
    }

    private void UpdateMandelbrot()
    {
        if (isFloatKernelActive)
        {
            buffer.SetData(mandelbrotFloatShaderData);
        }
        else
        {
            buffer.SetData(mandelbrotDoubleShaderData);
        }

        shader.SetBuffer(kernelHangle, isFloatKernelActive ? "bufferFloat" : "bufferDouble", buffer);
        shader.SetInt("maxIteration", iterations);

        shader.Dispatch(kernelHangle, Screen.width / NumberOfThreads, Screen.height / NumberOfThreads, 1);

        RenderTexture.active = texture;
        image.material.mainTexture = texture;
    }

    private void ReadInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            Zoom(true);
        }

        if (Input.GetKey(KeyCode.Backspace))
        {
            Zoom(false);
        }

        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            MoveScreen(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }
    }

    private void MoveScreen(float horizon, float vertical)
    {
        var speed = moveSpeed * width * Time.deltaTime;

        realStart += horizon * speed;
        imaginaryStart += vertical * speed;

        if (isFloatKernelActive)
        {
            mandelbrotFloatShaderData[0].real = (float)realStart;
            mandelbrotFloatShaderData[0].imaginary = (float)imaginaryStart;
        }
        else
        {
            mandelbrotDoubleShaderData[0].real = realStart;
            mandelbrotDoubleShaderData[0].imaginary = imaginaryStart;
        }

        return;
    }

    private void Zoom(bool zoomIn)
    {
        if (isFloatKernelActive)
        {
            if (!zoomIn && width > maxWidthZoom)
            {
                return;
            }
            else if (zoomIn && width < minWidthFloatZoom && CanUseDoubleInShader())
            {
                InitDoubleKernel();
            }
        }      
        
        if (!isFloatKernelActive)
        {
            if (zoomIn && width < minWidthDoubleZoom)
            {
                return;
            }
            else if (!zoomIn && width > minWidthFloatZoom)
            {
                InitFloatKernel();
            }
        }

        UpdateZoomInfoText();

        var factor = zoomIn ? 1 : -1;

        var newIterations = iterations + incrementIterations * factor;
        if (newIterations >= minIterations && newIterations <= maxIterations)
        {
            iterations = newIterations;
        }

        double wFactor = width * zoomSpeed * Time.deltaTime;
        double hFactor = height * zoomSpeed * Time.deltaTime;        

        width -= wFactor * factor;
        height -= hFactor * factor;
        realStart += wFactor / 2.0f * factor;
        imaginaryStart += hFactor / 2.0f * factor;

        if (isFloatKernelActive)
        {
            mandelbrotFloatShaderData[0].width = (float)width;
            mandelbrotFloatShaderData[0].height = (float)height;
            mandelbrotFloatShaderData[0].real = (float)realStart;
            mandelbrotFloatShaderData[0].imaginary = (float)imaginaryStart;
        }
        else
        {
            mandelbrotDoubleShaderData[0].width = width;
            mandelbrotDoubleShaderData[0].height = height;
            mandelbrotDoubleShaderData[0].real = realStart;
            mandelbrotDoubleShaderData[0].imaginary = imaginaryStart;
        }

        return;
    }

    private void OnDestroy()
    {
        buffer?.Dispose();
    }

    private void UpdateZoomInfoText()
    {
        var zoom = 1 / width / zoomDelta;
        zoomInfoText.text = $"Zoom: {string.Format(zoom < 10 ? "{0:0.#}" : "{0:0}", 1 / width / zoomDelta)}";
    }

    private bool CanUseDoubleInShader()
    {
        return SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Metal;
    }
}
