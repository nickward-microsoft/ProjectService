using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProjectService.Models;
using ProjectStatefulService.Interfaces;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Fabric;

namespace ProjectWebService.Controllers
{
    [Route("api/[controller]")]
    public class ProjectController : Controller
    {
        // GET api/project
        [HttpGet]
        public async Task<IEnumerable<Project>> Get()
        {
            IProject projectProxy;
            List<Project> projectList = new List<Project>();

            // loop through all the partitions to get all of the projects
            for(int i=0; i<26; i++)
            {
                projectProxy = this.CreateServiceFabricProxy(i);
                try
                {

                    IEnumerable<Project> projectIEnumberable = await projectProxy.GetProjects();
                    foreach(var p in projectIEnumberable)
                    {
                        projectList.Add(p);
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return projectList;
            
        }

        // GET api/project/Project%20Xray
        [HttpGet("{projectName}")]
        public async Task<Project> Get(string projectName)
        {
            var projectProxy = this.CreateServiceFabricProxy(projectName);

            try
            {
                Project foundProject = await projectProxy.GetProjectByName(projectName);
                if(foundProject.Name == null)
                {
                    HttpContext.Response.StatusCode = 404;
                    return null;
                }
                return foundProject;
            }
            catch (Exception)
            {
                HttpContext.Response.StatusCode = 500;
                return null;
            }
        }
        
        // PUT api/project?projectName=Project%20Xray&taskId=5
        [HttpPut]
        public async Task<Project> AddTaskToProject(string projectName, int taskId)
        {
            var projectProxy = this.CreateServiceFabricProxy(projectName);

            try
            {
                Project foundProject = await projectProxy.GetProjectByName(projectName);
                if (foundProject.Name == null)
                {
                    HttpContext.Response.StatusCode = 404;
                    return null;
                }
                Project updatedProject = await projectProxy.AddTaskToProject(projectName, taskId);
                return updatedProject;
            }
            catch (Exception)
            {
                HttpContext.Response.StatusCode = 500;
                return null;
            }
        }

        // PATCH api/project?projectName=Project%20Xray
        [HttpPatch]
        public async Task<Project> CompleteProject(string projectName)
        {
            var projectProxy = this.CreateServiceFabricProxy(projectName);

            try
            {
                Project updatedProject = await projectProxy.CompleteProject(projectName);
                return updatedProject;
            }
            catch (Exception)
            {
                HttpContext.Response.StatusCode = 500;
                return null;
            }
        }

        // DELETE api/project?projectName=Project%20Xray
        [HttpDelete]
        public async Task DeleteProject(string projectName)
        {
            var projectProxy = this.CreateServiceFabricProxy(projectName);

            try
            {
                await projectProxy.DeleteProject(projectName);
            }
            catch (Exception)
            {
                HttpContext.Response.StatusCode = 500;
            }
        }

        // POST api/project?projectName=Example%20Project
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

            var projectProxy = this.CreateServiceFabricProxy(projectName);

            try
            {
                Project newProject = await projectProxy.CreateEmptyProject(projectName);
                HttpContext.Response.StatusCode = 201;
                Microsoft.Extensions.Primitives.StringValues host;
                HttpContext.Request.Headers.TryGetValue("Host", out host);
                string requestedDomain = HttpContext.Request.Headers["Host"];
                string requestScheme = HttpContext.Request.Scheme;
                Uri newProjectUri = new Uri(String.Concat(requestScheme, "://", requestedDomain, "/api/project/", newProject.Name.ToUpperInvariant()));
                HttpContext.Response.Headers.Add("Location", newProjectUri.AbsoluteUri);
                return newProject;
            }
            catch (Exception)
            {
                HttpContext.Response.StatusCode = 500;
                return null;
            }
        }

        private IProject CreateServiceFabricProxy(String projectName)
        {
            char firstLetterOfProjectName = projectName.First();
            var partition = char.ToUpper(firstLetterOfProjectName) - 'A';
            return this.CreateServiceFabricProxy(partition);
        }

        private IProject CreateServiceFabricProxy(long partition)
        {
            ServicePartitionKey partitionKey = new ServicePartitionKey(partition);

            IProject projectProxy = ServiceProxy.Create<IProject>(new Uri("fabric:/ProjectService/ProjectStatefulService"), partitionKey);
            return projectProxy;
        }

    }
}
