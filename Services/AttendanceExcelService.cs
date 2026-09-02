using System.Data;
using ClosedXML.Excel;
using OBMS.WebAPI.Models.DTO;
using OBMS.WebAPI.Models.Domain;

namespace OBMS.WebAPI.Services
{
    public interface IAttendanceExcelService
    {
        byte[] GenerateAttendanceTemplate(DateTime period, string branchCode);
        List<AttendanceBulkUploadDto> ParseAttendanceExcel(Stream excelStream);
        byte[] ExportAttendanceData(List<AttendanceBulkUploadDto> data);
        DataTable ConvertToDataTable(List<AttendanceBulkUploadDto> data);
        
        // Simplified format methods (for PDF-style bulk upload with P, W/O, H, L, NH codes)
        byte[] GenerateSimplifiedAttendanceTemplate(DateTime period, string branchCode);
        List<AttendanceBulkUploadDto> ParseSimplifiedAttendanceExcel(Stream excelStream);
        AttendanceBulkUploadDto ConvertSimplifiedToDetailed(AttendanceBulkUploadDto simplifiedDto);
    }

    public class AttendanceExcelService : IAttendanceExcelService
    {
        private readonly Dictionary<string, int> _workTypeMapping = new()
        {
            { "General Working", 1 },
            { "Off Day", 2 },
            { "Off Day Working", 3 },
            { "Holiday", 4 },
            { "Holiday Working", 5 },
            { "Annual Leave", 8 },
            { "Medical Leave", 9 },
            { "Maternity Leave", 10 },
            { "Paternity Leave", 11 },
            { "Hospitalization Leave", 12 },
            { "Rest Day", 13 },
            { "Unpaid Leave", 14 },
            { "Marriage Leave", 17 }
        };

        private readonly string[] _simplifiedCodes = { "P", "W/O", "H", "L", "NH" };

        /// <summary>
        /// Standardize period to last day of month for consistency
        /// </summary>
        private DateTime StandardizePeriod(DateTime inputPeriod)
        {
            return new DateTime(
                inputPeriod.Year,
                inputPeriod.Month,
                DateTime.DaysInMonth(inputPeriod.Year, inputPeriod.Month)
            );
        }

        public byte[] GenerateAttendanceTemplate(DateTime period, string branchCode)
        {
            var standardizedPeriod = StandardizePeriod(period);
            
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Attendance Template");
            
            // Add header information
            worksheet.Cell(1, 1).Value = "Attendance Bulk Upload Template";
            worksheet.Cell(2, 1).Value = $"Period: {standardizedPeriod:yyyy-MM}";
            worksheet.Cell(3, 1).Value = $"Branch: {branchCode}";
            worksheet.Cell(5, 1).Value = "Instructions:";
            worksheet.Cell(6, 1).Value = "1. Fill in employee details and daily attendance";
            worksheet.Cell(7, 1).Value = "2. Work Types: General Working, Off Day, Holiday, Annual Leave, Medical Leave, etc.";
            worksheet.Cell(8, 1).Value = "3. Time format: HH:mm (24-hour format)";
            worksheet.Cell(9, 1).Value = "4. Leave empty cells for non-working days";
            worksheet.Cell(11, 1).Value = "Employee Code";
            worksheet.Cell(11, 2).Value = "Employee Name";
            worksheet.Cell(11, 3).Value = "Branch Code";
            worksheet.Cell(11, 4).Value = "Period";
            worksheet.Cell(11, 5).Value = "Shift2 Type";
            worksheet.Cell(11, 6).Value = "Shift2 Rate";
            worksheet.Cell(11, 7).Value = "Allowance Deduction";
            worksheet.Cell(11, 8).Value = "Special Allowance Deduction";
            worksheet.Cell(11, 9).Value = "Bonus";

            // Add daily columns for the month
            int daysInMonth = DateTime.DaysInMonth(standardizedPeriod.Year, standardizedPeriod.Month);
            int currentColumn = 10;
            
            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(standardizedPeriod.Year, standardizedPeriod.Month, day);
                worksheet.Cell(10, currentColumn).Value = date.ToString("dd-MMM");
                worksheet.Cell(11, currentColumn).Value = "Work Type";
                worksheet.Cell(11, currentColumn + 1).Value = "Client";
                worksheet.Cell(11, currentColumn + 2).Value = "Time Start";
                worksheet.Cell(11, currentColumn + 3).Value = "Time End";
                worksheet.Cell(11, currentColumn + 4).Value = "OT Client";
                worksheet.Cell(11, currentColumn + 5).Value = "OT Start";
                worksheet.Cell(11, currentColumn + 6).Value = "OT End";
                worksheet.Cell(11, currentColumn + 7).Value = "Remarks";
                currentColumn += 8;
            }

            // Add styling
            var headerRange = worksheet.Range(11, 1, 11, currentColumn - 1);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            var titleRange = worksheet.Range(1, 1, 3, 1);
            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.FontSize = 14;

            // Add dropdown for work types
            var workTypes = string.Join(",", _workTypeMapping.Keys);
            for (int day = 1; day <= daysInMonth; day++)
            {
                int workTypeColumn = 10 + (day - 1) * 8;
                var workTypeRange = worksheet.Range(12, workTypeColumn, 100, workTypeColumn);
                var validation = workTypeRange.CreateDataValidation();
                validation.List(workTypes, true);
                validation.ErrorMessage = "Please select a valid work type";
                validation.ShowErrorMessage = true;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public List<AttendanceBulkUploadDto> ParseAttendanceExcel(Stream excelStream)
        {
            var results = new List<AttendanceBulkUploadDto>();
            
            using var workbook = new XLWorkbook(excelStream);
            var worksheet = workbook.Worksheets.FirstOrDefault();
            
            if (worksheet == null)
                throw new InvalidOperationException("No worksheet found in Excel file");

            // Find the header row (assuming row 11 based on template)
            var headerRow = worksheet.Row(11);
            var lastCell = headerRow.LastCellUsed();
            var totalColumns = lastCell?.Address.ColumnNumber ?? 10;
            
            // Parse each data row
            for (int row = 12; row <= worksheet.LastRowUsed().RowNumber(); row++)
            {
                try
                {
                    var attendanceDto = ParseAttendanceRow(worksheet, row, totalColumns);
                    if (attendanceDto != null)
                        results.Add(attendanceDto);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing row {row}: {ex.Message}");
                }
            }

            return results;
        }

        private AttendanceBulkUploadDto? ParseAttendanceRow(IXLWorksheet worksheet, int row, int totalColumns)
        {
            var employeeCode = worksheet.Cell(row, 1).GetString() ?? string.Empty;
            var employeeName = worksheet.Cell(row, 2).GetString() ?? string.Empty;
            
            if (string.IsNullOrWhiteSpace(employeeCode))
                return null;

            var attendanceDto = new AttendanceBulkUploadDto
            {
                EmployeeCode = employeeCode,
                EmployeeName = employeeName,
                BranchCode = worksheet.Cell(row, 3).GetString(),
                Period = StandardizePeriod(worksheet.Cell(row, 4).GetDateTime()),
                Shift2Type = worksheet.Cell(row, 5).GetValue<int>(),
                Shift2Rate = worksheet.Cell(row, 6).GetValue<decimal>(),
                AllowanceDeduction = worksheet.Cell(row, 7).GetValue<decimal>(),
                SpecialAllowanceDeduction = worksheet.Cell(row, 8).GetValue<decimal>(),
                Bonus = worksheet.Cell(row, 9).GetValue<decimal>()
            };

            // Parse daily attendance
            int daysInMonth = DateTime.DaysInMonth(attendanceDto.Period.Year, attendanceDto.Period.Month);
            
            for (int day = 1; day <= daysInMonth; day++)
            {
                int baseColumn = 10 + (day - 1) * 8;
                
                if (baseColumn + 7 > totalColumns)
                    break;

                var workType = worksheet.Cell(row, baseColumn).GetString();
                if (string.IsNullOrWhiteSpace(workType))
                    continue;

                var dayDetail = new AttendanceDayDetailDto
                {
                    AttendanceDate = new DateTime(attendanceDto.Period.Year, attendanceDto.Period.Month, day),
                    WorkType = workType,
                    AttendanceCode = workType, // Store original code/type
                    Client = worksheet.Cell(row, baseColumn + 1).GetString(),
                    TimeStart = GetTimeValue(worksheet.Cell(row, baseColumn + 2)),
                    TimeEnd = GetTimeValue(worksheet.Cell(row, baseColumn + 3)),
                    OTClient = worksheet.Cell(row, baseColumn + 4).GetString(),
                    OTTimeStart = GetTimeValue(worksheet.Cell(row, baseColumn + 5)),
                    OTTimeEnd = GetTimeValue(worksheet.Cell(row, baseColumn + 6)),
                    Remarks = worksheet.Cell(row, baseColumn + 7).GetString()
                };

                // Calculate working hours
                if (dayDetail.TimeStart.HasValue && dayDetail.TimeEnd.HasValue)
                {
                    dayDetail.WorkingHours = (decimal)(dayDetail.TimeEnd.Value - dayDetail.TimeStart.Value).TotalHours;
                }

                if (dayDetail.OTTimeStart.HasValue && dayDetail.OTTimeEnd.HasValue)
                {
                    dayDetail.OvertimeHours = (decimal)(dayDetail.OTTimeEnd.Value - dayDetail.OTTimeStart.Value).TotalHours;
                }

                attendanceDto.DailyAttendance.Add(dayDetail);
            }

            return attendanceDto;
        }

        private DateTime? GetTimeValue(IXLCell cell)
        {
            if (cell.IsEmpty())
                return null;

            var value = cell.GetString();
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (DateTime.TryParse(value, out var timeValue))
                return timeValue;

            if (TimeSpan.TryParse(value, out var timeSpan))
                return DateTime.Today.Add(timeSpan);

            return null;
        }

        public byte[] ExportAttendanceData(List<AttendanceBulkUploadDto> data)
        {
            var dataTable = ConvertToDataTable(data);
            
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Attendance Data");
            
            // Load data into worksheet
            worksheet.Cell(1, 1).InsertTable(dataTable);
            
            // Style the header
            var headerRange = worksheet.Range(1, 1, 1, dataTable.Columns.Count);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            
            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public DataTable ConvertToDataTable(List<AttendanceBulkUploadDto> data)
        {
            var dataTable = new DataTable();
            
            // Add columns
            dataTable.Columns.Add("Employee Code", typeof(string));
            dataTable.Columns.Add("Employee Name", typeof(string));
            dataTable.Columns.Add("Branch Code", typeof(string));
            dataTable.Columns.Add("Period", typeof(DateTime));
            dataTable.Columns.Add("Shift2 Type", typeof(int));
            dataTable.Columns.Add("Shift2 Rate", typeof(decimal));
            dataTable.Columns.Add("Allowance Deduction", typeof(decimal));
            dataTable.Columns.Add("Special Allowance Deduction", typeof(decimal));
            dataTable.Columns.Add("Bonus", typeof(decimal));
            dataTable.Columns.Add("Total Working Days", typeof(int));
            dataTable.Columns.Add("Total Working Hours", typeof(decimal));
            dataTable.Columns.Add("Total Overtime Hours", typeof(decimal));
            dataTable.Columns.Add("Leave Days", typeof(int));

            // Add rows
            foreach (var attendance in data)
            {
                var workingDays = attendance.DailyAttendance.Count(d => 
                    !d.WorkType.Contains("Leave") && d.WorkType != "Off Day");
                var totalHours = attendance.DailyAttendance.Sum(d => d.WorkingHours);
                var overtimeHours = attendance.DailyAttendance.Sum(d => d.OvertimeHours);
                var leaveDays = attendance.DailyAttendance.Count(d => d.WorkType.Contains("Leave"));

                dataTable.Rows.Add(
                    attendance.EmployeeCode,
                    attendance.EmployeeName,
                    attendance.BranchCode,
                    attendance.Period,
                    attendance.Shift2Type,
                    attendance.Shift2Rate,
                    attendance.AllowanceDeduction,
                    attendance.SpecialAllowanceDeduction,
                    attendance.Bonus,
                    workingDays,
                    totalHours,
                    overtimeHours,
                    leaveDays
                );
            }

            return dataTable;
        }

        public byte[] GenerateSimplifiedAttendanceTemplate(DateTime period, string branchCode)
        {
            var standardizedPeriod = StandardizePeriod(period);
            
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Attendance Template");
            
            // Add header information matching PDF format
            worksheet.Cell(1, 1).Value = "Maragathammbal Industrial and Logistics Parks Pvt Ltd";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 14;
            
            worksheet.Cell(2, 1).Value = $"PWG Pvt Ltd {branchCode}";
            worksheet.Cell(2, 1).Style.Font.Bold = true;
            
            worksheet.Cell(3, 1).Value = $"Period: {standardizedPeriod:yyyy-MM}";
            worksheet.Cell(3, 1).Style.Font.Bold = true;
            
            // Format period as yyyy-MM for Excel cells
            string periodFormatted = standardizedPeriod.ToString("yyyy-MM");

            // Add instructions
            worksheet.Cell(5, 1).Value = "Instructions:";
            worksheet.Cell(6, 1).Value = "1. Fill in employee details and daily attendance";
            worksheet.Cell(7, 1).Value = "2. Attendance Codes: P=Present, W/O=Weekly Off, H=Holiday, L=Leave, NH=National Holiday";
            worksheet.Cell(8, 1).Value = "3. Leave cells empty for non-working days";
            worksheet.Cell(9, 1).Value = "4. Summary columns will be calculated automatically";
            worksheet.Cell(10, 1).Value = "5. DO NOT modify the Period column - it is pre-filled";

            // Add header row - Simplified format with only essential columns
            int headerRow = 11;
            worksheet.Cell(headerRow, 1).Value = "Employee Name";
            worksheet.Cell(headerRow, 2).Value = "Employee Code";
            worksheet.Cell(headerRow, 3).Value = "Branch Code";
            worksheet.Cell(headerRow, 4).Value = "Period";

            // Add daily columns for the month
            int daysInMonth = DateTime.DaysInMonth(standardizedPeriod.Year, standardizedPeriod.Month);
            int currentColumn = 5;
            
            for (int day = 1; day <= daysInMonth; day++)
            {
                worksheet.Cell(headerRow, currentColumn).Value = day;
                currentColumn++;
            }

            // Add summary columns matching PDF format
            worksheet.Cell(headerRow, currentColumn).Value = "Duty";
            worksheet.Cell(headerRow, currentColumn + 1).Value = "W/O";
            worksheet.Cell(headerRow, currentColumn + 2).Value = "NH/H";
            worksheet.Cell(headerRow, currentColumn + 3).Value = "L";
            worksheet.Cell(headerRow, currentColumn + 4).Value = "Total";

            // Add timing columns for work hours tracking
            int timeStartCol = currentColumn + 5;
            int timeEndCol = currentColumn + 6;
            worksheet.Cell(headerRow, timeStartCol).Value = "Start Time";
            worksheet.Cell(headerRow, timeEndCol).Value = "End Time";

            // Add instruction note for time columns
            worksheet.Cell(6, 1).Value = "6. Start Time / End Time: Enter as HH:mm (e.g. 08:00 / 17:00) — applies to all Present (P) days for this employee";

            // Style the header — blue base for all columns
            var headerRange = worksheet.Range(headerRow, 1, headerRow, timeEndCol);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Override timing columns with green to distinguish them
            worksheet.Cell(headerRow, timeStartCol).Style.Fill.BackgroundColor = XLColor.LightGreen;
            worksheet.Cell(headerRow, timeEndCol).Style.Fill.BackgroundColor = XLColor.LightGreen;

            // Pre-fill period column for 100 data rows (protects from accidental modification)
            for (int row = headerRow + 1; row <= headerRow + 100; row++)
            {
                var periodCell = worksheet.Cell(row, 4);
                periodCell.Value = periodFormatted;
                periodCell.Style.Font.FontColor = XLColor.DarkGray;
                periodCell.Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            // Add dropdown for attendance codes - Daily columns
            for (int day = 1; day <= daysInMonth; day++)
            {
                int dayColumn = 5 + (day - 1);
                var codeRange = worksheet.Range(headerRow + 1, dayColumn, headerRow + 100, dayColumn);
                var validation = codeRange.CreateDataValidation();
                validation.List("P,W/O,H,L,NH", true);
                validation.ErrorMessage = "Please use: P, W/O, H, L, or NH";
                validation.ShowErrorMessage = true;
                validation.ErrorTitle = "Invalid Attendance Code";
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public List<AttendanceBulkUploadDto> ParseSimplifiedAttendanceExcel(Stream excelStream)
        {
            var results = new List<AttendanceBulkUploadDto>();
            var validationErrors = new List<string>();
            
            try
            {
                // Validate stream
                if (excelStream == null || excelStream.Length == 0)
                    throw new InvalidOperationException("Excel file stream is empty or null");

                using var workbook = new XLWorkbook(excelStream);
                var worksheet = workbook.Worksheets.FirstOrDefault();
                
                if (worksheet == null)
                    throw new InvalidOperationException("No worksheet found in Excel file. Ensure the file has at least one sheet.");

                // Find the header row
                int headerRow = FindHeaderRow(worksheet);
                if (headerRow == -1)
                    throw new InvalidOperationException(
                        "Header row not found. Please ensure your Excel file follows the template format. " +
                        "Header should contain 'Employee Name' in first column.");

                var lastUsedRow = worksheet.LastRowUsed();
                if (lastUsedRow == null || lastUsedRow.RowNumber() <= headerRow)
                    throw new InvalidOperationException("No data rows found after header row. Please add attendance records.");

                var totalColumns = worksheet.LastColumnUsed()?.ColumnNumber() ?? 20;
                
                // Parse each data row
                int parsedCount = 0;
                for (int row = headerRow + 1; row <= lastUsedRow.RowNumber(); row++)
                {
                    try
                    {
                        var attendanceDto = ParseSimplifiedAttendanceRow(worksheet, row, headerRow, totalColumns);
                        if (attendanceDto != null)
                        {
                            results.Add(attendanceDto);
                            parsedCount++;
                        }
                    }
                    catch (InvalidOperationException validEx)
                    {
                        validationErrors.Add($"Row {row}: {validEx.Message}");
                    }
                    catch (Exception ex)
                    {
                        validationErrors.Add($"Row {row}: Unexpected error - {ex.Message}");
                    }
                }

                // Report validation errors
                if (validationErrors.Any())
                {
                    var errorSummary = $"Found {validationErrors.Count} validation issues:\n" +
                        string.Join("\n", validationErrors.Take(10)); // Show first 10 errors
                    
                    if (validationErrors.Count > 10)
                        errorSummary += $"\n...and {validationErrors.Count - 10} more errors";

                    throw new InvalidOperationException(errorSummary);
                }

                if (results.Count == 0)
                    throw new InvalidOperationException(
                        "No valid attendance records found in file. Please ensure employee data is properly filled.");

                return results;
            }
            catch (Exception ex)
            {
                if (ex.GetType().Name.Contains("XLException") || ex.GetType().Name.Contains("Excel"))
                {
                    throw new InvalidOperationException($"Invalid Excel file format: {ex.Message}. Please ensure the file is a valid Excel (.xlsx) file.", ex);
                }
                throw new InvalidOperationException(
                    $"Error parsing Excel file: {ex.Message}", ex);
            }
        }

        private int FindHeaderRow(IXLWorksheet worksheet)
        {
            for (int row = 1; row <= 20; row++)
            {
                var cellValue = worksheet.Cell(row, 1).GetString() ?? string.Empty;
                if (cellValue.Contains("Employee Name", StringComparison.OrdinalIgnoreCase) ||
                    cellValue.Contains("Name of Contraction", StringComparison.OrdinalIgnoreCase) ||
                    cellValue.Contains("Contraction", StringComparison.OrdinalIgnoreCase))
                    return row;
            }
            return -1;
        }

        private AttendanceBulkUploadDto? ParseSimplifiedAttendanceRow(
            IXLWorksheet worksheet,
            int row,
            int headerRow,
            int totalColumns)
        {
            var employeeName = worksheet.Cell(row, 1).GetString() ?? string.Empty;
            
            // Skip empty rows
            if (string.IsNullOrWhiteSpace(employeeName))
                return null;

            // Validate required fields
            var employeeCode = worksheet.Cell(row, 2).GetString() ?? string.Empty;
            var branchCode = worksheet.Cell(row, 3).GetString() ?? string.Empty;
            var periodCell = worksheet.Cell(row, 4);
            
            var validationErrors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(employeeCode))
                validationErrors.Add("Employee Code (Column B) is required and cannot be empty");
                
            if (string.IsNullOrWhiteSpace(branchCode))
                validationErrors.Add("Branch Code (Column C) is required and cannot be empty");
                
            if (periodCell == null || periodCell.IsEmpty())
                validationErrors.Add("Period (Column D) is required");
            
            if (validationErrors.Any())
            {
                throw new InvalidOperationException(
                    $"Row {row} (Employee: {employeeName}) - Validation Failed:\n" +
                    string.Join("\n", validationErrors.Select(e => "  • " + e)));
            }

            DateTime period;
            try
            {
                period = ParsePeriodValue(periodCell, row);
                period = StandardizePeriod(period); // Standardize immediately after parsing
            }
            catch (InvalidOperationException periodEx)
            {
                throw new InvalidOperationException(
                    $"Row {row} (Employee: {employeeName}) - {periodEx.Message}");
            }

            var attendanceDto = new AttendanceBulkUploadDto
            {
                EmployeeName = employeeName.Trim(),
                EmployeeCode = employeeCode.Trim(),
                BranchCode = branchCode.Trim(),
                Period = period,
                Shift2Type = 0,
                Shift2Rate = 0,
                AllowanceDeduction = 0,
                SpecialAllowanceDeduction = 0,
                Bonus = 0
            };

            // Parse daily attendance codes
            int daysInMonth = DateTime.DaysInMonth(attendanceDto.Period.Year, attendanceDto.Period.Month);
            var attendanceCodeErrors = new List<string>();
            
            for (int day = 1; day <= daysInMonth; day++)
            {
                int dayColumn = 5 + (day - 1);
                
                if (dayColumn > totalColumns)
                    break;

                var code = worksheet.Cell(row, dayColumn).GetString();
                if (!string.IsNullOrWhiteSpace(code))
                {
                    // Validate attendance code
                    var upperCode = code.ToUpper().Trim();
                    if (!_simplifiedCodes.Contains(upperCode))
                    {
                        attendanceCodeErrors.Add(
                            $"Day {day} (Column {ExcelColumnName(dayColumn)}): Invalid code '{code}'. " +
                            $"Valid codes are: P (Present), W/O (Weekly Off), H (Holiday), L (Leave), NH (National Holiday)");
                    }
                    else
                    {
                        attendanceDto.DailyAttendanceCodes[day] = upperCode;
                    }
                }
            }

            if (attendanceCodeErrors.Any())
            {
                throw new InvalidOperationException(
                    $"Row {row} (Employee: {employeeName}) - Invalid Attendance Codes:\n" +
                    string.Join("\n", attendanceCodeErrors.Select(e => "  • " + e)));
            }

            // Validate that at least some attendance data is present
            if (attendanceDto.DailyAttendanceCodes.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Row {row} (Employee: {employeeName}) - No attendance data found. " +
                    $"Please fill at least one day's attendance code.");
            }

            // Parse Start Time and End Time columns (after summary columns: col 5+daysInMonth+0..4, so time at +5 and +6)
            int summaryStartCol = 5 + daysInMonth;
            int timeStartColIdx = summaryStartCol + 5;
            int timeEndColIdx = summaryStartCol + 6;

            var startTimeStr = worksheet.Cell(row, timeStartColIdx).GetString()?.Trim();
            var endTimeStr = worksheet.Cell(row, timeEndColIdx).GetString()?.Trim();

            // Store using Unit/Designation as temporary carriers for start/end time strings
            // (these fields are unused in simplified flow and will be consumed in ConvertSimplifiedToDetailed)
            if (!string.IsNullOrWhiteSpace(startTimeStr))
                attendanceDto.Unit = startTimeStr;
            if (!string.IsNullOrWhiteSpace(endTimeStr))
                attendanceDto.Designation = endTimeStr;

            // Calculate summary fields
            CalculateSummaryFields(attendanceDto);

            return attendanceDto;
        }

        /// <summary>
        /// Convert column number to Excel column letter (1=A, 2=B, etc.)
        /// </summary>
        private string ExcelColumnName(int columnNumber)
        {
            if (columnNumber <= 0)
                return "?";
            
            string columnName = "";
            while (columnNumber > 0)
            {
                columnNumber--;
                columnName = Convert.ToChar('A' + columnNumber % 26) + columnName;
                columnNumber /= 26;
            }
            return columnName;
        }

        private DateTime ParsePeriodValue(IXLCell periodCell, int row)
        {
            try
            {
                // Try to parse as DateTime first
                if (periodCell.DataType == XLDataType.DateTime)
                {
                    return periodCell.GetDateTime();
                }

                // Try parsing as string in various formats
                var periodString = periodCell.GetString()?.Trim();
                if (string.IsNullOrWhiteSpace(periodString))
                {
                    throw new InvalidOperationException("Period cell is empty");
                }

                // Try parsing yyyy-MM format
                if (DateTime.TryParseExact(
                    periodString,
                    "yyyy-MM",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var parsedDate))
                {
                    return parsedDate;
                }

                // Try parsing full date format yyyy-MM-dd
                if (DateTime.TryParseExact(
                    periodString,
                    "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out parsedDate))
                {
                    return parsedDate;
                }

                // Try parsing with current culture
                if (DateTime.TryParse(periodString, out parsedDate))
                {
                    return parsedDate;
                }

                throw new InvalidOperationException(
                    $"Period format '{periodString}' is invalid. Expected format: yyyy-MM or yyyy-MM-dd");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Row {row} has invalid period format. {ex.Message}");
            }
        }

        private void CalculateSummaryFields(AttendanceBulkUploadDto attendanceDto)
        {
            if (attendanceDto?.DailyAttendanceCodes == null)
                return;

            attendanceDto.Duty = attendanceDto.DailyAttendanceCodes.Count(x => x.Value == "P");
            attendanceDto.WeeklyOff = attendanceDto.DailyAttendanceCodes.Count(x => x.Value == "W/O");
            attendanceDto.HolidayOrNationalHoliday = attendanceDto.DailyAttendanceCodes.Count(
                x => x.Value == "H" || x.Value == "NH");
            attendanceDto.Leave = attendanceDto.DailyAttendanceCodes.Count(x => x.Value == "L");
            attendanceDto.Total = attendanceDto.DailyAttendanceCodes.Count;
        }

        /// <summary>
        /// Calculate working hours based on attendance code
        /// </summary>
        private decimal CalculateWorkingHours(string code, DateTime? timeStart, DateTime? timeEnd)
        {
            // If actual times provided, calculate from times
            if (timeStart.HasValue && timeEnd.HasValue)
            {
                return (decimal)(timeEnd.Value - timeStart.Value).TotalHours;
            }

            // Otherwise use code-based defaults
            return code switch
            {
                "P" => 8m,        // Present = 8 hours
                "W/O" => 0m,      // Weekly Off = 0 hours
                "H" => 0m,        // Holiday = 0 hours
                "L" => 0m,        // Absent = 0 hours (no pay)
                "NH" => 0m,       // National Holiday = 0 hours
                _ => 0m
            };
        }

        public AttendanceBulkUploadDto ConvertSimplifiedToDetailed(AttendanceBulkUploadDto simplifiedDto)
        {
            if (simplifiedDto?.DailyAttendanceCodes == null || simplifiedDto.DailyAttendanceCodes.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Employee {simplifiedDto?.EmployeeCode} has no attendance data");
            }

            var detailedDto = new AttendanceBulkUploadDto
            {
                EmployeeCode = simplifiedDto.EmployeeCode,
                EmployeeName = simplifiedDto.EmployeeName,
                BranchCode = simplifiedDto.BranchCode,
                Period = simplifiedDto.Period,
                Shift2Type = simplifiedDto.Shift2Type,
                Shift2Rate = simplifiedDto.Shift2Rate,
                AllowanceDeduction = simplifiedDto.AllowanceDeduction,
                SpecialAllowanceDeduction = simplifiedDto.SpecialAllowanceDeduction,
                Bonus = simplifiedDto.Bonus,
                Unit = simplifiedDto.Unit,
                Designation = simplifiedDto.Designation,
                DailyAttendanceCodes = new Dictionary<int, string>(simplifiedDto.DailyAttendanceCodes),
                Duty = simplifiedDto.Duty,
                WeeklyOff = simplifiedDto.WeeklyOff,
                HolidayOrNationalHoliday = simplifiedDto.HolidayOrNationalHoliday,
                Leave = simplifiedDto.Leave,
                Total = simplifiedDto.Total,
                ClientCode = simplifiedDto.ClientCode  // propagate for period resolution
            };

            // Parse start/end time strings carried in Unit/Designation fields
            DateTime? parsedTimeStart = null;
            DateTime? parsedTimeEnd = null;
            decimal? parsedWorkingHours = null;

            if (!string.IsNullOrWhiteSpace(simplifiedDto.Unit) &&
                TimeSpan.TryParse(simplifiedDto.Unit, out var tsStart))
            {
                parsedTimeStart = DateTime.Today.Add(tsStart); // placeholder date; date replaced per day below
            }

            if (!string.IsNullOrWhiteSpace(simplifiedDto.Designation) &&
                TimeSpan.TryParse(simplifiedDto.Designation, out var tsEnd))
            {
                parsedTimeEnd = DateTime.Today.Add(tsEnd);
            }

            if (parsedTimeStart.HasValue && parsedTimeEnd.HasValue)
            {
                var diff = parsedTimeEnd.Value - parsedTimeStart.Value;
                if (diff.TotalHours > 0)
                    parsedWorkingHours = (decimal)diff.TotalHours;
            }

            // Convert daily codes to detailed attendance format with proper type mapping
            foreach (var kvp in simplifiedDto.DailyAttendanceCodes)
            {
                var day = kvp.Key;
                var code = kvp.Value?.ToUpper() ?? "P";
                
                // Get standardized type from code mapping
                int typeId = AttendanceCodeMapping.GetAttendanceType(code);
                string workTypeName = ConvertIntToWorkType(typeId);

                // Build per-day time values anchored to the actual attendance date
                DateTime attendanceDate = new DateTime(
                    simplifiedDto.Period.Year,
                    simplifiedDto.Period.Month,
                    day
                );

                DateTime? dayTimeStart = null;
                DateTime? dayTimeEnd = null;
                decimal workingHours;

                // Apply start/end times only to working-type days
                bool isWorkingDay = code == "P";
                if (isWorkingDay && parsedTimeStart.HasValue && parsedTimeEnd.HasValue)
                {
                    dayTimeStart = attendanceDate.Date.Add(parsedTimeStart.Value.TimeOfDay);
                    dayTimeEnd = attendanceDate.Date.Add(parsedTimeEnd.Value.TimeOfDay);
                    workingHours = parsedWorkingHours ?? CalculateWorkingHours(code, null, null);
                }
                else
                {
                    workingHours = CalculateWorkingHours(code, null, null);
                }
                
                var dayDetail = new AttendanceDayDetailDto
                {
                    AttendanceDate = attendanceDate,
                    WorkType = workTypeName,
                    AttendanceCode = code,
                    TypeID = typeId,
                    Client = string.Empty,
                    TimeStart = dayTimeStart,
                    TimeEnd = dayTimeEnd,
                    OTClient = string.Empty,
                    OTTimeStart = null,
                    OTTimeEnd = null,
                    Remarks = $"Code: {code}",
                    WorkingHours = workingHours,
                    OvertimeHours = 0
                };

                detailedDto.DailyAttendance.Add(dayDetail);
            }

            return detailedDto;
        }

        public string ConvertIntToWorkType(int workType)
        {
            return _workTypeMapping.FirstOrDefault(x => x.Value == workType).Key ?? "General Working";
        }
    }
}
