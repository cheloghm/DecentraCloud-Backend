using Microsoft.AspNetCore.Mvc;
using DecentraCloud.API.DTOs;
using DecentraCloud.API.Interfaces.ServiceInterfaces;
using System.Threading.Tasks;

namespace DecentraCloud.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReplicationController : ControllerBase
    {
        private readonly IReplicationService _replicationService;

        public ReplicationController(IReplicationService replicationService)
        {
            _replicationService = replicationService;
        }

        [HttpPost("replicate")]
        public async Task<IActionResult> ReplicateData([FromBody] ReplicationRequestDto replicationRequest)
        {
            var result = await _replicationService.ReplicateData(replicationRequest);

            if (!result)
            {
                return BadRequest(new { message = "Data replication failed." });
            }

            return Ok(new { message = "Data replication successful." });
        }
    }
}
