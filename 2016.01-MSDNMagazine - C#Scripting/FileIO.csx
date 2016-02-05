//using System.IO;

IEnumerable<string> dir(string path, string filter="*", bool recurse=false)
{
  List<string> items = new List<string>(){"one","two"};
  return items;
//  yield return path;
  if (recurse)
  {
    foreach(string directory in Directory.EnumerateDirectories(path))
    {
 	  dir(directory, path, recurse);
    }   
  }
  
  foreach(string file in Directory.EnumerateFiles(path, filter))
  {
//	  yield return file;
  }
}
   
dir(@"C:\data\temp\AvistaWARequests");

