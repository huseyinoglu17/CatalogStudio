using System.Collections.Concurrent;
namespace CatalogStudio.Services;
public record CatalogScene(string Description, string Paper, string Ink);
public class CatalogSceneService {
 private readonly ConcurrentDictionary<int,int> previous = new();
 private static readonly CatalogScene[] Scenes = [
 new("a cozy ivory boucle nursery armchair, softly blurred teddy bear at the far edge, warm window light and a delicate olive branch", "F6F1E8", "795B3D"),
 new("a sunlit oatmeal linen nursery, pale oak furniture in soft focus, sheer curtains and subtle eucalyptus leaves", "F0EEE6", "63715A"),
 new("a warm blush plaster studio with a cream upholstered seat, dried meadow flowers at the far edge and diffused morning light", "F5EDE7", "886652"),
 new("a pale powder blue nursery with ivory cushions, a small wooden toy in the distant background and soft side window light", "EDF1F2", "526775"),
 new("a sand-colored Mediterranean studio, rounded plaster alcove, natural woven rug and soft botanical shadows", "F3EBDD", "806A4C"),
 new("a muted sage nursery, soft cream quilted seating, pale wood and delicate fern foliage in the distant corners", "EDF0E7", "59694F")];
 public CatalogScene Next(int userId) {
 var index=previous.AddOrUpdate(userId,_=>Random.Shared.Next(Scenes.Length),(_,old)=>(old+Random.Shared.Next(1,Scenes.Length))%Scenes.Length);
 return Scenes[index];
 }
}