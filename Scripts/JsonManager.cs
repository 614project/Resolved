using Newtonsoft.Json;
using System;
using System.IO;
namespace Resolved.Scripts;

class JsonManager
{
    public static string SaveFolder { get; } = Path.Combine(Windows.Storage.ApplicationData.Current.LocalCacheFolder.Path, "Roaming" , "resolved");

    public static bool TryRead<T>(ref T target , string path)
    {
        try
        {
            if (!File.Exists(path = Path.Combine(SaveFolder , path)))
                return true;
            if (JsonConvert.DeserializeObject<T>(File.ReadAllText(path)) is T t)
            {
                target = t;
            }
        } catch
        {
            return false;
        }
        return true;
    }
    public static void Write(object target , in string path)
    {
        File.WriteAllText(Path.Combine(SaveFolder , path) , JsonConvert.SerializeObject(target));
    }
    public static Exception? TryDelete(in string path)
    {
        try
        {
            File.Delete(Path.Combine(SaveFolder, path));
        } catch (Exception ex)
        {
            return ex;
        }
        return null;
    }
}
