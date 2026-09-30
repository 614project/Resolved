using Newtonsoft.Json;
using System;
using System.IO;
namespace Resolved.Scripts;

class JsonManager
{
    public static string SaveFolder { get; } = GetSaveFolder();

    private static string GetSaveFolder()
    {
        try
        {
            return Windows.Storage.ApplicationData.Current.LocalFolder.Path;
        } catch
        {
            //패키지 ID가 없는(Unpackaged) 실행에서는 ApplicationData를 쓸 수 없다.
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) , "resolved.ce");
            Directory.CreateDirectory(path);
            return path;
        }
    }

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
