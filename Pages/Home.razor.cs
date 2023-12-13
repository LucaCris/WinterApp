using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SkiaSharp;
using SkiaSharp.Views.Blazor;

namespace WinterApp.Pages;

public partial class Home
{
    SKCanvasView? canvasView;
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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender) {
            await JsRuntime.InvokeAsync<object>("initRenderJS", DotNetObjectReference.Create(this));
            timer = new Timer(async (s) => await OnTimer(s), null, 0, 16);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    async Task OnTimer(object? state)
    {
        if (canvasView == null) {
            await Task.Delay(1);
            return;
        }

        if (throttle % 60 == 0)
            FPS = (int)(1.0 / (DateTime.Now - LastRender).TotalSeconds);
        //double factor = (DateTime.Now - LastRender).TotalMilliseconds * 6 / 100;
        LastRender = DateTime.Now;

        if (rnd.NextDouble() < force) {
            theSky.AddFlake(false);
        }
        else if (rnd.NextDouble() < force * 2) {
            theSky.AddFlake(true);
        }

        if (force > minForce)
            force -= rnd.NextDouble() / 1000d;
        else
            force = minForce;

        theSky.Next(windX);

        canvasView.Invalidate();

        throttle++;
    }

    [JSInvokable]
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

    void PaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        // canvas.Clear(SKColors.DarkBlue);

        var paint = new SKPaint
        {
            Shader = SKShader.CreateLinearGradient(
                                new SKPoint(0, 0),
                                new SKPoint(0, theSky.Height),
                                [SKColors.Black, SKColors.DarkBlue],
                                [0, 1],
                                SKShaderTileMode.Clamp)
        };
        canvas.DrawRect(0, 0, theSky.Width, theSky.Height, paint);

        canvas.DrawText($"FPS: {FPS} - OBJS: {theSky.SFList.Count + theSky.BackSFList.Count}", 10, 20, new SKPaint { ColorF = SKColors.Gray });

        var p = new SKPaint { Color = SKColors.FloralWhite, StrokeWidth = 1 };
        foreach (var sf in theSky.BackSFList) {
            canvas.DrawPoint(sf.X, sf.Y, p);
        }

        p = new SKPaint { Color = SKColors.White, StrokeWidth = SnowFlake.Dim };
        foreach (var sf in theSky.SFList) {
            canvas.DrawPoint(sf.X - SnowFlake.Dim2, sf.Y - SnowFlake.Dim2, p);
        }

        foreach (var brick in theSky.BrickList) {
            p = new SKPaint { ColorF = brick.Color };
            canvas.DrawRect(brick.X, brick.Y, brick.W, brick.H, p);
        }

        p = new SKPaint { ColorF = SKColors.White };
        canvas.DrawRect(0, theSky.Floor, theSky.Width, theSky.Height - theSky.Floor, p);
    }
}