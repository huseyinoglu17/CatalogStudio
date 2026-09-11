namespace CatalogStudio.Models;
public class TokenWallet { public int UserId {get;set;} public AppUser User {get;set;}=null!; public long Balance {get;set;} }
public class TokenOperation { public string Id {get;set;}=Guid.NewGuid().ToString("N"); public int UserId {get;set;} public int Cost {get;set;} public string Status {get;set;}="Pending"; public DateTime CreatedAt {get;set;}=DateTime.UtcNow; }
