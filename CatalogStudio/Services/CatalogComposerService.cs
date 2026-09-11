using CatalogStudio.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
namespace CatalogStudio.Services;
public class CatalogComposerService(FileService files,IConfiguration config) {
 public async Task<string> ComposeAsync(string logo,string model,List<string> variants,int code,AgeRange age,CancellationToken ct,CatalogScene? scene=null,int? seriesCount=null) {
 var paper=Color.ParseHex(scene?.Paper??"F6F1E8");var ink=Color.ParseHex(scene?.Ink??"795B3D");
 using var canvas=new Image<Rgba32>(1400,1100,paper);
 using(var portrait=Image.Load(files.PathFor(model))) {
 portrait.Mutate(p=>p.Resize(new ResizeOptions{Size=new(760,1100),Mode=ResizeMode.Crop,Position=AnchorPositionMode.Center}));
 canvas.Mutate(p=>p.DrawImage(portrait,new Point(0,0),1));
 }
 var edge=new List<PointF>();for(int y=0;y<=1100;y+=10)edge.Add(new(710+35*(float)Math.Sin(y/1100d*Math.PI*2),y));
 var right=edge.Concat(new[]{new PointF(1400,1100),new PointF(1400,0)}).ToArray();
 canvas.Mutate(p=>p.Fill(paper,new Polygon(new LinearLineSegment(right))).DrawLine(ink.WithAlpha(.35f),2,edge.ToArray()));
 // A curved quiet label panel overlays only the reserved bottom-left part of the portrait.
 canvas.Mutate(p=>p.Fill(paper,new EllipsePolygon(-20,1135,1080,520)));
 void Place(string file,int x,int y,int w,int h) {using var image=Image.Load(files.PathFor(file));image.Mutate(p=>p.Resize(new ResizeOptions{Size=new(w,h),Mode=ResizeMode.Max}));canvas.Mutate(p=>p.DrawImage(image,new Point(x+(w-image.Width)/2,y+(h-image.Height)/2),1));}
 using(var cleanLogo=LogoBackgroundService.Prepare(files.PathFor(logo))) { cleanLogo.Mutate(p=>p.Resize(new ResizeOptions{Size=new(300,125),Mode=ResizeMode.Max})); canvas.Mutate(p=>p.DrawImage(cleanLogo,new Point(24,20),1)); }
 int columns=variants.Count<=6?1:2;int rows=(int)Math.Ceiling(variants.Count/(double)columns);int width=640/columns,height=1020/rows;
 for(int i=0;i<variants.Count;i++)Place(variants[i],745+i%columns*width,35+i/columns*height,width-12,height-8);
 // Small botanical accents stay outside product cells.
 canvas.Mutate(p=>p.DrawLine(ink.WithAlpha(.35f),2,new PointF(1375,0),new PointF(1385,145)));
 for(int i=0;i<5;i++)canvas.Mutate(p=>p.Fill(ink.WithAlpha(.18f),new EllipsePolygon(1368+(i%2)*18,20+i*25,12,25)));
 FontFamily family;var path=config["Catalog:FontPath"];if(!string.IsNullOrEmpty(path)){var fonts=new FontCollection();family=fonts.Add(path);}else family=SystemFonts.TryGet("Arial",out var arial)?arial:SystemFonts.Families.First();
 canvas.Mutate(p=>p.DrawText(code.ToString(System.Globalization.CultureInfo.InvariantCulture),family.CreateFont(108),ink,new PointF(55,910)).DrawText(age.ToString(),family.CreateFont(32),ink,new PointF(62,1005)));
 if(seriesCount.HasValue) canvas.Mutate(p=>p.DrawText($"{seriesCount.Value} pieces",family.CreateFont(26),ink,new PointF(62,1050)));
 var result=files.NewPath();await canvas.SaveAsPngAsync(result,ct);return System.IO.Path.GetFileName(result);
 }
}