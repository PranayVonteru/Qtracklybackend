

using Demoproject.Data;
using Demoproject.Dto_s;
using Demoproject.Dtos;
using Demoproject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demoproject.Services
{
    public class TimesheetService : ITimesheetService
    {
        private readonly QTraklyDBContext _context;
        private readonly ILogger<TimesheetService> _logger;
        private readonly string _aesKey = "placeholder_key"; // Replace with actual key management
        private readonly string _aesIV = "placeholder_iv";  // Replace with actual IV management

        public TimesheetService(QTraklyDBContext context, ILogger<TimesheetService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private string DecryptString(string encryptedValue, string aesKey, string aesIV)
        {
            // Placeholder: Implement actual decryption logic
            return encryptedValue;
        }

        public async Task<List<WeeklyTimesheetDto>> GetWeeklyTimesheetsAsync(
            string userId,
            int year,
            int month,
            int week,
            string department = "All Departments",
            string subDepartment = "All Sub-Departments",
            string manager = "All Managers")
        {
            try
            {
                var ist = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                if (month < 1 || month > 12)
                    throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");

                var firstDayOfMonthLocal = new DateTime(year, month, 1);
                var startDateLocal = firstDayOfMonthLocal.AddDays((week - 1) * 7);

                if (startDateLocal.Month != month)
                {
                    startDateLocal = new DateTime(year, month, DateTime.DaysInMonth(year, month));
                }

                var endDateLocal = startDateLocal.AddDays(6);
                var lastDayOfMonthLocal = new DateTime(year, month, DateTime.DaysInMonth(year, month));

                if (endDateLocal > lastDayOfMonthLocal)
                    endDateLocal = lastDayOfMonthLocal;

                var startDateUtc = TimeZoneInfo.ConvertTimeToUtc(startDateLocal, ist);
                var endDateUtc = TimeZoneInfo.ConvertTimeToUtc(endDateLocal.Date.AddDays(1).AddTicks(-1), ist);

                var query = from log in _context.TaskDateworkedHours
                            join task in _context.Tasks on log.TaskId equals task.Id
                            join user in _context.Users on task.CreatedBy equals user.UserId
                            where log.DateTime >= startDateUtc && log.DateTime <= endDateUtc
                            && (department == "All Departments" || user.Department == department)
                            && (subDepartment == "All Sub-Departments" || user.SubDepartment == subDepartment)
                            && (manager == "All Managers" || user.Manager == manager)
                            select new { log, user };

                var rawData = await query.ToListAsync();

                var timesheets = rawData
                    .GroupBy(x => x.user.Name)
                    .Select(g =>
                    {
                        var items = g.ToList();
                        return new WeeklyTimesheetDto
                        {
                            Name = DecryptString(g.Key, _aesKey, _aesIV),
                            Mon = items.Where(x => TimeZoneInfo.ConvertTimeFromUtc(x.log.DateTime, ist).DayOfWeek == DayOfWeek.Monday).Sum(x => x.log.WorkedHours),
                            Tue = items.Where(x => TimeZoneInfo.ConvertTimeFromUtc(x.log.DateTime, ist).DayOfWeek == DayOfWeek.Tuesday).Sum(x => x.log.WorkedHours),
                            Wed = items.Where(x => TimeZoneInfo.ConvertTimeFromUtc(x.log.DateTime, ist).DayOfWeek == DayOfWeek.Wednesday).Sum(x => x.log.WorkedHours),
                            Thu = items.Where(x => TimeZoneInfo.ConvertTimeFromUtc(x.log.DateTime, ist).DayOfWeek == DayOfWeek.Thursday).Sum(x => x.log.WorkedHours),
                            Fri = items.Where(x => TimeZoneInfo.ConvertTimeFromUtc(x.log.DateTime, ist).DayOfWeek == DayOfWeek.Friday).Sum(x => x.log.WorkedHours),
                            Sat = items.Where(x => TimeZoneInfo.ConvertTimeFromUtc(x.log.DateTime, ist).DayOfWeek == DayOfWeek.Saturday).Sum(x => x.log.WorkedHours),
                            Sun = items.Where(x => TimeZoneInfo.ConvertTimeFromUtc(x.log.DateTime, ist).DayOfWeek == DayOfWeek.Sunday).Sum(x => x.log.WorkedHours),
                            Total = items.Sum(x => x.log.WorkedHours)
                        };
                    })
                    .ToList();

                return timesheets;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching weekly timesheets for user {UserId}, year {Year}, month {Month}, week {Week}, department {Department}, subDepartment {SubDepartment}, manager {Manager}",
                    userId, year, month, week, department, subDepartment, manager);
                throw;
            }
        }

        public async Task<object> GetMonthlyTimesheetsAsync(
            string userId,
            int year,
            int month,
            string department = "All Departments",
            string subDepartment = "All Sub-Departments",
            string manager = "All Managers")
        {
            try
            {
                var ist = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                if (month < 1 || month > 12)
                    throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");

                var startDateLocal = new DateTime(year, month, 1);
                var endDateLocal = new DateTime(year, month, DateTime.DaysInMonth(year, month));

                var startDateUtc = TimeZoneInfo.ConvertTimeToUtc(startDateLocal, ist);
                var endDateUtc = TimeZoneInfo.ConvertTimeToUtc(endDateLocal.Date.AddDays(1).AddTicks(-1), ist);

                var query = from log in _context.TaskDateworkedHours
                            join task in _context.Tasks on log.TaskId equals task.Id
                            join user in _context.Users on task.CreatedBy equals user.UserId
                            where log.DateTime >= startDateUtc && log.DateTime <= endDateUtc
                            && (department == "All Departments" || user.Department == department)
                            && (subDepartment == "All Sub-Departments" || user.SubDepartment == subDepartment)
                            && (manager == "All Managers" || user.Manager == manager)
                            select new { log, user };

                var rawData = await query.ToListAsync();

                var daysInMonth = DateTime.DaysInMonth(year, month);

                var timesheets = rawData
                    .GroupBy(x => x.user.Name)
                    .Select(g =>
                    {
                        var items = g.ToList();
                        var dailyHours = new List<double>();

                        for (int day = 1; day <= daysInMonth; day++)
                        {
                            var totalHours = items.Where(x => TimeZoneInfo.ConvertTimeFromUtc(x.log.DateTime, ist).Day == day)
                                .Sum(x => x.log.WorkedHours);

                            dailyHours.Add((double)totalHours);
                        }

                        return new MonthlyTimesheetDto
                        {
                            Name = DecryptString(g.Key, _aesKey, _aesIV),
                            Days = dailyHours
                        };
                    })
                    .ToList();

                return timesheets;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching monthly timesheets for user {UserId}, year {Year}, month {Month}, department {Department}, subDepartment {SubDepartment}, manager {Manager}",
                    userId, year, month, department, subDepartment, manager);
                throw;
            }
        }

        public async Task<object> GetYearlyTimesheetsAsync(
            string userId,
            int year,
            string department = "All Departments",
            string subDepartment = "All Sub-Departments",
            string manager = "All Managers")
        {
            try
            {
                var ist = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                var startDateLocal = new DateTime(year, 1, 1);
                var endDateLocal = new DateTime(year, 12, 31);

                var startDateUtc = TimeZoneInfo.ConvertTimeToUtc(startDateLocal, ist);
                var endDateUtc = TimeZoneInfo.ConvertTimeToUtc(endDateLocal.Date.AddDays(1).AddTicks(-1), ist);

                var query = from log in _context.TaskDateworkedHours
                            join task in _context.Tasks on log.TaskId equals task.Id
                            join user in _context.Users on task.CreatedBy equals user.UserId
                            where log.DateTime >= startDateUtc && log.DateTime <= endDateUtc
                            && (department == "All Departments" || user.Department == department)
                            && (subDepartment == "All Sub-Departments" || user.SubDepartment == subDepartment)
                            && (manager == "All Managers" || user.Manager == manager)
                            select new { log, user };

                var rawData = await query.ToListAsync();

                var timesheets = rawData
                    .GroupBy(x => x.user.Name)
                    .Select(g =>
                    {
                        var items = g.ToList();
                        return new YearlyTimesheetDto
                        {
                            Name = DecryptString(g.Key, _aesKey, _aesIV),
                            Jan = items.Where(x => TimeZoneInfo.ConvertTimeFromUtc(x.log.DateTime, ist).Month == 1).Sum(x => x.log.WorkedHours),
                            Feb = items.Where(x => TimeZoneInfo.ConvertTimeFromUtc(x.log.DateTime, ist).Month == 2).Sum(x => x.log.WorkedHours),
                            Mar = items.Where(x => TimeZoneInfo.ConvertTimeFromUtc(x.log.DateTime, ist).Month == 3).Sum(x => x.log.WorkedHours),
                            Apr = items.Where(x => TimeZoneInfo.ConvertTimeFromUtc(x.log.DateTime, ist).Month == 4).Sum(x => x.log.WorkedHours),
                            May = items.Where(x => TimeZoneInfo.ConvertTimeFromUtc(x.log.DateTime, ist).Month == 5).Sum(x => x.log.WorkedHours),
                            Jun = items.Where(x => TimeZoneInfo.ConvertTimeFromUtc(x.log.DateTime, ist).Month == 6).Sum(x => x.log.WorkedHours),
                            Jul = items.Where(x => TimeZoneInfo.ConvertTimeFromUtc(x.log.DateTime, ist).Month == 7).Sum(x => x.log.WorkedHours),
                            Aug = items.Where(x => TimeZoneInfo.ConvertTimeFromUtc(x.log.DateTime, ist).Month == 8).Sum(x => x.log.WorkedHours),
                            Sep = items.Where(x => TimeZoneInfo.ConvertTimeFromUtc(x.log.DateTime, ist).Month == 9).Sum(x => x.log.WorkedHours),
                            Oct = items.Where(x => TimeZoneInfo.ConvertTimeFromUtc(x.log.DateTime, ist).Month == 10).Sum(x => x.log.WorkedHours),
                            Nov = items.Where(x => TimeZoneInfo.ConvertTimeFromUtc(x.log.DateTime, ist).Month == 11).Sum(x => x.log.WorkedHours),
                            Dec = items.Where(x => TimeZoneInfo.ConvertTimeFromUtc(x.log.DateTime, ist).Month == 12).Sum(x => x.log.WorkedHours),
                            Total = items.Sum(x => x.log.WorkedHours)
                        };
                    })
                    .ToList();

                return timesheets;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching yearly timesheets for user {UserId}, year {Year}, department {Department}, subDepartment {SubDepartment}, manager {Manager}",
                    userId, year, department, subDepartment, manager);
                throw;
            }
        }

        public async Task<List<DailyTimesheetDto>> GetDailyTimesheetsAsync(
            string userId,
            DateTime date,
            DateTime? endDate = null,
            string department = "All Departments",
            string subDepartment = "All Sub-Departments",
            string manager = "All Managers")
        {
            try
            {
                var ist = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                if (date.Year < 2000 || date.Year > DateTime.UtcNow.Year + 1)
                    throw new ArgumentOutOfRangeException(nameof(date), "Date is out of valid range.");

                var startDateLocal = date.Date;
                var endDateLocal = endDate?.Date ?? startDateLocal;

                if (endDateLocal < startDateLocal)
                    throw new ArgumentException("End date cannot be earlier than start date.", nameof(endDate));

                var startDateUtc = TimeZoneInfo.ConvertTimeToUtc(startDateLocal, ist);
                var endDateUtc = TimeZoneInfo.ConvertTimeToUtc(endDateLocal.Date.AddDays(1).AddTicks(-1), ist);

                var query = from log in _context.TaskDateworkedHours
                            join task in _context.Tasks on log.TaskId equals task.Id
                            join user in _context.Users on task.CreatedBy equals user.UserId
                            where log.DateTime >= startDateUtc && log.DateTime <= endDateUtc
                            && (department == "All Departments" || user.Department == department)
                            && (subDepartment == "All Sub-Departments" || user.SubDepartment == subDepartment)
                            && (manager == "All Managers" || user.Manager == manager)
                            select new { log, user };

                var rawData = await query.ToListAsync();

                var timesheets = rawData
                    .GroupBy(x => new { x.user.Name, Date = TimeZoneInfo.ConvertTimeFromUtc(x.log.DateTime, ist).Date })
                    .Select(g => new DailyTimesheetDto
                    {
                        Name = DecryptString(g.Key.Name, _aesKey, _aesIV),
                        Date = g.Key.Date,
                        HoursWorked = g.Sum(x => x.log.WorkedHours)
                    })
                    .OrderBy(x => x.Date)
                    .ThenBy(x => x.Name)
                    .ToList();

                return timesheets;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching daily timesheets for user {UserId}, date {Date}, endDate {EndDate}, department {Department}, subDepartment {SubDepartment}, manager {Manager}",
                    userId, date, endDate, department, subDepartment, manager);
                throw;
            }
        }

        public async Task<List<TaskDto>> GetFilteredTasksAsync(
       string userId,
       string viewMode,
       int number,
       DateTime currentDate)
        {
            try
            {
                _logger.LogInformation("Fetching tasks for user {UserId}, viewMode {ViewMode}, number {Number}, currentDate {CurrentDate}",
                    userId, viewMode, number, currentDate);

                var ist = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                int year = currentDate.Year;

                DateTime startDateLocal;
                DateTime endDateLocal;

                switch (viewMode?.ToLower())
                {
                    case "week":
                        if (number < 1 || number > 53)
                            throw new ArgumentOutOfRangeException(nameof(number), "Week number must be between 1 and 53.");
                        startDateLocal = new DateTime(year, 1, 1);
                        if (startDateLocal.DayOfWeek != DayOfWeek.Monday)
                            startDateLocal = startDateLocal.AddDays(8 - (int)startDateLocal.DayOfWeek);
                        startDateLocal = startDateLocal.AddDays((number - 1) * 7);
                        endDateLocal = startDateLocal.AddDays(6);
                        if (endDateLocal.Year > year)
                            endDateLocal = new DateTime(year, 12, 31);
                        break;
                    case "month":
                        if (number < 1 || number > 12)
                            throw new ArgumentOutOfRangeException(nameof(number), "Month number must be between 1 and 12.");
                        startDateLocal = new DateTime(year, number, 1);
                        endDateLocal = startDateLocal.AddMonths(1).AddTicks(-1);
                        break;
                    case "quarter":
                    default:
                        if (number < 1 || number > 4)
                            throw new ArgumentOutOfRangeException(nameof(number), "Quarter number must be between 1 and 4.");
                        int quarterStartMonth = (number - 1) * 3 + 1;
                        startDateLocal = new DateTime(year, quarterStartMonth, 1);
                        endDateLocal = startDateLocal.AddMonths(3).AddTicks(-1);
                        break;
                }

                var startDateUtc = TimeZoneInfo.ConvertTimeToUtc(startDateLocal, ist);
                var endDateUtc = TimeZoneInfo.ConvertTimeToUtc(endDateLocal, ist);

                _logger.LogInformation("Querying tasks between {StartDateUtc} and {EndDateUtc} for user {UserId}", startDateUtc, endDateUtc, userId);

                var query = from task in _context.Tasks
                            join user in _context.Users on task.CreatedBy equals user.UserId into userJoin
                            from user in userJoin.DefaultIfEmpty()
                            where (task.StartDate <= endDateUtc && task.DueDate >= startDateUtc)
                            && (userId == null || task.CreatedBy == userId)
                            select new TaskDto
                            {
                                Id = task.Id,
                                TaskName = task.TaskName ?? "Unnamed Task",
                                Description = task.Description ?? string.Empty,
                                StartDate = task.StartDate,
                                DueDate = task.DueDate,
                                Status = task.Status ?? "Not Started",
                                Priority = task.Priority ?? "Medium",
                                AssignedTo = user != null ? user.Name : "Unassigned",
                                CreatedBy = task.CreatedBy
                            };

                var tasks = await query.ToListAsync();

                // Apply decryption to AssignedTo field after query execution
                foreach (var task in tasks)
                {
                    if (!string.IsNullOrEmpty(task.AssignedTo) && task.AssignedTo != "Unassigned")
                    {
                        task.AssignedTo = DecryptString(task.AssignedTo, _aesKey, _aesIV);
                    }
                }

                _logger.LogInformation("Retrieved {TaskCount} tasks", tasks.Count);

                if (tasks.Count == 0)
                {
                    _logger.LogWarning("No tasks found for the given criteria. StartDateUtc: {StartDateUtc}, EndDateUtc: {EndDateUtc}, UserId: {UserId}, ViewMode: {ViewMode}, Number: {Number}",
                        startDateUtc, endDateUtc, userId, viewMode ?? "quarter (default)", number);
                }

                return tasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching filtered tasks for user {UserId}, viewMode {ViewMode}, number {Number}, currentDate {CurrentDate}",
                    userId, viewMode, number, currentDate);
                throw;
            }
        }
    }

    
}