using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Linq;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public class ApiExplorerGroupPerVersionConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            var controllerNamespace = controller.ControllerType.Namespace;
            var controllerPurpose = controllerNamespace.Split('.').Last();

            switch (controllerPurpose)
            {
                case "AdministratorControllers":
                    controller.ApiExplorer.GroupName = "admin-v1.0";
                    return;
                case "AgentControllers":
                    controller.ApiExplorer.GroupName = "agent-v1.0";
                    return;
                case "Controllers":
                    controller.ApiExplorer.GroupName = "common-v1.0";
                    return;
            }
        }
    }
}
