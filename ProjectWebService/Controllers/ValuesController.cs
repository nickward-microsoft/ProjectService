using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProjectStatefulService.Interfaces;
using ProjectService.Models;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Client;

namespace ProjectWebService.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            IProject projectProxy = ServiceProxy.Create<IProject>(new Uri("fabric:/ProjectService/ProjectStatefulService"), new ServicePartitionKey(0));

            try
            {
                Project newProject = await projectProxy.CreateEmptyProject("Test Project");
                return new string[] { newProject.Id.ToString(), newProject.Name, newProject.TaskIdList.Count.ToString(), newProject.Status };
            } catch (Exception e)
            {
                return new string[] { "Error occurred"};
            }            
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
