using System.Security.Cryptography;

public class FileHash

{
    public string getHash(string path)
    {
        try{
        using (FileStream stream =File.OpenRead(path))
        {
            using (SHA256 sha256= SHA256.Create())
            {
                byte[] shaBytes= sha256.ComputeHash(stream);
                string hashString= BitConverter.ToString(shaBytes).Replace("-","");
                return hashString;
            }
        }
        }
        catch (Exception ex)
        {
            return ex.Message;
        }

    }

}