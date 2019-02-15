
// 临时
public class FakeFileSystem : Duktape.IFileSystem
{
    public bool Exists(string path)
    {
        return System.IO.File.Exists(path);
    }

    public string ReadAllText(string path)
    {
        return System.IO.File.ReadAllText(path);
    }
}
