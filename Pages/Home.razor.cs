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
    bool SnowMode = true;

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

        if (SnowMode) {
            if (rnd.NextDouble() < force) {
                theSky.AddFlake(false);
            }
            else if (rnd.NextDouble() < force * 2) {
                theSky.AddFlake(true);
            }
        }
        else {
            theSky.AddDrop();
        }

        if (force > minForce)
            force -= rnd.NextDouble() / 1000d;
        else
            force = minForce;

        theSky.Next(windX, SnowMode);

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

    public void OnSwitch()
    {
        SnowMode = !SnowMode;
    }

    void PaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;

        theSky.Draw(e.Surface.Canvas);

        canvas.DrawText($"FPS: {FPS} - OBJS: {theSky.SFList.Count + theSky.BackSFList.Count + theSky.DropList.Count}", 10, 20, new SKPaint { ColorF = SKColors.Gray });
        //canvas.DrawText($"SEASON GREETINGS FROM COMMODORE", 130, 65, new SKPaint { ColorF = SKColors.White });
        //canvas.DrawText($"BY LUCA C. 2023/24", theSky.Width - 140, theSky.Height - 15, new SKPaint { ColorF = SKColors.White });
    }
}
