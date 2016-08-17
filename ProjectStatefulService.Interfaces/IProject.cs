using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectService.Models;

namespace ProjectStatefulService.Interfaces
{
    using Microsoft.ServiceFabric.Services.Remoting;

    public interface IProject : IService
    {
        Task<IEnumerable<Project>> GetProjects();
        Task<Project> GetProjectByName(string projectName);
        Task<Project> CreateEmptyProject(string projectName);
        Task<Project> AddTaskToProject(string projectName, int taskId);
        Task<Project> CompleteProject(string projectName);
        Task DeleteProject(string projectName);
    }
}
