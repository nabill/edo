using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace HappyTravel.Edo.Api.Infrastructure;

public class RemoveMetadataControllerFeatureProvider : ControllerFeatureProvider
{
    protected override bool IsController(TypeInfo typeInfo) 
        => typeInfo.FullName != "Microsoft.AspNetCore.OData.Routing.Controllers.MetadataController" && base.IsController(typeInfo);
}