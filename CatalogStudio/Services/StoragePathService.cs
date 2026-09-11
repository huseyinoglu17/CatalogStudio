using Microsoft.Data.Sqlite;
namespace CatalogStudio.Services;
public interface IStoragePathService {
 string BasePath {get;} string FilesPath {get;} string KeysPath {get;} string TempPath {get;}
 string ConnectionString {get;}
}
public sealed class StoragePathService:IStoragePathService {
 public string BasePath {get;} public string FilesPath {get;} public string KeysPath {get;} public string TempPath {get;}
 public string ConnectionString {get;}
 public StoragePathService(IWebHostEnvironment env,IConfiguration config) {
 BasePath=Path.GetFullPath(config["APP_DATA_PATH"]??Path.Combine(env.ContentRootPath,"App_Data"),env.ContentRootPath);
 FilesPath=Path.GetFullPath(config["Storage:Root"]??Path.Combine(BasePath,"files"),env.ContentRootPath);
 KeysPath=Path.Combine(BasePath,"DataProtection-Keys");
 TempPath=Path.Combine(Path.GetTempPath(),"CatalogStudio",Guid.NewGuid().ToString("N"));
 var connection=config.GetConnectionString("DefaultConnection")??config.GetConnectionString("Default");
 var sqlite=new SqliteConnectionStringBuilder(connection??"");
 if(string.IsNullOrWhiteSpace(sqlite.DataSource))sqlite.DataSource=Path.Combine(BasePath,"catalog.db");
 else if(sqlite.DataSource!=":memory:")sqlite.DataSource=Path.GetFullPath(sqlite.DataSource,env.ContentRootPath);
 if(!env.IsDevelopment()) {
 EnsureContained(FilesPath);
 if(sqlite.DataSource==":memory:")throw new InvalidOperationException("Production requires a persistent SQLite file.");
 EnsureContained(sqlite.DataSource);
 }
 sqlite.DefaultTimeout=30;ConnectionString=sqlite.ToString();
 foreach(var path in new[]{BasePath,FilesPath,KeysPath,TempPath})Directory.CreateDirectory(path);
 if(sqlite.DataSource!=":memory:")Directory.CreateDirectory(Path.GetDirectoryName(sqlite.DataSource)!);
 }
 void EnsureContained(string path) {
 var relative=Path.GetRelativePath(BasePath,path);
 if(relative==".."||relative.StartsWith(".."+Path.DirectorySeparatorChar)||Path.IsPathRooted(relative))
 throw new InvalidOperationException("Production persistent storage must be inside APP_DATA_PATH.");
 }
}