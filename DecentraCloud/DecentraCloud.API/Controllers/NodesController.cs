using DecentraCloud.API.DTOs;
using DecentraCloud.API.Helpers;
using DecentraCloud.API.Interfaces.RepositoryInterfaces;
using DecentraCloud.API.Interfaces.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DecentraCloud.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NodesController : ControllerBase
    {
        private readonly INodeService _nodeService;
        private readonly TokenHelper _tokenHelper;
        private readonly INodeRepository _nodeRepository;

        public NodesController(INodeService nodeService, TokenHelper tokenHelper, INodeRepository nodeRepository)
        {
            _nodeService = nodeService;
            _tokenHelper = tokenHelper;
            _nodeRepository = nodeRepository;
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyNode([FromBody] NodeVerificationDto nodeVerificationDto)
        {
            try
            {
                var nodeExists = await _nodeService.VerifyNode(nodeVerificationDto.Email, nodeVerificationDto.NodeName);
                if (nodeExists)
                {
                    return BadRequest(new { message = "Node already exists. Please authenticate or choose a different name." });
                }

                return Ok(new { message = "Node name is available." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterNode([FromBody] NodeRegistrationDto nodeRegistrationDto)
        {
            try
            {
                var node = await _nodeService.RegisterNode(nodeRegistrationDto);
                return Ok(node);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginNode([FromBody] NodeLoginDto nodeLoginDto)
        {
            try
            {
                var token = await _nodeService.LoginNode(nodeLoginDto);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("status")]
        [Authorize]
        public async Task<IActionResult> UpdateNodeStatus([FromBody] NodeStatusDto nodeStatusDto)
        {
            var result = await _nodeService.UpdateNodeStatus(nodeStatusDto);

            if (!result)
            {
                return BadRequest(new { message = "Failed to update node status." });
            }

            return Ok(new { message = "Node status updated successfully." });
        }

        [HttpGet("all")]
        [Authorize]
        public async Task<IActionResult> GetAllNodes(int pageNumber = 1, int pageSize = 20)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var nodes = await _nodeService.GetNodesByUser(userId, pageNumber, pageSize);
            return Ok(nodes);
        }

        // New PingNode endpoint
        [HttpGet("ping/{nodeId}")]
        [Authorize]
        public async Task<IActionResult> PingNode(string nodeId)
        {
            var node = await _nodeService.GetNodeById(nodeId);
            if (node == null)
            {
                return NotFound(new { message = "Node not found." });
            }

            var isOnline = await _nodeService.PingNode(nodeId);
            return Ok(new { nodeId, isOnline });
        }
    }
}
