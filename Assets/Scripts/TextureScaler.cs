using UnityEngine;
using UnityEngine.Profiling;

class TextureScaler : System.IDisposable
{
    int width, height;
    RenderTexture renderTexture;

    public TextureScaler(int width, int height)
    {
        this.width = width;
        this.height = height;
        renderTexture = new RenderTexture(width, height, 32); // 32 is the depth
    }

    public Texture2D Scaled(Texture2D src, FilterMode mode = FilterMode.Trilinear)
    {
        Profiler.BeginSample("TextureScaler.Scaled");
        Rect texR = new Rect(0, 0, width, height);
        _gpu_scale(src, mode);

        //Get rendered data back to a new texture
        Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, true);
        result.Resize(width, height);
        result.ReadPixels(texR, 0, 0, true);

        Profiler.EndSample();
        return result;
    }

    public void Scale(Texture2D tex, FilterMode mode = FilterMode.Trilinear)
    {
        Profiler.BeginSample("TextureScaler.Scale");

        Rect texR = new Rect(0, 0, width, height);
        _gpu_scale(tex, mode);

        // Update new texture
        tex.Resize(width, height);
        tex.ReadPixels(texR, 0, 0, true);
        tex.Apply(true);        
        Profiler.EndSample();
    }

    void _gpu_scale(Texture2D src, FilterMode fmode)
    {
        Profiler.BeginSample("TextureScaler.GpuScale");

        src.filterMode = fmode;
        src.Apply(true);

        Graphics.SetRenderTarget(renderTexture);

        GL.LoadPixelMatrix(0, 1, 1, 0);

        //Then clear & draw the texture to fill the entire RTT.
        GL.Clear(true, true, new Color(0, 0, 0, 0));
        Graphics.DrawTexture(new Rect(0, 0, 1, 1), src);
        Profiler.EndSample();
    }

    public void Dispose()
    {
        renderTexture.Release();
    }
}
