using DecentraCloud.API.DTOs;

namespace DecentraCloud.API.Interfaces.ServiceInterfaces
{
    public interface IReplicationService
    {
        Task<bool> ReplicateData(ReplicationRequestDto replicationRequest);
    }
}
