using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MandelbrotScript : MonoBehaviour
{
    private const int NumberOfThreads = 16;

    [Header("Shader Objects")]
    [SerializeField] private ComputeShader shader;
    [SerializeField] private RawImage image;
    [SerializeField] private Texture2D colorTexture;

    [Header("Madelbrot Properties")]
    [SerializeField] private float width;
    [SerializeField] private float realStart, imaginaryStart;
    [SerializeField] private int iterations, incrementIterations, maxIterations, minIterations;
    [SerializeField] private float zoomSpeed, moveSpeed, changeColorSpeed;
    [SerializeField] private float maxWidthZoom, minWidthZoom;

    [Header("UI Update")]    
    [SerializeField] private Text zoomInfoText;

    private float height;
    private float zoomDelta;

    private ComputeBuffer buffer;
    private RenderTexture texture;
    private int kernelHangle;

    public struct MandelbrotShaderData
    {
        public float width, height, real, imaginary;
        public int screenWidth, screenHeight;
        public float time;
    }

    MandelbrotShaderData[] mandelbrotShaderdata;

    private void Start()
    {
        InitData();

        UpdateMandelbrot();        
    }

    private void Update()
    {
        ReadInput();

        mandelbrotShaderdata[0].time = Time.time * changeColorSpeed;

        UpdateMandelbrot();
    }

    private void InitData()
    {
        height = width * Screen.height / Screen.width;
        zoomDelta = 1 / width;
        UpdateZoomInfoText();

        mandelbrotShaderdata = new MandelbrotShaderData[1];
        mandelbrotShaderdata[0] = new MandelbrotShaderData
        {
            width = width,
            height = height,
            real = realStart,
            imaginary = imaginaryStart,
            screenWidth = Screen.width,
            screenHeight = Screen.height
        };

        buffer = new ComputeBuffer(mandelbrotShaderdata.Length, 28);
        texture = new RenderTexture(Screen.width, Screen.height, 0);
        texture.enableRandomWrite = true;
        texture.Create();

        kernelHangle = shader.FindKernel("CSMain");

        shader.SetTexture(kernelHangle, "Result", texture);
        shader.SetTexture(kernelHangle, "ColorTexture", colorTexture);
    }

    private void UpdateMandelbrot()
    {
        buffer.SetData(mandelbrotShaderdata);
        shader.SetBuffer(kernelHangle, "buffer", buffer);
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

        mandelbrotShaderdata[0].real = realStart;
        mandelbrotShaderdata[0].imaginary = imaginaryStart;

        return;
    }

    private void Zoom(bool zoomIn)
    {
        if ((zoomIn && width < minWidthZoom) || (!zoomIn && width > maxWidthZoom))
        {
            return;
        }

        UpdateZoomInfoText();

        var factor = zoomIn ? 1 : -1;

        var newIterations = iterations + incrementIterations * factor;
        if (newIterations >= minIterations && newIterations <= maxIterations)
        {
            iterations = newIterations;
        }

        float wFactor = width * zoomSpeed * Time.deltaTime;
        float hFactor = height * zoomSpeed * Time.deltaTime;        

        width -= wFactor * factor;
        height -= hFactor * factor;
        realStart += wFactor / 2.0f * factor;
        imaginaryStart += hFactor / 2.0f * factor;
        
        mandelbrotShaderdata[0].width = width;
        mandelbrotShaderdata[0].height = height;
        mandelbrotShaderdata[0].real = realStart;
        mandelbrotShaderdata[0].imaginary = imaginaryStart;

        return;
    }

    private void OnDestroy()
    {
        buffer.Dispose();
    }

    private void UpdateZoomInfoText()
    {
        zoomInfoText.text = $"Zoom: {string.Format("{0:0.#}", 1 / width / zoomDelta)}";
    }
}
