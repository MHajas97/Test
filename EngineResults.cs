public class Engine
{
    public string? name{get;set;}
    public string? threat_found { get; set; }
    public int scan_result_i { get; set; }
    public DateTime def_time { get; set; }

    public void Print()
    {
        Console.WriteLine("Engine: "+name);
        Console.WriteLine("ThreatFound: "+threat_found);
        Console.WriteLine("ScanResult: "+ scan_result_i);
        Console.WriteLine("DefTime: " + def_time);
    }

}