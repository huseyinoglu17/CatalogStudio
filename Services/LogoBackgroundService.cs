using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
namespace CatalogStudio.Services;

/// <summary>Removes a flat, border-connected logo background without changing the stored original.</summary>
public static class LogoBackgroundService
{
    public static Image<Rgba32> Prepare(string path)
    {
        var image = Image.Load<Rgba32>(path);
        if (image.Width > 1600 || image.Height > 1600)
            image.Mutate(p => p.Resize(new ResizeOptions { Size = new(1600,1600), Mode = ResizeMode.Max }));
        RemoveBackground(image);
        return image;
    }

    public static void RemoveBackground(Image<Rgba32> image)
    {
        int w=image.Width,h=image.Height;
        // Already transparent artwork must retain its existing alpha and white details.
        if (image[0,0].A<240 || image[w-1,0].A<240 || image[0,h-1].A<240 || image[w-1,h-1].A<240) return;
        var corners=new[]{image[0,0],image[w-1,0],image[0,h-1],image[w-1,h-1]};
        var background=corners[0];
        static int Distance(Rgba32 a,Rgba32 b)=>Math.Max(Math.Abs(a.R-b.R),Math.Max(Math.Abs(a.G-b.G),Math.Abs(a.B-b.B)));
        // Only remove a background supported by at least three matching corners.
        var best=corners.OrderByDescending(c=>corners.Count(x=>Distance(c,x)<=18)).First();
        if(corners.Count(x=>Distance(best,x)<=18)<3)return;
        background=best;
        var visited=new bool[w*h];
        var queue=new Queue<int>();
        void Enqueue(int x,int y) {
            if(x<0||y<0||x>=w||y>=h)return;
            int index=y*w+x;if(visited[index])return;
            if(Distance(image[x,y],background)>48)return;
            visited[index]=true;queue.Enqueue(index);
        }
        for(int x=0;x<w;x++){Enqueue(x,0);Enqueue(x,h-1);}
        for(int y=0;y<h;y++){Enqueue(0,y);Enqueue(w-1,y);}
        while(queue.TryDequeue(out var index)) {
            int x=index%w,y=index/w;var pixel=image[x,y];
            int distance=Distance(pixel,background);
            // Feather JPEG/antialias fringe instead of leaving a solid pale halo.
            var alpha=Math.Clamp((distance-18)/30f,0,1);
            if(alpha>0) {
                pixel.R=(byte)Math.Clamp((pixel.R-background.R*(1-alpha))/alpha,0,255);
                pixel.G=(byte)Math.Clamp((pixel.G-background.G*(1-alpha))/alpha,0,255);
                pixel.B=(byte)Math.Clamp((pixel.B-background.B*(1-alpha))/alpha,0,255);
            }
            pixel.A=(byte)(pixel.A*alpha);image[x,y]=pixel;
            Enqueue(x-1,y);Enqueue(x+1,y);Enqueue(x,y-1);Enqueue(x,y+1);
        }
    }
}
