using System.Globalization;
using System.Reflection.Metadata;
using System.Net.Http;
using System.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http;
using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Buffers;


namespace srestClient
{ 

    class RestClient
    {
        public static async Task getRequest(string filepath)
       {
            FileInfo fileinfo= new FileInfo();
            if(filepath!=null)
            {
                fileinfo.filename=Path.GetFileName(filepath);
            }
            var client = new HttpClient();
            FileHash filehash= new FileHash();
            client.BaseAddress=new Uri("https://api.metadefender.com");
            string message= "";
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://api.metadefender.com/v4/hash/"+ filehash.getHash(filepath)),
                Headers =
                {
                    { "apikey", "#APIKEY" }
                }
            };
            try
            {
                using (var response = await client.SendAsync(request)) 
                {
                    if (response.IsSuccessStatusCode==false)
                        {
                            message=RestClient.IsFailed(filepath);
                        }  
                        else
                        {
                            message=await response.Content.ReadAsStringAsync();
                        }
                        JToken outer = JToken.Parse(message);
                        JObject? inner = outer["scan_results"]["scan_details"].Value<JObject>();
                        List<string>? keys= inner.Properties().Select(p=> p.Name).ToList();
                        var dict = JsonConvert.DeserializeObject<Dictionary<string,Object>>(inner.ToString());
                        for(int i=0;i<keys.Count();i++)
                        {
                            Engine eng= new Engine();
                            eng.name=keys[i];
                            var en=(JObject)inner.Properties().Children().ElementAt(i);
                            foreach (PropertyInfo prop in eng.GetType().GetProperties())
                            {
                                if (en.ContainsKey(prop.Name))
                                {
                                    object? dicValue= en[prop.Name];
                                    Type? propertyInfo= typeof(Engine).GetProperty(prop.Name).PropertyType;
                                    dicValue=ConvertObject((JValue)dicValue,propertyInfo);
                                    prop.SetValue(eng,dicValue);
                                }
                            }
                            fileinfo.scanresultList.Add(eng);
                            fileinfo.oStatus=RestClient.getOStatus(fileinfo);
                        }
                        fileinfo.printFile();
                }
            }
            catch( Exception e)
            {
                Console.WriteLine ("error:" + e);
            }
           
        }
        public static object ConvertObject(JValue jVal, Type target)
        {
                // Check the target type and cast accordingly
            if (target == typeof(string))
            {
                return jVal.Value<string>();
            }
            else if (target == typeof(int))
            {
                 return jVal.Value<int>();
            }
            else if (target == typeof(double))
            {
                return jVal.Value<double>();
            }
            else if (target == typeof(bool))
            {
                return jVal.Value<bool>();
            }
            else if (target == typeof(System.DateTime))
            {
                return jVal.Value<System.DateTime>();
            }
            else
            {
                throw new ArgumentException($"Unsupported target type: {target}");
            }
        }
        //<HttpResponseMessage>
        public static string IsFailed(string filepath)
        {
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage
            {
            Method = HttpMethod.Post,
            RequestUri = new Uri("https://api.metadefender.com/v4/file"),
            Headers =
            {
                { "apikey", "#APIKEY" },
            },
            Content = new StringContent("\""+filepath+"\"}")
            };
            request.Content.Headers.ContentType= new MediaTypeHeaderValue("application/octet-stream");
            try
            {
                using (var response = client.Send(request)) 
                {
                    var text= response.Content.ReadAsStringAsync().Result;
                    JObject key= JObject.Parse(text.ToString());
                    string? dataid="";
                    if (key.ContainsKey("data_id"))
                    {
                        dataid= key["data_id"].ToString();
                    }
                        return RestClient.Retry(dataid);
                    }
            } catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public static string Retry(string data_id)
        {
            bool responseOK=false;
            string message="";
            string? status="";
            do
            {
                var client = new HttpClient();
                client.BaseAddress=new Uri("https://api.metadefender.com");
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("https://api.metadefender.com/v4/file/"+ data_id),
                    Headers =
                    {
                        { "apikey", "#APIKEY" },
                        { "x-file-metadata", "1" }
                    }
                };
                try
                {
                using (var response = client.Send(request)) 
                {

                    if(response.IsSuccessStatusCode==false)
                    {
                        CancellationTokenSource cancel= new CancellationTokenSource();
                        System.Threading.Thread.Sleep(10000);
                        cancel.Cancel();
                    }
                    else if(response.IsSuccessStatusCode==true)
                    {
                        message= response.Content.ReadAsStringAsync().Result;
                        JToken? outer = JToken.Parse(message);
                        if(outer!=null)
                        {
                            JObject? inner = outer["scan_results"].Value<JObject>();
                            if(inner != null)
                            {
                                if(inner.ContainsKey("scan_all_result_a"))
                                {
                                    status=inner["scan_all_result_a"].ToString();
                                }
                                if(status=="In Progress")
                                {
                                    CancellationTokenSource cancel= new CancellationTokenSource();
                                    System.Threading.Thread.Sleep(10000);
                                    cancel.Cancel();   
                                }
                                else
                                {
                                    responseOK=true;
                                    message= response.Content.ReadAsStringAsync().Result;
                                }
                            }
                        }
                        
                    }

                }
                }
                catch(Exception ex)
                {
                    message = ex.ToString();
                }
            }while(responseOK==false);
            return message;
        }
        
        public static string getOStatus(FileInfo fileInfo)
        {
            string oStatus;
            int clean=0;
            int infected=0;
            foreach( Engine eng in fileInfo.scanresultList)
            {
                if (eng.scan_result_i==1)
                {
                    infected+=1;
                }
                else
                {
                    clean+=1;
                }
            }
            if(clean>infected)
            {
                oStatus = "Clean";
            }
            else
            {
                oStatus="Infected";
            }
            return oStatus;
        }
    }

}