using HappyTravel.EdoContracts.Grpc.Services;
using HappyTravel.SupplierOptionsClient.Models;

namespace HappyTravel.Edo.Api.Services.Connectors;

public interface IGrpcClientsStorage
{
    /// <summary>
    ///     Returns IConnectionGrpcService
    /// </summary>
    IConnectorGrpcService Get(SlimSupplier supplier);

    /// <summary>
    ///     Updates existed IConnectionGrpcService
    /// </summary>
    void Update(SlimSupplier supplier);
}