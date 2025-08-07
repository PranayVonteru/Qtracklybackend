

using Demoproject.Dto_s;
using Demoproject.Dtos;

using Demoproject.Services.Interfaces;

using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Logging;

using System;

using System.Collections.Generic;

using System.Linq;

using System.Threading.Tasks;

namespace Demoproject.Controllers

{

    [Route("api/[controller]")]

    [ApiController]

    public class ManagerDashboardController : ControllerBase

    {

        private readonly ITimesheetService _timesheetService;

        private readonly ILogger<ManagerDashboardController> _logger;

        public ManagerDashboardController(

            ITimesheetService timesheetService,

            ILogger<ManagerDashboardController> logger)

        {

            _timesheetService = timesheetService;

            _logger = logger;

        }

        private string GetCurrentUserId()

        {

            var userId = User.FindFirst("oid")?.Value

                       ?? User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value

                       ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))

            {

                _logger.LogWarning("No user ID found in claims. Available claims: {Claims}",

                    string.Join(", ", User.Claims.Select(c => $"{c.Type}:{c.Value}")));

                throw new UnauthorizedAccessException("User ID not found in token claims");

            }

            return userId;

        }

        [HttpGet("weekly")]

        public async Task<ActionResult<List<WeeklyTimesheetDto>>> GetWeeklyTimesheets(

            [FromQuery] int year,

            [FromQuery] int month,

            [FromQuery] int week,

            [FromQuery] string department = "All Departments",

            [FromQuery] string subDepartment = "All Sub-Departments",

            [FromQuery] string manager = "All Managers")

        {

            try

            {

                var userId = GetCurrentUserId();

                var timesheets = await _timesheetService.GetWeeklyTimesheetsAsync(

                    userId,

                    year,

                    month,

                    week,

                    department,

                    subDepartment,

                    manager);

                return Ok(timesheets);

            }

            catch (Exception ex)

            {

                _logger.LogError(ex, "Error fetching weekly timesheets for year {Year}, month {Month}, week {Week}, department {Department}, subDepartment {SubDepartment}, manager {Manager}",

                    year, month, week, department, subDepartment, manager);

                return StatusCode(500, new { message = "An error occurred while fetching weekly timesheets.", error = ex.Message });

            }

        }

        [HttpGet("monthly")]

        public async Task<ActionResult<object>> GetMonthlyTimesheets(

            [FromQuery] int year,

            [FromQuery] int month,

            [FromQuery] string department = "All Departments",

            [FromQuery] string subDepartment = "All Sub-Departments",

            [FromQuery] string manager = "All Managers")

        {

            try

            {

                var userId = GetCurrentUserId();

                var timesheets = await _timesheetService.GetMonthlyTimesheetsAsync(

                    userId,

                    year,

                    month,

                    department,

                    subDepartment,

                    manager);

                return Ok(timesheets);

            }

            catch (Exception ex)

            {

                _logger.LogError(ex, "Error fetching monthly timesheets for year {Year}, month {Month}, department {Department}, subDepartment {SubDepartment}, manager {Manager}",

                    year, month, department, subDepartment, manager);

                return StatusCode(500, new { message = "An error occurred while fetching monthly timesheets.", error = ex.Message });

            }

        }

        [HttpGet("yearly")]

        public async Task<ActionResult<object>> GetYearlyTimesheets(

            [FromQuery] int year,

            [FromQuery] string department = "All Departments",

            [FromQuery] string subDepartment = "All Sub-Departments",

            [FromQuery] string manager = "All Managers")

        {

            try

            {

                var userId = GetCurrentUserId();

                var timesheets = await _timesheetService.GetYearlyTimesheetsAsync(

                    userId,

                    year,

                    department,

                    subDepartment,

                    manager);

                return Ok(timesheets);

            }

            catch (Exception ex)

            {

                _logger.LogError(ex, "Error fetching yearly timesheets for year {Year}, department {Department}, subDepartment {SubDepartment}, manager {Manager}",

                    year, department, subDepartment, manager);

                return StatusCode(500, new { message = "An error occurred while fetching yearly timesheets.", error = ex.Message });

            }

        }

        [HttpGet("daily")]

        public async Task<ActionResult<List<DailyTimesheetDto>>> GetDailyTimesheets(

            [FromQuery] DateTime date,

            [FromQuery] DateTime? endDate = null,

            [FromQuery] string department = "All Departments",

            [FromQuery] string subDepartment = "All Sub-Departments",

            [FromQuery] string manager = "All Managers")

        {

            try

            {

                var userId = GetCurrentUserId();

                var timesheets = await _timesheetService.GetDailyTimesheetsAsync(

                    userId,

                    date,

                    endDate,

                    department,

                    subDepartment,

                    manager);

                return Ok(timesheets);

            }

            catch (ArgumentOutOfRangeException ex)

            {

                _logger.LogWarning(ex, "Invalid date range for daily timesheets: date {Date}, endDate {EndDate}, department {Department}, subDepartment {SubDepartment}, manager {Manager}",

                    date, endDate, department, subDepartment, manager);

                return BadRequest(new { message = ex.Message });

            }

            catch (ArgumentException ex)

            {

                _logger.LogWarning(ex, "Invalid input for daily timesheets: date {Date}, endDate {EndDate}, department {Department}, subDepartment {SubDepartment}, manager {Manager}",

                    date, endDate, department, subDepartment, manager);

                return BadRequest(new { message = ex.Message });

            }

            catch (Exception ex)

            {

                _logger.LogError(ex, "Error fetching daily timesheets for date {Date}, endDate {EndDate}, department {Department}, subDepartment {SubDepartment}, manager {Manager}",

                    date, endDate, department, subDepartment, manager);

                return StatusCode(500, new { message = "An error occurred while fetching daily timesheets.", error = ex.Message });

            }

        }

        [HttpGet("filtered")]
        public async Task<ActionResult<List<TaskDto>>> GetTasks(string userId, string viewMode,int number, DateTime currentDate)
        {
            var tasks = await _timesheetService.GetFilteredTasksAsync(userId, viewMode, number,currentDate);
            return Ok(tasks);
        }

    }

}
