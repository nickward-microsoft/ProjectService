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
        Task<Project> CreateEmptyProject(string projectName);
        Task<Project> GetProjectById(Guid projectId);
    }
}
