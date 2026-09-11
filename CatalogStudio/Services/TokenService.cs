using CatalogStudio.Data;
using CatalogStudio.Models;
using Microsoft.EntityFrameworkCore;
namespace CatalogStudio.Services;
public class TokenService(AppDbContext db) {
 public async Task<long> Balance(int userId) {
 await db.Database.ExecuteSqlInterpolatedAsync($"INSERT OR IGNORE INTO TokenWallets (UserId, Balance) VALUES ({userId}, {200L})");
 return await db.TokenWallets.Where(x=>x.UserId==userId).Select(x=>x.Balance).SingleAsync();
 }
 public async Task<string> Reserve(int userId,int cost) {
 await Balance(userId);
 await using var tx=await db.Database.BeginTransactionAsync();
 var changed=await db.TokenWallets.Where(x=>x.UserId==userId && x.Balance>=cost).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.Balance,x=>x.Balance-cost));
 if(changed==0)throw new InvalidOperationException($"Yetersiz token. Bu işlem {cost} token gerektirir.");
 var op=new TokenOperation{UserId=userId,Cost=cost};db.TokenOperations.Add(op);await db.SaveChangesAsync();await tx.CommitAsync();return op.Id;
 }
 public Task Complete(string id)=>db.TokenOperations.Where(x=>x.Id==id && x.Status=="Pending").ExecuteUpdateAsync(s=>s.SetProperty(x=>x.Status,"Completed"));
 public async Task Refund(string id) {
 await using var tx=await db.Database.BeginTransactionAsync();
 var op=await db.TokenOperations.AsNoTracking().SingleAsync(x=>x.Id==id);
 if(await db.TokenOperations.Where(x=>x.Id==id && x.Status=="Pending").ExecuteUpdateAsync(s=>s.SetProperty(x=>x.Status,"Refunded"))==1)
 await db.TokenWallets.Where(x=>x.UserId==op.UserId).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.Balance,x=>x.Balance+op.Cost));
 await tx.CommitAsync();
 }
 public async Task RecoverPending() {foreach(var id in await db.TokenOperations.Where(x=>x.Status=="Pending").Select(x=>x.Id).ToListAsync()) await Refund(id);}
 public async Task<bool> Change(int userId,string operation,long amount) {
 if(amount<0||amount>1000000000)return false;await Balance(userId);
 return operation switch {
 "add"=>await db.TokenWallets.Where(x=>x.UserId==userId && x.Balance<=1000000000-amount).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.Balance,x=>x.Balance+amount))==1,
 "subtract"=>await db.TokenWallets.Where(x=>x.UserId==userId && x.Balance>=amount).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.Balance,x=>x.Balance-amount))==1,
 "set"=>await db.TokenWallets.Where(x=>x.UserId==userId).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.Balance,amount))==1,
 "clear"=>await db.TokenWallets.Where(x=>x.UserId==userId).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.Balance,0L))==1,
 _=>false };
 }
}