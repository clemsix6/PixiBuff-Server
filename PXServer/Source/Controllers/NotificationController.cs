using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PXServer.Source.Database.Notifications;
using PXServer.Source.Managers;
using PXServer.Source.Services;


namespace PXServer.Source.Controllers;


[ApiController]
[Route("notification")]
public class NotificationController : ControllerBase
{
    private readonly PlayerService playerService;
    private readonly NotificationManager notificationManager;


    public NotificationController(PlayerService playerService, NotificationManager notificationManager)
    {
        this.playerService = playerService;
        this.notificationManager = notificationManager;
    }


    [Authorize]
    [HttpGet("notifications")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<RuntimeNotification>>> GetNotifications()
    {
        var player = await this.playerService.GetPlayer(this.User);
        var notifications = await this.notificationManager.GetNotifications(player);
        return Ok(notifications);
    }
}