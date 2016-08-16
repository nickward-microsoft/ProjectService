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

        // GET api/project
        [HttpGet]
        public async Task<Project> GetProject(string projectId)
        {
            Guid projectIdGuid;

            if (!Guid.TryParse(projectId, out projectIdGuid))
            {
                HttpContext.Response.StatusCode = 400;
                return null;
            }

            IProject projectProxy = ServiceProxy.Create<IProject>(new Uri("fabric:/ProjectService/ProjectStatefulService"), new ServicePartitionKey(0));

            try
            {
                Project foundProject = await projectProxy.GetProjectById(projectIdGuid);
                if(foundProject.Name == null)
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
    }
}
