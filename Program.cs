using System.Text;
using System.Threading.Tasks;
using srestClient;


namespace Program
{
    public class Program
    {
        static async Task Main(string[] args)
    {
        //Task task1 = RestClient.IsFailed(args[0]);
        try
        {
        Task task2 = RestClient.getRequest(args[0]);
        //Task task3 = RestClient.Retry(args[0]);
        await Task.WhenAll(task2);
        }
        catch( Exception e)
        {
            Console.WriteLine("error:" + e);
        }
    }
    }
}