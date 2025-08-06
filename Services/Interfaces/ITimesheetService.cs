//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Demoproject.Dtos;

//namespace Demoproject.Services.Interfaces
//{
//    public interface ITimesheetService
//    {
//        Task<List<WeeklyTimesheetDto>> GetWeeklyTimesheetsAsync(string userId, string team, int year, int month, int week);
//        Task<List<MonthlyTimesheetDto>> GetMonthlyTimesheetsAsync(string userId, string team, int year, int month);
//        Task<List<YearlyTimesheetDto>> GetYearlyTimesheetsAsync(string userId, string team, int year);
//    }
//}

//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Demoproject.Dtos;

//namespace Demoproject.Services.Interfaces
//{
//    public interface ITimesheetService
//    {
//        Task<List<WeeklyTimesheetDto>> GetWeeklyTimesheetsAsync(string userId, string team, int year, int month, int week);
//        Task<List<MonthlyTimesheetDto>> GetMonthlyTimesheetsAsync(string userId, string team, int year, int month);
//        Task<List<YearlyTimesheetDto>> GetYearlyTimesheetsAsync(string userId, string team, int year);
//        Task<List<DailyTimesheetDto>> GetDailyTimesheetsAsync(string userId, string team, DateTime date, DateTime? endDate = null);
//    }
//}

using Demoproject.Dto_s;
using Demoproject.Dtos;

using System;

using System.Collections.Generic;

using System.Threading.Tasks;

namespace Demoproject.Services.Interfaces

{

    public interface ITimesheetService

    {

        Task<List<WeeklyTimesheetDto>> GetWeeklyTimesheetsAsync(

            string userId,

            int year,

            int month,

            int week,

            string department = "All Departments",

            string subDepartment = "All Sub-Departments",

            string manager = "All Managers");

        Task<object> GetMonthlyTimesheetsAsync(

            string userId,

            int year,

            int month,

            string department = "All Departments",

            string subDepartment = "All Sub-Departments",

            string manager = "All Managers");

        Task<object> GetYearlyTimesheetsAsync(

            string userId,

            int year,

            string department = "All Departments",

            string subDepartment = "All Sub-Departments",

            string manager = "All Managers");

        Task<List<DailyTimesheetDto>> GetDailyTimesheetsAsync(

            string userId,

            DateTime date,

            DateTime? endDate = null,

            string department = "All Departments",

            string subDepartment = "All Sub-Departments",

            string manager = "All Managers");

        Task<List<TaskDto>> GetFilteredTasksAsync(
            string userId,
            string viewMode,
            int number,
            DateTime currentDate);

    }

      

}

