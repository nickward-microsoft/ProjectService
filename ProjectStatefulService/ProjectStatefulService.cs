using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ProjectService.Models;
using ProjectStatefulService.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using System.Fabric.Description;

namespace ProjectStatefulService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class ProjectStatefulService : StatefulService, IProject
    {
        public ProjectStatefulService(StatefulServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see http://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new List<ServiceReplicaListener>()
            {
                new ServiceReplicaListener((context) => this.CreateServiceRemotingListener(context))
            };
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        //protected override async Task RunAsync(CancellationToken cancellationToken)
        //{
        // TODO: Replace the following sample code with your own logic 
        //       or remove this RunAsync override if it's not needed in your service.

        //var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

        //while (true)
        //{
        //    cancellationToken.ThrowIfCancellationRequested();

        //    using (var tx = this.StateManager.CreateTransaction())
        //    {
        //        var result = await myDictionary.TryGetValueAsync(tx, "Counter");

        //        ServiceEventSource.Current.ServiceMessage(this, "Current Counter Value: {0}",
        //            result.HasValue ? result.Value.ToString() : "Value does not exist.");

        //        await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

        //        // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
        //        // discarded, and nothing is saved to the secondary replicas.
        //        await tx.CommitAsync();
        //    }

        //    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        //}
        //}

        public async Task<Project> CreateEmptyProject(string projectName)
        {
            // set up a blank Project to create
            var newProject = new Project();
            newProject.Name = projectName;
            newProject.Status = "New";
            newProject.TaskIdList = new List<int>();

            // save the project to the reliable dictionary
            var projectDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<String, Project>>("ProjectDictionary");
            using (var tx = this.StateManager.CreateTransaction())
            {
                try
                {
                    await projectDictionary.AddAsync(tx, projectName.ToUpperInvariant(), newProject);
                } catch (Exception e)
                {
                    ServiceEventSource.Current.ServiceMessage(this, "Could not add project {0} to ProjectDictionary. Exception details: {1}", newProject.Name, e.ToString());
                }
                await tx.CommitAsync();
            }

            // return the created project to the caller
            return newProject;
        }

        public async Task<IEnumerable<Project>> GetProjects()
        {
            var projectList = new List<Project>();

            var projectDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<String, Project>>("ProjectDictionary");

            using (var tx = this.StateManager.CreateTransaction())
            {
                try
                {
                    if (await projectDictionary.GetCountAsync(tx) > 0)
                    {
                        var projectEnumerable = await projectDictionary.CreateEnumerableAsync(tx);
                        var enumerator = projectEnumerable.GetAsyncEnumerator();
                        while (await enumerator.MoveNextAsync(new CancellationToken()))
                        {
                            projectList.Add(enumerator.Current.Value);
                        }
                    }
                }
                catch (Exception e)
                {
                    ServiceEventSource.Current.ServiceMessage(this, "Could not get list of projects. Exception details: {0}", e.ToString());
                }
            }

            return projectList;
        }

        public async Task<Project> GetProjectByName(string projectName)
        {
            // set up variable to hold the project
            var project = new Project();

            // retrieve the project from the reliable dictionary
            var projectDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<String, Project>>("ProjectDictionary");
            using (var tx = this.StateManager.CreateTransaction())
            {
                try
                {
                    var result = await projectDictionary.TryGetValueAsync(tx, projectName.ToUpperInvariant());
                    if (result.HasValue)
                    {
                        project = result.Value;
                    }
                }
                catch (Exception e)
                {
                    ServiceEventSource.Current.ServiceMessage(this, "Could not read project with Id {0} from ProjectDictionary. Exception details: {1}", projectName, e.ToString());
                }
            }

            return project;
        }

        public async Task<Project> AddTaskToProject(String projectName, int taskId)
        {
            // set up variable to hold the project
            var project = new Project();

            // retrieve the project from the reliable dictionary
            var projectDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<String, Project>>("ProjectDictionary");
            using (var tx = this.StateManager.CreateTransaction())
            {
                try
                {
                    var result = await projectDictionary.TryGetValueAsync(tx, projectName.ToUpperInvariant());
                    if (result.HasValue)
                    {
                        project = result.Value;
                        // add the task ID to the project
                        project.TaskIdList.Add(taskId);
                        await projectDictionary.AddOrUpdateAsync(tx, projectName.ToUpperInvariant(), project, (key, value) => project);
                    }
                }
                catch (Exception e)
                {
                    ServiceEventSource.Current.ServiceMessage(this, "Could not read project with Id {0} from ProjectDictionary. Exception details: {1}", projectName, e.ToString());
                    return null;
                }
                await tx.CommitAsync();
            }

            // return the project to the caller
            return project;
        }

        public async Task<Project> CompleteProject(string projectName)
        {
            // set up variable to hold the project
            var project = new Project();

            // retrieve the project from the reliable dictionary
            var projectDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<String, Project>>("ProjectDictionary");
            using (var tx = this.StateManager.CreateTransaction())
            {
                try
                {
                    var result = await projectDictionary.TryGetValueAsync(tx, projectName.ToUpperInvariant());
                    if (result.HasValue)
                    {
                        project = result.Value;
                        // add the task ID to the project
                        project.Status = "Completed";
                        await projectDictionary.AddOrUpdateAsync(tx, projectName.ToUpperInvariant(), project, (key, value) => project);
                    }
                }
                catch (Exception e)
                {
                    ServiceEventSource.Current.ServiceMessage(this, "Could not read project with Id {0} from ProjectDictionary. Exception details: {1}", projectName, e.ToString());
                    return null;
                }
                await tx.CommitAsync();
            }

            // return the project to the caller
            return project;            
        }

        public async Task DeleteProject(String projectName)
        {
            // retrieve the project from the reliable dictionary
            var projectDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<String, Project>>("ProjectDictionary");
            using (var tx = this.StateManager.CreateTransaction())
            {
                try
                {
                    var result = await projectDictionary.TryRemoveAsync(tx, projectName.ToUpperInvariant());
                }
                catch (Exception e)
                {
                    ServiceEventSource.Current.ServiceMessage(this, "Could not delete project with Id {0} from ProjectDictionary. Exception details: {1}", projectName, e.ToString());
                }
                await tx.CommitAsync();
            }
        }

    }
}
