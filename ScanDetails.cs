public class FileInfo
{
    public string filename;
    public string oStatus;

    public List<Engine>scanresultList;

public FileInfo() {
    scanresultList = new List<Engine>();
    filename="";
    oStatus="";
}
public void printFile()
{
    Console.WriteLine("Filename: " + filename);
    Console.WriteLine("OverallStatus: "+ oStatus);
    foreach (Engine eng in scanresultList)
    {
        eng.Print();
    }
}
}
