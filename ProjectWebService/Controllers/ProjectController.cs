using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProjectService.Models;
using ProjectStatefulService.Interfaces;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace ProjectWebService.Controllers
{
    [Route("api/[controller]")]
    public class ProjectController : Controller
    {
        // GET api/project
        [HttpGet]
        public async Task<IEnumerable<Project>> Get()
        {
            IProject projectProxy = ServiceProxy.Create<IProject>(new Uri("fabric:/ProjectService/ProjectStatefulService"), new ServicePartitionKey(0));

            try
            {
                IEnumerable<Project> projectIEnumerable = await projectProxy.GetProjects();
                return projectIEnumerable;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        // GET api/project/5
        [HttpGet("{id}")]
        public async Task<Project> Get(string id)
        {
            Guid projectIdGuid;

            if (!Guid.TryParse(id, out projectIdGuid))
            {
                HttpContext.Response.StatusCode = 400;
                return null;
            }

            IProject projectProxy = ServiceProxy.Create<IProject>(new Uri("fabric:/ProjectService/ProjectStatefulService"), new ServicePartitionKey(0));

            try
            {
                Project foundProject = await projectProxy.GetProjectById(projectIdGuid);
                if (foundProject.Name == null)
                {
                    HttpContext.Response.StatusCode = 404;
                    return null;
                }
                return foundProject;
            }
            catch (Exception e)
            {
                HttpContext.Response.StatusCode = 500;
                return null;
            }
        }
        
        
        // POST api/project
        [HttpPost]
        public async Task<Project> CreateProject(string projectName)
        {
            if(projectName == null)
            {
                HttpContext.Response.StatusCode = 400;
                return null;
            }

            if(projectName.Trim() == string.Empty)
            {
                HttpContext.Response.StatusCode = 400;
                return null;
            }

            IProject projectProxy = ServiceProxy.Create<IProject>(new Uri("fabric:/ProjectService/ProjectStatefulService"), new ServicePartitionKey(0));

            try
            {
                Project newProject = await projectProxy.CreateEmptyProject(projectName);
                return newProject;
            }
            catch (Exception e)
            {
                HttpContext.Response.StatusCode = 500;
                return null;
            }
        }

        
    }
}
