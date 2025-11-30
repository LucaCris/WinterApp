using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SkiaSharp;
using SkiaSharp.Views.Blazor;

namespace WinterApp.Pages;

public partial class Home
{
    private SKGLView? glView;

    private Timer? timer;
    readonly Random rnd = new();
    readonly Sky theSky = new();
    float windX = 0;
    const double minForce = 0.1;
    double force = minForce;
    public float DPI;
    private DateTime LastRender;
    int FPS;
    int throttle;
    bool SnowMode = true;
    private float prevW;
    private float prevH;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender) {
            timer = new Timer(async (s) => await OnTimer(s), null, 0, 16);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    async Task OnTimer(object? state)
    {
        if (glView == null) {
            await Task.Yield();
            return;
        }

        if (throttle % 60 == 0)
            FPS = (int)(1.0 / (DateTime.Now - LastRender).TotalSeconds);
        //double factor = (DateTime.Now - LastRender).TotalMilliseconds * 6 / 100;
        LastRender = DateTime.Now;

        if (SnowMode) {
            if (rnd.NextDouble() < force) {
                theSky.AddFlake(false);
            } else if (rnd.NextDouble() < force * 2) {
                theSky.AddFlake(true);
            }
        } else {
            theSky.AddDrop();
        }

        if (force > minForce)
            force -= rnd.NextDouble() / 1000d;
        else
            force = minForce;

        theSky.Next(windX, SnowMode);
        throttle++;
    }

    public void ResizeInBlazor(double width, double height, double ratio)
    {
        theSky.Resize((float)width, (float)height);
        DPI = (float)ratio;
        throttle = 0;
    }

    public void OnMouseMove(MouseEventArgs e)
    {
        windX = ((float)e.OffsetX * DPI - theSky.Width / 2) / theSky.Width;
    }

    public void OnMouseClick()
    {
        force = minForce * 4;
    }

    public void OnSwitch()
    {
        SnowMode = !SnowMode;
    }

    private void GLPaintSurface(SKPaintGLSurfaceEventArgs args)
    {
        var canvas = args.Surface.Canvas;

        var W = canvas.DeviceClipBounds.Width;
        var H = canvas.DeviceClipBounds.Height;

        if (W != prevW || H != prevH) {
            ResizeInBlazor(W, H, DPI);
            prevW = W;
            prevH = H;
        }

        theSky.Draw(canvas);

        // Create a font for text rendering
        using var font = new SKFont();

        // Draw FPS and object count
        canvas.DrawText(
            $"FPS: {FPS} - OBJS: {theSky.SFList.Count + theSky.BackSFList.Count + theSky.DropList.Count}",
            10, 20,
            SKTextAlign.Left,
            font,
            new SKPaint { ColorF = SKColors.Gray }
        );

        //canvas.DrawText($"SEASON GREETINGS FROM COMMODORE", 180, 65, new SKPaint { ColorF = SKColors.White });

        // Draw author text
        canvas.DrawText(
            $"BY LUCA C. 2025/26",
            theSky.Width - 140, theSky.Height - 15,
            SKTextAlign.Left,
            font,
            new SKPaint { ColorF = SKColors.White }
        );
    }
}
