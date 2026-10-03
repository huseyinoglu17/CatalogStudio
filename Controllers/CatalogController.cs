using CatalogStudio.Data;
using CatalogStudio.Models;
using CatalogStudio.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
namespace CatalogStudio.Controllers;
[Authorize]
public class CatalogController(AppDbContext db,FileService files,UserPreferenceService prefs,OpenAiImageService ai,PromptBuilderService prompts,CatalogComposerService composer,CatalogSceneService scenes,TokenService tokens,PricingService pricing,CatalogRetentionService retention,ILogger<CatalogController> logger):Controller {
 int UserId=>int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 string Copy(string source){var target=files.NewPath();System.IO.File.Copy(files.PathFor(source),target);return Path.GetFileName(target);}
 [HttpPost,RequestSizeLimit(215*1024*1024)]
 public async Task<IActionResult> Create(CatalogRequest m,CancellationToken ct) {
 var pref=await prefs.GetAsync(UserId);ViewBag.Preference=pref;
 var colorPrice=await pricing.GetColorPrice();ViewBag.ColorTokenPrice=colorPrice;
 if(m.QuotedColorTokenPrice.HasValue && m.QuotedColorTokenPrice!=colorPrice)
     ModelState.AddModelError("","Renk başına token fiyatı değişti. Güncel fiyatı kontrol ederek formu yeniden gönderin.");
 if(ModelState.TryGetValue(nameof(m.QuotedColorTokenPrice),out var quoteState) && quoteState.Errors.Count>0)
     ModelState.AddModelError("","Token fiyatı geçersiz. Güncel fiyatı kontrol ederek formu yeniden gönderin.");
 ModelState.Remove(nameof(m.QuotedColorTokenPrice));m.QuotedColorTokenPrice=colorPrice;
 if(m.UseSavedLogo&&pref==null)ModelState.AddModelError("Logo","Kayıtlı logo bulunamadı.");
 if(!m.UseSavedLogo&&m.Logo==null)ModelState.AddModelError("Logo","Logo yükleyin veya kayıtlı logoyu onaylayın.");
 if(!ModelState.IsValid)return View("~/Views/Home/Index.cshtml",m);
 var uploaded=new List<string>();
 try {
 var cost=m.ColorCount!.Value*colorPrice;
 if(await tokens.Balance(UserId)<cost)throw new InvalidOperationException($"Yetersiz token. {m.ColorCount} renk için {cost} token gerekir.");
 var logo=m.UseSavedLogo?pref!.LogoPath:await files.SaveAsync(m.Logo!,ct);if(!m.UseSavedLogo)uploaded.Add(logo);
 var raw=new List<string>(); string main;
 if(m.BackPrint){
 foreach(var front in m.FrontImages){var name=await files.SaveAsync(front,ct);raw.Add(name);uploaded.Add(name);}
 main=await files.SavePairAsync(m.FrontImages[0],m.BackImages[0],ct);uploaded.Add(main);
 }else{
 main=await files.SaveAsync(m.MainModelProductImage!,ct);uploaded.Add(main);
 foreach(var file in m.VariantImages){var name=await files.SaveAsync(file,ct);raw.Add(name);uploaded.Add(name);}
 }
 var snapshot=Copy(logo);uploaded.Add(snapshot);
 var detail="";
 if(m.EmbroideryDetailImage!=null){detail=await files.SaveAsync(m.EmbroideryDetailImage,ct);uploaded.Add(detail);}
 await prefs.SaveAsync(UserId,m.BrandName,logo);uploaded.Remove(logo);
 var catalog=new Catalog{WhiteOutputWidth=m.WhiteCustomSize?m.WhiteOutputWidth!.Value:1400,WhiteOutputHeight=m.WhiteCustomSize?m.WhiteOutputHeight!.Value:1100,DecorationType=m.DecorationType,DetailImagePath=detail,ColorTokenPrice=colorPrice,OutputWidth=m.CustomSize?m.OutputWidth!.Value:1400,OutputHeight=m.CustomSize?m.OutputHeight!.Value:1100,BackPrint=m.BackPrint,UserId=UserId,BrandName=m.BrandName,LogoSnapshotPath=snapshot,VariantPathsJson=JsonSerializer.Serialize(raw),MainProductImagePath=main,ProductCode=m.ProductCode,MinimumAge=m.MinimumAge,MaximumAge=m.MaximumAge,AgeUnit=m.AgeUnit,ModelGender=m.ModelGender,HasPockets=m.HasPockets,ModelAge=m.ModelAge,ModelAgeUnit=m.ModelAgeUnit,SeriesCount=m.SeriesCount};
 await Produce(catalog,raw,cost,ct);uploaded.Clear();
 return RedirectToAction("Result",new{id=catalog.Id});
 }catch(Exception e)when(e is not OutOfMemoryException){logger.LogWarning("Catalog failed: {Type}",e.GetType().Name);ModelState.AddModelError("",e is InvalidOperationException?e.Message:"Görsel oluşturulamadı. Ayrılan tokenlar iade edildi.");return View("~/Views/Home/Index.cshtml",m);}
 finally{foreach(var name in uploaded)files.Delete(name);}
 }
 [HttpPost] public async Task<IActionResult> Regenerate(int id,RegenerationRequest request,CancellationToken ct) {
 var original=await db.Catalogs.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id&&x.UserId==UserId);
 if(original==null)return NotFound();
 ViewBag.Regeneration=request;
 if(!ModelState.IsValid)return View("Result",original);
 var effectiveChanges=string.IsNullOrWhiteSpace(original.GenerationInstructions)?request.Changes.Trim():original.GenerationInstructions+"\n\nLATEST REQUEST (takes precedence where it conflicts with earlier corrections):\n"+request.Changes.Trim();
 if(effectiveChanges.Length>12000){ModelState.AddModelError("","Bu kataloğun değişiklik geçmişi doldu. Yeni katalog formundan ürün fotoğraflarını tekrar yükleyin.");return View("Result",original);}
 if(string.IsNullOrEmpty(original.LogoSnapshotPath)||string.IsNullOrEmpty(original.MainProductImagePath)){TempData["Message"]="Bu eski kataloğun kaynak fotoğrafları saklanmamış. Yeni katalog formundan fotoğrafları yükleyin.";return RedirectToAction("Result",new{id});}
 var copies=new List<string>();
 try {
 if(await tokens.Balance(UserId)<25)throw new InvalidOperationException("Yeniden üretim için 25 token gerekir.");
 string Clone(string name){var copy=Copy(name);copies.Add(copy);return copy;}
 var raw=(JsonSerializer.Deserialize<List<string>>(original.VariantPathsJson)??[]).Select(Clone).ToList();
 var catalog=new Catalog{WhiteOutputWidth=original.WhiteOutputWidth,WhiteOutputHeight=original.WhiteOutputHeight,DecorationType=original.DecorationType,DetailImagePath=string.IsNullOrEmpty(original.DetailImagePath)?"":Clone(original.DetailImagePath),RegenerationReason=request.Reason.Trim(),RegenerationChanges=request.Changes.Trim(),GenerationInstructions=effectiveChanges,ColorTokenPrice=original.ColorTokenPrice,OutputWidth=original.OutputWidth,OutputHeight=original.OutputHeight,BackPrint=original.BackPrint,UserId=UserId,BrandName=original.BrandName,LogoSnapshotPath=Clone(original.LogoSnapshotPath),MainProductImagePath=Clone(original.MainProductImagePath),VariantPathsJson=JsonSerializer.Serialize(raw),ProductCode=original.ProductCode,MinimumAge=original.MinimumAge,MaximumAge=original.MaximumAge,AgeUnit=original.AgeUnit,ModelGender=original.ModelGender,HasPockets=original.HasPockets,ModelAge=original.ModelAge,ModelAgeUnit=original.ModelAgeUnit,SeriesCount=original.SeriesCount};
 await Produce(catalog,raw,25,ct);copies.Clear();return RedirectToAction("Result",new{id=catalog.Id});
 }catch(Exception e)when(e is not OutOfMemoryException){logger.LogWarning("Regeneration failed: {Type}",e.GetType().Name);ModelState.AddModelError("",e is InvalidOperationException?e.Message:"Yeniden üretim tamamlanamadı. Ayrılan tokenlar iade edildi.");return View("Result",original);}
 finally{foreach(var name in copies)files.Delete(name);}
 }
 async Task Produce(Catalog catalog,List<string> raw,int cost,CancellationToken ct) {
 string? reservation=null;var scratch=new List<string>();
 try {
 reservation=await tokens.Reserve(UserId,cost);
 var scene=scenes.Next(UserId);using var gate=new SemaphoreSlim(3);
 IReadOnlyList<string> References(string name)=>string.IsNullOrEmpty(catalog.DetailImagePath)?[name]:[name,catalog.DetailImagePath];
 var detailRule=string.IsNullOrEmpty(catalog.DetailImagePath)?"":" Image 1 is the garment/color reference. Image 2 is an artwork/material close-up: copy its embroidery construction and stitch relief only, retaining Image 1's garment color and artwork placement. Never use the close-up's background or its different garment color.";
 var correction=PromptBuilderService.Correction(catalog);
 var quality=catalog.DecorationType is DecorationType.Embroidery or DecorationType.Mixed || !string.IsNullOrEmpty(catalog.DetailImagePath)?"high":null;
 async Task<string> Generate(IReadOnlyList<string> names,string prompt,string size){
 await gate.WaitAsync(ct);try{var output=await ai.EditAsync(names,prompt,ct,size,quality);lock(scratch)scratch.Add(output);return output;}finally{gate.Release();}
 }
 var displayColors=(catalog.BackPrint ? raw.Skip(1) : raw).ToList();
 var variantTasks=displayColors.Select(name=>Generate(References(name),prompts.Variant(scene,catalog.HasPockets)+(catalog.BackPrint?PromptBuilderService.BackVariant:"")+PromptBuilderService.DecorationRule(catalog.DecorationType)+detailRule+correction,"1536x768")).ToArray();
 // All colors, including the first/model color, have their own clean white-background model photo.
 var allColors=catalog.BackPrint
     ? new[]{catalog.MainProductImagePath}.Concat(raw.Skip(1)).ToList()
     : new[]{catalog.MainProductImagePath}.Concat(raw).ToList();
 var whiteTasks=allColors.Select((name,index)=>Generate(References(name),prompts.WhiteModel(catalog,catalog.BackPrint && (index==0 || catalog.MainProductImagePath==name))+(catalog.BackPrint && index>0 ? " If this is a legacy two-panel reference sheet, use only its LEFT front half." : "")+detailRule+correction,"1024x1536")).ToArray();
 // The branded scene edits the same first-color white photograph instead of independently interpreting the garment.
 async Task<string> GenerateScene(){
     var canonical=await whiteTasks[0];
     IReadOnlyList<string> references=string.IsNullOrEmpty(catalog.DetailImagePath)
         ?[canonical,catalog.MainProductImagePath]:[canonical,catalog.MainProductImagePath,catalog.DetailImagePath];
     return await Generate(references,prompts.SceneFromWhite(catalog,scene)+correction,displayColors.Count==0?"1536x1024":"1024x1536");
 }
 var modelTask=GenerateScene();
 await Task.WhenAll(variantTasks.Concat(whiteTasks).Prepend(modelTask));
 var final=await composer.ComposeAsync(catalog.LogoSnapshotPath,await modelTask,(await Task.WhenAll(variantTasks)).ToList(),catalog.ProductCode,catalog.Age,ct,scene,catalog.SeriesCount,catalog.OutputWidth,catalog.OutputHeight);scratch.Add(final);
 var whites=new List<string>();
 foreach(var generated in await Task.WhenAll(whiteTasks)){
     var output=await composer.ExportWhiteAsync(generated,catalog.WhiteOutputWidth,catalog.WhiteOutputHeight,ct);
     scratch.Add(output);whites.Add(output);
 }
 catalog.GeneratedCatalogPath=final;
 catalog.WhiteModelPathsJson=JsonSerializer.Serialize(whites);
 // Catalog persistence and token completion commit together.
 await using(var transaction=await db.Database.BeginTransactionAsync(ct)){
 db.Catalogs.Add(catalog);await db.SaveChangesAsync(ct);await tokens.Complete(reservation);await transaction.CommitAsync(ct);
 }
 scratch.Remove(final);
 foreach(var white in whites)scratch.Remove(white);
 await retention.Cleanup();
 }catch{if(reservation!=null)await tokens.Refund(reservation);throw;}
 finally{foreach(var name in scratch)files.Delete(name);}
 }
 [HttpPost] public async Task<IActionResult> Delete(int id) {
 var c=await db.Catalogs.SingleOrDefaultAsync(x=>x.Id==id&&x.UserId==UserId);if(c==null)return NotFound();
 db.Remove(c);await db.SaveChangesAsync();files.DeleteCatalog(c);TempData["Message"]="Katalog silindi.";return RedirectToAction("MyCatalogs");
 }
 public async Task<IActionResult> MyCatalogs()=>View(await db.Catalogs.Where(x=>x.UserId==UserId).OrderByDescending(x=>x.CreatedAt).ToListAsync());
 public async Task<IActionResult> Result(int id){var c=await db.Catalogs.SingleOrDefaultAsync(x=>x.Id==id&&x.UserId==UserId);return c==null?NotFound():View(c);}
 public async Task<IActionResult> Image(int id,bool download=false){var c=await db.Catalogs.SingleOrDefaultAsync(x=>x.Id==id&&(x.UserId==UserId||User.IsInRole("Admin")));if(c==null)return NotFound();Response.Headers.CacheControl="private, no-store";return PhysicalFile(files.PathFor(c.GeneratedCatalogPath),"image/png",download?$"katalog-{c.ProductCode}.png":null);}

 public async Task<IActionResult> WhiteImage(int id,int index=0,bool download=false){
 var c=await db.Catalogs.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id&&(x.UserId==UserId||User.IsInRole("Admin")));
 if(c==null)return NotFound();
 var images=JsonSerializer.Deserialize<List<string>>(c.WhiteModelPathsJson)??[];
 if(index<0||index>=images.Count)return NotFound();
 Response.Headers.CacheControl="private, no-store";
 return PhysicalFile(files.PathFor(images[index]),"image/png",download?$"renk-{index+1}-beyaz-fon.png":null);
 }
 public async Task<IActionResult> Logo(int? userId){var id=userId??UserId;if(id!=UserId&&!User.IsInRole("Admin"))return NotFound();var p=await prefs.GetAsync(id);if(p==null)return NotFound();Response.Headers.CacheControl="private, no-store";return PhysicalFile(files.PathFor(p.LogoPath),"image/png");}
}
