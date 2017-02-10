using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ProjectService.SeedData
{
    class Program
    {
        static void Main(string[] args)
        {
            var c = new char();
            var projName = "";

            for(var i=0; i < 26; i++)
            {
                c = Convert.ToChar(65 + i);
                projName = c.ToString() + " Project";

                Console.WriteLine("Creating project {0}...", projName);
                CreateProject(projName);
            }
            
            Console.ReadKey();
        }

        static async void CreateProject(string ProjectName)
        {
            var client = new HttpClient();
            
            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "b7e6b1c694384a4688aa3cc9ec801271");

            var uri = "https://hectagonapi.azure-api.net/project/" + ProjectName;

            HttpResponseMessage response;

            //Request body
            byte[] byteData = Encoding.UTF8.GetBytes("{}");

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                response = await client.PostAsync(uri, content);

                if(response.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    Console.WriteLine(response.ReasonPhrase);
                } else
                {
                    Console.WriteLine("{0} created.", ProjectName);
                }
            }

        }
    }
}
