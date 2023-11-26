public class ScanResult
{
    public string def_time { get; set; }
    public int scan_result_i { get; set; }
    public int scan_time { get; set; }
    public string threat_found { get; set; }

    public ScanResult()
    {
        def_time="";
        scan_result_i=0;
        scan_time=0;
        threat_found="";
    }
}