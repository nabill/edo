using HappyTravel.EdoContracts.Grpc.Services;
using HappyTravel.SupplierOptionsClient.Models;

namespace HappyTravel.Edo.Api.Services.Connectors;

public interface IGrpcClientsStorage
{
    IConnectorGrpcService Get(SlimSupplier supplier);

    void Update(SlimSupplier supplier);
}