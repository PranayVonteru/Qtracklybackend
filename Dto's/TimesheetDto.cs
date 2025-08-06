//using System;
//using System.Collections.Generic;

//namespace Demoproject.Dtos
//{
//    public class WeeklyTimesheetDto
//    {
//        public string Name { get; set; } = string.Empty;
//        public decimal Mon { get; set; }
//        public decimal Tue { get; set; }
//        public decimal Wed { get; set; }
//        public decimal Thu { get; set; }
//        public decimal Fri { get; set; }
//        public decimal Sat { get; set; }
//        public decimal Sun { get; set; }
//        public decimal Total { get; set; }
//    }

//    public class MonthlyTimesheetDto
//    {
//        public string Name { get; set; } = string.Empty;
//        public List<decimal> Days { get; set; } = new List<decimal>();
//    }

//    public class YearlyTimesheetDto
//    {
//        public string Name { get; set; } = string.Empty;
//        public decimal Jan { get; set; }
//        public decimal Feb { get; set; }
//        public decimal Mar { get; set; }
//        public decimal Apr { get; set; }
//        public decimal May { get; set; }
//        public decimal Jun { get; set; }
//        public decimal Jul { get; set; }
//        public decimal Aug { get; set; }
//        public decimal Sep { get; set; }
//        public decimal Oct { get; set; }
//        public decimal Nov { get; set; }
//        public decimal Dec { get; set; }
//        public decimal Total { get; set; }
//    }

//    public class DailyTimesheetDto
//    {
//        public string Name { get; set; } = string.Empty;
//        public DateTime Date { get; set; }
//        public decimal HoursWorked { get; set; }
//    }
//}

using System;
using System.Collections.Generic;

namespace Demoproject.Dtos
{
    public class WeeklyTimesheetDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Mon { get; set; }
        public decimal Tue { get; set; }
        public decimal Wed { get; set; }
        public decimal Thu { get; set; }
        public decimal Fri { get; set; }
        public decimal Sat { get; set; }
        public decimal Sun { get; set; }
        public decimal Total { get; set; }
    }

    public class MonthlyTimesheetDto
    {
        public string Name { get; set; } = string.Empty;
        public List<double> Days { get; set; } = new List<double>();
    }

    public class YearlyTimesheetDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Jan { get; set; }
        public decimal Feb { get; set; }
        public decimal Mar { get; set; }
        public decimal Apr { get; set; }
        public decimal May { get; set; }
        public decimal Jun { get; set; }
        public decimal Jul { get; set; }
        public decimal Aug { get; set; }
        public decimal Sep { get; set; }
        public decimal Oct { get; set; }
        public decimal Nov { get; set; }
        public decimal Dec { get; set; }
        public decimal Total { get; set; }
    }

    public class DailyTimesheetDto
    {
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal HoursWorked { get; set; }
    }
}