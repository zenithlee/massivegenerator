// Only works on ARGB32, RGB24 and Alpha8 textures that are marked readable
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace _Massive
{

  public class TextureTools
  {

    public static List<Color32> pal = new List<Color32>();

    public static void SetupTerrainColours()
    {

      TextureTools.pal.Clear();

      //grass medium
      TextureTools.AddPaletteColor(new Color32(76, 110, 58, 255));

      //grass dark
      TextureTools.AddPaletteColor(new Color32(21, 36, 13, 255));

      //Water medium
      TextureTools.AddPaletteColor(new Color32(8, 41, 52, 255));

      //Water dark
      TextureTools.AddPaletteColor(new Color32(40, 70, 82, 255));

      //Sand medium
      TextureTools.AddPaletteColor(new Color32(143, 118, 94, 255));

      //Rock medium
      TextureTools.AddPaletteColor(new Color32(76, 69, 48, 255));

      //snow medium
      TextureTools.AddPaletteColor(new Color32(240, 240, 240, 255));
    }


    public static Texture2D ReducePalette(Texture2D tex)
    {

      Texture2D rt = new Texture2D(tex.width, tex.height);

      Color32[] cs = tex.GetPixels32();

      Color32 tc = new Color32();
      for (int i = 0; i < cs.Length; i++)
      {
        cs[i] = GetNearest(cs[i]);
      }

      rt.SetPixels32(cs);
      rt.Apply();

      return rt;
    }

    public static Color32 GetNearest(Color32 c)
    {
      int closestindex = 0;
      float closestdistance = 99999999999;
      Vector4 vc = new Vector4(c.r, c.g, c.b, c.a);
      for (int i = 0; i < pal.Count; i++)
      {
        Vector4 v = new Vector4(pal[i].r, pal[i].g, pal[i].b, pal[i].a);
        float dist = Vector4.Distance(vc, v);
        if (dist < closestdistance)
        {
          closestdistance = dist;
          closestindex = i;
        }
      }
      return pal[closestindex];
    }

    public static void AddPaletteColor(Color32 c)
    {
      pal.Add(c);
    }

    public static Texture2D DitherImage(Texture2D tex, int amt, int iterations = 1)
    {
      Texture2D t = new Texture2D(tex.width, tex.height);
      t.SetPixels(tex.GetPixels());
      //t.Apply();

      Color[] pixels = tex.GetPixels();
      int offset = 0;
      int noffset = 0;
      for (int i = 0; i < iterations; i++)
      {
        for (int xx = 0; xx < tex.width; xx++)
        {
          for (int yy = 0; yy < tex.height; yy++)
          {
            offset = yy * tex.width + xx;
            int nx = xx + (int)(Random.value * (float)amt);
            int ny = yy + (int)(Random.value * (float)amt);
            noffset = ny * tex.width + nx;
            noffset = Mathf.Clamp(noffset, 0, tex.width * tex.height - 1);
            pixels[offset] = pixels[noffset];

          }
        }
      }

      t.SetPixels(pixels);

      /*

              //TODO: Upgrade to GetPixels
              for (int i = 0; i < iterations; i++)
  {
    for (int xx = 0; xx < tex.width; xx++)
    {
      for (int yy = 0; yy < tex.height; yy++)
      {
        Color c = tex.GetPixel(xx, yy);
        int nx = xx + (int)(Random.value * (float)amt);
        int ny = yy + (int)(Random.value * (float)amt);
        nx = nx > tex.width-1 ? tex.width-1 : nx;
        ny = ny > tex.height - 1 ? tex.height - 1 : ny;
        t.SetPixel(nx, ny, c);
      }
    }
  }
  */
      t.Apply();
      return t;
    }


    public class ThreadData
    {
      public int start;
      public int end;
      public ThreadData(int s, int e)
      {
        start = s;
        end = e;
      }
    }

    private static Color[] texColors;
    private static Color[] newColors;
    private static int w;
    private static float ratioX;
    private static float ratioY;
    private static int w2;
    private static int finishCount;
    private static Mutex mutex;

    public static void Point(Texture2D tex, int newWidth, int newHeight)
    {
      ThreadedScale(tex, newWidth, newHeight, false);
    }

    public static void Bilinear(Texture2D tex, int newWidth, int newHeight)
    {
      ThreadedScale(tex, newWidth, newHeight, true);
    }

    private static void ThreadedScale(Texture2D tex, int newWidth, int newHeight, bool useBilinear)
    {
      texColors = tex.GetPixels();
      newColors = new Color[newWidth * newHeight];
      if (useBilinear)
      {
        ratioX = 1.0f / ((float)newWidth / (tex.width - 1));
        ratioY = 1.0f / ((float)newHeight / (tex.height - 1));
      }
      else
      {
        ratioX = ((float)tex.width) / newWidth;
        ratioY = ((float)tex.height) / newHeight;
      }
      w = tex.width;
      w2 = newWidth;
      var cores = Mathf.Min(SystemInfo.processorCount, newHeight);
      var slice = newHeight / cores;

      finishCount = 0;
      if (mutex == null)
      {
        mutex = new Mutex(false);
      }
      if (cores > 1)
      {
        int i = 0;
        ThreadData threadData;
        for (i = 0; i < cores - 1; i++)
        {
          threadData = new ThreadData(slice * i, slice * (i + 1));
          ParameterizedThreadStart ts = useBilinear ? new ParameterizedThreadStart(BilinearScale) : new ParameterizedThreadStart(PointScale);
          Thread thread = new Thread(ts);
          thread.Start(threadData);
        }
        threadData = new ThreadData(slice * i, newHeight);
        if (useBilinear)
        {
          BilinearScale(threadData);
        }
        else
        {
          PointScale(threadData);
        }
        while (finishCount < cores)
        {
          Thread.Sleep(1);
        }
      }
      else
      {
        ThreadData threadData = new ThreadData(0, newHeight);
        if (useBilinear)
        {
          BilinearScale(threadData);
        }
        else
        {
          PointScale(threadData);
        }
      }

      tex.Resize(newWidth, newHeight, TextureFormat.ARGB32, false);
      tex.SetPixels(newColors);
      tex.Apply();
    }

    public static void BilinearScale(System.Object obj)
    {
      ThreadData threadData = (ThreadData)obj;
      for (var y = threadData.start; y < threadData.end; y++)
      {
        int yFloor = (int)Mathf.Floor(y * ratioY);
        var y1 = yFloor * w;
        var y2 = (yFloor + 1) * w;
        var yw = y * w2;

        for (var x = 0; x < w2; x++)
        {
          int xFloor = (int)Mathf.Floor(x * ratioX);
          var xLerp = x * ratioX - xFloor;
          newColors[yw + x] = ColorLerpUnclamped(ColorLerpUnclamped(texColors[y1 + xFloor], texColors[y1 + xFloor + 1], xLerp),
                               ColorLerpUnclamped(texColors[y2 + xFloor], texColors[y2 + xFloor + 1], xLerp),
                               y * ratioY - yFloor);
        }
      }

      mutex.WaitOne();
      finishCount++;
      mutex.ReleaseMutex();
    }

    public static void PointScale(System.Object obj)
    {
      ThreadData threadData = (ThreadData)obj;
      for (var y = threadData.start; y < threadData.end; y++)
      {
        var thisY = (int)(ratioY * y) * w;
        var yw = y * w2;
        for (var x = 0; x < w2; x++)
        {
          newColors[yw + x] = texColors[(int)(thisY + ratioX * x)];
        }
      }

      mutex.WaitOne();
      finishCount++;
      mutex.ReleaseMutex();
    }

    private static Color ColorLerpUnclamped(Color c1, Color c2, float value)
    {
      return new Color(c1.r + (c2.r - c1.r) * value,
            c1.g + (c2.g - c1.g) * value,
            c1.b + (c2.b - c1.b) * value,
            c1.a + (c2.a - c1.a) * value);
    }

    //used to encode heighmap data
    //note the 255-a inverted alpha is so that PhotoShop and image editors can read the png file
    public static double MassivePixelToDouble(Color c)
    {
      byte a = (byte)(255 - (c.a * 255));
      byte r = (byte)(c.r * 255.0);
      byte g = (byte)(c.g * 255.0);
      byte b = (byte)(c.b * 255.0);
      long rgb = (a << 24 | (b << 16) | (g << 8) | r);
      return (double)rgb;
    }

    //mapbox
    public static float GetAbsoluteHeightFromColor(Color color)
    {
      return (float)(-10000 + ((color.r * 255 * 256 * 256 + color.g * 255 * 256 + color.b * 255) * 0.1));
    }
    //mapbox
    public static float GetAbsoluteHeightFromColor32(Color32 color)
    {
      return (float)(-10000 + ((color.r * 256 * 256 + color.g * 256 + color.b) * 0.1));
    }
    //mapbox
    public static float GetAbsoluteHeightFromColor(float r, float g, float b)
    {
      return (float)(-10000 + ((r * 256 * 256 + g * 256 + b) * 0.1));
    }

    private static double Resolution(int zoom)
    {
      //return InitialResolution / Math.Pow(2, zoom);
      return 1;
    }

    public static double MapZenPixelToDouble(Color32 c)
    {
      return (c.r * 256 + c.g + c.b / 256.0) - 32768.0;
    }
  }
}