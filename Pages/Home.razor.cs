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

        if (rnd.NextDouble() < force) {
            theSky.AddFlake();
        }
        if (force > minForce)
            force -= rnd.NextDouble() / 1000d;
        else
            force = minForce;

        theSky.Next(windX);

        canvasView.Invalidate();
    }

    [JSInvokable]
    public void ResizeInBlazor(double width, double height, double ratio)
    {
        theSky.Resize((float)width, (float)height);
        DPI = (float)ratio;
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
        canvas.Clear(SKColors.DarkBlue);
        //canvas.DrawText($"{force}", 0, 20, new SKPaint { ColorF = SKColors.White });

        var p = new SKPaint { Color = SKColors.White, StrokeWidth = SnowFlake.Dim };
        foreach (var sf in theSky.SFList) {
            canvas.DrawPoint(sf.X - SnowFlake.Dim2, sf.Y - SnowFlake.Dim2, p);
        }

        p = new SKPaint { ColorF = SKColors.White };
        canvas.DrawRect(0, theSky.Floor, theSky.Width, theSky.Height - theSky.Floor, p);
    }
}