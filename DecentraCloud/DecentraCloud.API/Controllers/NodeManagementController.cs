using DecentraCloud.API.DTOs;
using DecentraCloud.API.Interfaces.ServiceInterfaces;
using DecentraCloud.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DecentraCloud.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NodeManagementController : ControllerBase
    {
        private readonly INodeService _nodeService;

        public NodeManagementController(INodeService nodeService)
        {
            _nodeService = nodeService;
        }

        [HttpGet("node/{nodeId}")]
        public async Task<IActionResult> GetNodeById(string nodeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var nodeStatus = await _nodeService.GetNodeStatus(nodeId, userId);

            if (nodeStatus == null)
            {
                return NotFound(new { message = "Node not found or access denied." });
            }

            return Ok(nodeStatus);
        }

        [HttpPut("node/{nodeId}")]
        public async Task<IActionResult> UpdateNode(string nodeId, [FromBody] NodeUpdateDto nodeUpdateDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var node = await _nodeService.GetNodeById(nodeId);

            if (node == null || node.UserId != userId)
            {
                return NotFound(new { message = "Node not found or access denied." });
            }

            node.Storage = nodeUpdateDto.Storage;
            var result = await _nodeService.UpdateNode(node);

            if (result)
            {
                return Ok(new { message = "Node updated successfully" });
            }

            return BadRequest(new { message = "Failed to update node" });
        }

        [HttpDelete("node/{nodeId}")]
        public async Task<IActionResult> DeleteNode(string nodeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var node = await _nodeService.GetNodeById(nodeId);

            if (node == null || node.UserId != userId)
            {
                return NotFound(new { message = "Node not found or access denied." });
            }

            var result = await _nodeService.DeleteNode(nodeId);

            if (result)
            {
                return Ok(new { message = "Node deleted successfully" });
            }

            return BadRequest(new { message = "Failed to delete node" });
        }
    }
}
