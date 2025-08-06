////using Microsoft.AspNetCore.Mvc;
////using Demoproject.Models;
////using Demoproject.Dto_s;
////using System.Threading.Tasks;
////using Microsoft.AspNetCore.Authorization;
////using System.Security.Claims;
////using Demoproject.Services.Interfaces;

////[ApiController]
////[Route("api/[controller]")]
//////[Authorize(Policy = "ManagerAccess")]
////public class FeedbackController : ControllerBase
////{
////    private readonly IFeedbackService _feedbackService;

////    public FeedbackController(IFeedbackService feedbackService)
////    {
////        _feedbackService = feedbackService;
////    }

////    [HttpPost]
////    public async Task<IActionResult> SendFeedback([FromBody] FeedbackDto feedbackDto)
////    {
////        if (!ModelState.IsValid)
////            return BadRequest(ModelState);

////        var managerId = User.FindFirst("oid")?.Value
////            ?? User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
////            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

////        if (string.IsNullOrEmpty(managerId))
////            return Unauthorized(new { message = "Unable to identify the manager." });

////        var feedback = new Feedback
////        {
////            UserId = feedbackDto.UserId,
////            ManagerId = managerId,
////            Message = feedbackDto.Message,
////            SentAt = DateTime.UtcNow
////        };

////        await _feedbackService.SendFeedbackAsync(feedback);
////        return Ok(new { message = "Feedback sent successfully." });
////    }

////    [HttpGet("{userId}")]
////    public async Task<IActionResult> GetFeedbacks(string userId)
////    {
////        var feedbacks = await _feedbackService.GetFeedbacksForUserAsync(userId);
////        return Ok(feedbacks);
////    }
////}







//using Microsoft.AspNetCore.Mvc;

//using Demoproject.Models;

//using Demoproject.Dto_s;

//using System.Threading.Tasks;

//using Microsoft.AspNetCore.Authorization;

//using System.Security.Claims;

//using System;

//using System.Linq;

//using System.Collections.Generic;
//using Demoproject.Services.Interfaces;

//[ApiController]

//[Route("api/[controller]")]

////[Authorize(Policy = "ManagerAccess")]

//public class FeedbackController : ControllerBase

//{

//    private readonly IFeedbackService _feedbackService;

//    public FeedbackController(IFeedbackService feedbackService)

//    {

//        _feedbackService = feedbackService;

//    }

//    [HttpPost]

//    public async Task<IActionResult> SendFeedback([FromBody] FeedbackDto feedbackDto)

//    {

//        if (!ModelState.IsValid)

//            return BadRequest(ModelState);

//        var managerId = User.FindFirst("oid")?.Value

//            ?? User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value

//            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

//        if (string.IsNullOrEmpty(managerId))

//            return Unauthorized(new { message = "Unable to identify the manager." });

//        var feedback = new Feedback

//        {

//            UserId = feedbackDto.UserId,

//            ManagerId = managerId,

//            Message = feedbackDto.Message,

//            SentAt = DateTime.UtcNow,

//            IsRead = false

//        };

//        await _feedbackService.SendFeedbackAsync(feedback);

//        return Ok(new { message = "Feedback sent successfully." });

//    }

//    [HttpPost("broadcast")]

//    public async Task<IActionResult> SendBroadcastFeedback([FromBody] BulkFeedbackDto bulkFeedbackDto)

//    {

//        if (!ModelState.IsValid)

//            return BadRequest(ModelState);

//        var managerId = User.FindFirst("oid")?.Value

//            ?? User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value

//            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

//        if (string.IsNullOrEmpty(managerId))

//            return Unauthorized(new { message = "Unable to identify the manager." });

//        if (bulkFeedbackDto.UserIds == null || !bulkFeedbackDto.UserIds.Any())

//            return BadRequest(new { message = "No user IDs provided." });

//        var feedbacks = bulkFeedbackDto.UserIds.Select(userId => new Feedback

//        {

//            UserId = userId,

//            ManagerId = managerId,

//            Message = bulkFeedbackDto.Message,

//            SentAt = DateTime.UtcNow,

//            IsRead = false,

//        }).ToList();

//        await _feedbackService.SendBulkFeedbackAsync(feedbacks);

//        return Ok(new { message = $"Feedback sent to {feedbacks.Count} user(s) successfully." });

//    }

//    // Returns only unread feedbacks (IsRead = 0)

//    [HttpGet("{userId}")]

//    public async Task<IActionResult> GetFeedbacks(string userId)

//    {

//        var feedbacks = await _feedbackService.GetUnreadFeedbacksForUserAsync(userId);

//        return Ok(feedbacks);

//    }

//    // Mark single feedback as read

//    [HttpPut("{feedbackId}/read")]

//    public async Task<IActionResult> MarkFeedbackAsRead(int feedbackId)

//    {

//        var success = await _feedbackService.MarkFeedbackAsReadAsync(feedbackId);

//        if (!success) return NotFound(new { message = "Feedback not found." });

//        return NoContent();

//    }

//    // Mark all feedbacks as read for user

//    [HttpPut("user/{userId}/read-all")]

//    public async Task<IActionResult> MarkAllFeedbacksAsRead(string userId)

//    {

//        var success = await _feedbackService.MarkAllFeedbacksAsReadAsync(userId);

//        if (!success) return NotFound(new { message = "No unread feedbacks found for user." });

//        return NoContent();

//    }

//}




using Microsoft.AspNetCore.Mvc;

using Demoproject.Models;

using Demoproject.Dto_s;

using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;

using System.Security.Claims;

using System;

using System.Linq;

using System.Collections.Generic;

using Demoproject.Services.Interfaces;

[ApiController]

[Route("api/[controller]")]

//[Authorize(Policy = "ManagerAccess")]

public class FeedbackController : ControllerBase

{

    private readonly IFeedbackService _feedbackService;

    public FeedbackController(IFeedbackService feedbackService)

    {

        _feedbackService = feedbackService;

    }

    [HttpPost]

    public async Task<IActionResult> SendFeedback([FromBody] FeedbackDto feedbackDto)

    {

        if (!ModelState.IsValid)

            return BadRequest(ModelState);

        var managerId = User.FindFirst("oid")?.Value

            ?? User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value

            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(managerId))

            return Unauthorized(new { message = "Unable to identify the manager." });

        var feedback = new Feedback

        {

            UserId = feedbackDto.UserId,

            ManagerId = managerId,

            Message = feedbackDto.Message,

            SentAt = DateTime.UtcNow,

            IsRead = false

        };

        await _feedbackService.SendFeedbackAsync(feedback);

        return Ok(new { message = "Feedback sent successfully." });

    }

    [HttpPost("broadcast")]

    public async Task<IActionResult> SendBroadcastFeedback([FromBody] BulkFeedbackDto bulkFeedbackDto)

    {

        if (!ModelState.IsValid)

            return BadRequest(ModelState);

        var managerId = User.FindFirst("oid")?.Value

            ?? User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value

            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(managerId))

            return Unauthorized(new { message = "Unable to identify the manager." });

        if (bulkFeedbackDto.UserIds == null || !bulkFeedbackDto.UserIds.Any())

            return BadRequest(new { message = "No user IDs provided." });

        var feedbacks = bulkFeedbackDto.UserIds.Select(userId => new Feedback

        {

            UserId = userId,

            ManagerId = managerId,

            Message = bulkFeedbackDto.Message,

            SentAt = DateTime.UtcNow,

            IsRead = false,

        }).ToList();

        await _feedbackService.SendBulkFeedbackAsync(feedbacks);

        return Ok(new { message = $"Feedback sent to {feedbacks.Count} user(s) successfully." });

    }

    // Returns only unread feedbacks (IsRead = 0)

    [HttpGet("{userId}")]

    public async Task<IActionResult> GetFeedbacks(string userId)

    {

        var feedbacks = await _feedbackService.GetUnreadFeedbacksForUserAsync(userId);

        return Ok(feedbacks);

    }

    // Mark single feedback as read

    [HttpPut("{feedbackId}/read")]

    public async Task<IActionResult> MarkFeedbackAsRead(int feedbackId)

    {

        var success = await _feedbackService.MarkFeedbackAsReadAsync(feedbackId);

        if (!success) return NotFound(new { message = "Feedback not found." });

        return NoContent();

    }

    // Mark all feedbacks as read for user

    [HttpPut("user/{userId}/read-all")]

    public async Task<IActionResult> MarkAllFeedbacksAsRead(string userId)

    {

        var success = await _feedbackService.MarkAllFeedbacksAsReadAsync(userId);

        if (!success) return NotFound(new { message = "No unread feedbacks found for user." });

        return NoContent();

    }

}

