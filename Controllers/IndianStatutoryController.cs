using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using OBMS.WebAPI.Services;
using OBMS.WebAPI.Models.Domain;
using OBMS.WebAPI.Models;

namespace OBMS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/statutory")]
    public class IndianStatutoryController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly IGSTCalculationService _gstCalculationService;
        private readonly OBMSDbContext _context;
        private readonly IPFCalculationService _pfService;

        public IndianStatutoryController(IConfiguration configuration, IGSTCalculationService gstCalculationService, OBMSDbContext context, IPFCalculationService pfService)
        {
            _connectionString = configuration.GetConnectionString("obms") 
                ?? "Server=localhost;Database=obmsuat;Trusted_Connection=true;";
            _gstCalculationService = gstCalculationService;
            _context = context;
            _pfService = pfService;
        }

        #region PF Configuration

        [HttpGet("pf-configuration")]
        public async Task<IActionResult> GetPFConfiguration()
        {
            try
            {
                var pfConfigurations = new List<object>();
                
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = @"
                        SELECT Id, Basic_Salary_Limit, Employee_Rate, Employer_Rate, Employer_EPS_Rate,
                               Effective_Date, Is_Active, Created_Date, Financial_Year
                        FROM PFConfiguration 
                        WHERE Is_Active = 1
                        ORDER BY Effective_Date DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                pfConfigurations.Add(new
                                {
                                    Id = reader["Id"],
                                    BasicSalaryLimit = reader["Basic_Salary_Limit"],
                                    EmployeeContributionRate = reader["Employee_Rate"],
                                    EmployerContributionRate = reader["Employer_Rate"],
                                    EmployerEPSRate = reader["Employer_EPS_Rate"],
                                    EffectiveDate = reader["Effective_Date"],
                                    IsActive = reader["Is_Active"],
                                    CreatedDate = reader["Created_Date"],
                                    FinancialYear = reader["Financial_Year"]
                                });
                            }
                        }
                    }
                }

                return Ok(pfConfigurations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving PF configuration", error = ex.Message });
            }
        }

        [HttpPost("pf-configuration")]
        public async Task<IActionResult> SavePFConfiguration([FromBody] dynamic pfConfig)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = @"
                        INSERT INTO PFConfiguration 
                        (Employee_Rate, Employer_Rate, Employer_EPS_Rate, Basic_Salary_Limit,
                         Effective_Date, Is_Active, Created_Date, Financial_Year)
                        VALUES (@EmployeeRate, @EmployerRate, @EmployerEPSRate, @BasicSalaryLimit,
                                @EffectiveDate, @IsActive, GETDATE(), @FinancialYear)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@EmployeeRate", (decimal)pfConfig.EmployeeContributionRate);
                        command.Parameters.AddWithValue("@EmployerRate", (decimal)pfConfig.EmployerContributionRate);
                        command.Parameters.AddWithValue("@EmployerEPSRate", (decimal)pfConfig.EmployerEPSRate);
                        command.Parameters.AddWithValue("@BasicSalaryLimit", (decimal)pfConfig.BasicSalaryLimit);
                        command.Parameters.AddWithValue("@EffectiveDate", DateTime.Parse(pfConfig.EffectiveDate.ToString()));
                        command.Parameters.AddWithValue("@IsActive", true);
                        command.Parameters.AddWithValue("@FinancialYear", pfConfig.FinancialYear?.ToString() ?? "2024-2025");

                        await command.ExecuteNonQueryAsync();
                    }
                }

                return Ok(new { message = "PF configuration saved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error saving PF configuration", error = ex.Message });
            }
        }

        [HttpPut("pf-configuration/{id}")]
        public async Task<IActionResult> UpdatePFConfiguration(int id, [FromBody] dynamic pfConfig)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = @"
                        UPDATE PFConfiguration 
                        SET Employee_Rate = @EmployeeRate,
                            Employer_Rate = @EmployerRate,
                            Employer_EPS_Rate = @EmployerEPSRate,
                            Basic_Salary_Limit = @BasicSalaryLimit,
                            Effective_Date = @EffectiveDate,
                            LastUpdatedDate = GETDATE(),
                            Financial_Year = @FinancialYear
                        WHERE Id = @Id";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.Parameters.AddWithValue("@EmployeeRate", (decimal)pfConfig.EmployeeContributionRate);
                        command.Parameters.AddWithValue("@EmployerRate", (decimal)pfConfig.EmployerContributionRate);
                        command.Parameters.AddWithValue("@EmployerEPSRate", (decimal)pfConfig.EmployerEPSRate);
                        command.Parameters.AddWithValue("@BasicSalaryLimit", (decimal)pfConfig.BasicSalaryLimit);
                        command.Parameters.AddWithValue("@EffectiveDate", DateTime.Parse(pfConfig.EffectiveDate.ToString()));
                        command.Parameters.AddWithValue("@FinancialYear", pfConfig.FinancialYear?.ToString() ?? "2024-2025");

                        await command.ExecuteNonQueryAsync();
                    }
                }

                return Ok(new { message = "PF configuration updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating PF configuration", error = ex.Message });
            }
        }

        [HttpDelete("pf-configuration/{id}")]
        public async Task<IActionResult> DeletePFConfiguration(int id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = "DELETE FROM PFConfiguration WHERE Id = @Id";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        await command.ExecuteNonQueryAsync();
                    }
                }

                return Ok(new { message = "PF configuration deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting PF configuration", error = ex.Message });
            }
        }

        #endregion

        #region ESI Configuration

        [HttpGet("esi-configuration")]
        public async Task<IActionResult> GetESIConfiguration()
        {
            try
            {
                var esiConfigurations = new List<object>();
                
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = @"
                        SELECT Id, Gross_Salary_Limit, Employee_Rate, Employer_Rate,
                               Effective_Date, Is_Active, Created_Date, Financial_Year
                        FROM ESIConfiguration 
                        WHERE Is_Active = 1
                        ORDER BY Effective_Date DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                esiConfigurations.Add(new
                                {
                                    Id = reader["Id"],
                                    GrossSalaryLimit = reader["Gross_Salary_Limit"],
                                    EmployeeContributionRate = reader["Employee_Rate"],
                                    EmployerContributionRate = reader["Employer_Rate"],
                                    EffectiveDate = reader["Effective_Date"],
                                    IsActive = reader["Is_Active"],
                                    CreatedDate = reader["Created_Date"],
                                    FinancialYear = reader["Financial_Year"]
                                });
                            }
                        }
                    }
                }

                return Ok(esiConfigurations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving ESI configuration", error = ex.Message });
            }
        }

        [HttpPost("esi-configuration")]
        public async Task<IActionResult> SaveESIConfiguration([FromBody] dynamic esiConfig)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = @"
                        INSERT INTO ESIConfiguration 
                        (GrossSalaryLimit, EmployeeContributionRate, EmployerContributionRate,
                         EffectiveDate, IsActive, CreatedBy, CreatedDate)
                        VALUES (@GrossSalaryLimit, @EmployeeContributionRate, @EmployerContributionRate,
                                @EffectiveDate, @IsActive, @CreatedBy, GETDATE())";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@GrossSalaryLimit", (decimal)esiConfig.GrossSalaryLimit);
                        command.Parameters.AddWithValue("@EmployeeContributionRate", (decimal)esiConfig.EmployeeContributionRate);
                        command.Parameters.AddWithValue("@EmployerContributionRate", (decimal)esiConfig.EmployerContributionRate);
                        command.Parameters.AddWithValue("@EffectiveDate", DateTime.Parse(esiConfig.EffectiveDate.ToString()));
                        command.Parameters.AddWithValue("@IsActive", true);
                        command.Parameters.AddWithValue("@CreatedBy", "SYSTEM");

                        await command.ExecuteNonQueryAsync();
                    }
                }

                return Ok(new { message = "ESI configuration saved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error saving ESI configuration", error = ex.Message });
            }
        }

        [HttpPut("esi-configuration/{id}")]
        public async Task<IActionResult> UpdateESIConfiguration(int id, [FromBody] dynamic esiConfig)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = @"
                        UPDATE ESIConfiguration 
                        SET GrossSalaryLimit = @GrossSalaryLimit,
                            EmployeeContributionRate = @EmployeeContributionRate,
                            EmployerContributionRate = @EmployerContributionRate,
                            EffectiveDate = @EffectiveDate,
                            LastUpdatedDate = GETDATE(),
                            LastUpdatedBy = @LastUpdatedBy
                        WHERE Id = @Id";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.Parameters.AddWithValue("@GrossSalaryLimit", (decimal)esiConfig.GrossSalaryLimit);
                        command.Parameters.AddWithValue("@EmployeeContributionRate", (decimal)esiConfig.EmployeeContributionRate);
                        command.Parameters.AddWithValue("@EmployerContributionRate", (decimal)esiConfig.EmployerContributionRate);
                        command.Parameters.AddWithValue("@EffectiveDate", DateTime.Parse(esiConfig.EffectiveDate.ToString()));
                        command.Parameters.AddWithValue("@LastUpdatedBy", "SYSTEM");

                        await command.ExecuteNonQueryAsync();
                    }
                }

                return Ok(new { message = "ESI configuration updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating ESI configuration", error = ex.Message });
            }
        }

        [HttpDelete("esi-configuration/{id}")]
        public async Task<IActionResult> DeleteESIConfiguration(int id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = "DELETE FROM ESIConfiguration WHERE Id = @Id";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        await command.ExecuteNonQueryAsync();
                    }
                }

                return Ok(new { message = "ESI configuration deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting ESI configuration", error = ex.Message });
            }
        }

        #endregion

        #region Professional Tax Configuration

        [HttpGet("pt-configuration")]
        public async Task<IActionResult> GetPTConfiguration()
        {
            try
            {
                var ptConfigurations = new List<object>();
                
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = @"
                        SELECT 
                            Id, 
                            State as state, 
                            MinSalary as minSalary,
                            MaxSalary as maxSalary,
                            TaxAmount as taxAmount, 
                            EffectiveDate as effectiveDate, 
                            IsActive as isActive, 
                            CreatedDate as createdDate,
                            CreatedBy as createdBy,
                            LastUpdatedDate as lastUpdatedDate,
                            LastUpdatedBy as lastUpdatedBy,
                            NULL as notes
                        FROM ProfessionalTaxConfiguration 
                        WHERE IsActive = 1
                        ORDER BY state, minSalary";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                ptConfigurations.Add(new
                                {
                                    Id = reader["Id"],
                                    state = reader["state"],
                                    minSalary = reader["minSalary"],
                                    maxSalary = reader["maxSalary"] != DBNull.Value ? reader["maxSalary"] : null,
                                    taxAmount = reader["taxAmount"],
                                    effectiveDate = reader["effectiveDate"],
                                    isActive = reader["isActive"],
                                    createdDate = reader["createdDate"],
                                    createdBy = reader["createdBy"] != DBNull.Value ? reader["createdBy"] : "SYSTEM",
                                    lastUpdatedDate = reader["lastUpdatedDate"] != DBNull.Value ? reader["lastUpdatedDate"] : null,
                                    lastUpdatedBy = reader["lastUpdatedBy"] != DBNull.Value ? reader["lastUpdatedBy"] : null,
                                    notes = reader["notes"] != DBNull.Value ? reader["notes"] : null
                                });
                            }
                        }
                    }
                }

                return Ok(ptConfigurations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving PT configuration", error = ex.Message });
            }
        }

        [HttpPost("pt-configuration")]
        public async Task<IActionResult> SavePTConfiguration([FromBody] dynamic ptConfig)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = @"
                        INSERT INTO ProfessionalTaxConfiguration 
                        (State, MinSalary, MaxSalary, TaxAmount, EffectiveDate, IsActive, CreatedBy, CreatedDate)
                        VALUES (@State, @MinSalary, @MaxSalary, @TaxAmount, @EffectiveDate, @IsActive, @CreatedBy, GETDATE())";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@State", (string)ptConfig.State);
                        command.Parameters.AddWithValue("@MinSalary", (decimal)ptConfig.MinSalary);
                        
                        if (ptConfig.MaxSalary != null)
                            command.Parameters.AddWithValue("@MaxSalary", (decimal)ptConfig.MaxSalary);
                        else
                            command.Parameters.AddWithValue("@MaxSalary", DBNull.Value);
                            
                        command.Parameters.AddWithValue("@TaxAmount", (decimal)ptConfig.TaxAmount);
                        command.Parameters.AddWithValue("@EffectiveDate", DateTime.Parse(ptConfig.EffectiveDate.ToString()));
                        command.Parameters.AddWithValue("@IsActive", true);
                        command.Parameters.AddWithValue("@CreatedBy", "SYSTEM");

                        await command.ExecuteNonQueryAsync();
                    }
                }

                return Ok(new { message = "Professional Tax configuration saved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error saving PT configuration", error = ex.Message });
            }
        }

        [HttpPut("pt-configuration/{id}")]
        public async Task<IActionResult> UpdatePTConfiguration(int id, [FromBody] dynamic ptConfig)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = @"
                        UPDATE ProfessionalTaxConfiguration 
                        SET State = @State,
                            MinSalary = @MinSalary,
                            MaxSalary = @MaxSalary,
                            TaxAmount = @TaxAmount,
                            EffectiveDate = @EffectiveDate,
                            LastUpdatedDate = GETDATE(),
                            LastUpdatedBy = @LastUpdatedBy
                        WHERE Id = @Id";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.Parameters.AddWithValue("@State", (string)ptConfig.State);
                        command.Parameters.AddWithValue("@MinSalary", (decimal)ptConfig.MinSalary);
                        
                        if (ptConfig.MaxSalary != null)
                            command.Parameters.AddWithValue("@MaxSalary", (decimal)ptConfig.MaxSalary);
                        else
                            command.Parameters.AddWithValue("@MaxSalary", DBNull.Value);
                            
                        command.Parameters.AddWithValue("@TaxAmount", (decimal)ptConfig.TaxAmount);
                        command.Parameters.AddWithValue("@EffectiveDate", DateTime.Parse(ptConfig.EffectiveDate.ToString()));
                        command.Parameters.AddWithValue("@LastUpdatedBy", "SYSTEM");

                        await command.ExecuteNonQueryAsync();
                    }
                }

                return Ok(new { message = "Professional Tax configuration updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating PT configuration", error = ex.Message });
            }
        }

        [HttpDelete("pt-configuration/{id}")]
        public async Task<IActionResult> DeletePTConfiguration(int id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = @"
                        UPDATE ProfessionalTaxConfiguration 
                        SET IsActive = 0,
                            LastUpdatedDate = GETDATE(),
                            LastUpdatedBy = @LastUpdatedBy
                        WHERE Id = @Id";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.Parameters.AddWithValue("@LastUpdatedBy", "SYSTEM");

                        await command.ExecuteNonQueryAsync();
                    }
                }

                return Ok(new { message = "Professional Tax configuration deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting PT configuration", error = ex.Message });
            }
        }

        [HttpGet("pt-calculate")]
        public async Task<IActionResult> CalculateProfessionalTax([FromQuery] decimal grossSalary, [FromQuery] string state, [FromQuery] DateTime? calculationDate = null)
        {
            try
            {
                var date = calculationDate ?? DateTime.Now;
                var ptService = new ProfessionalTaxService(_context);
                var result = ptService.CalculateProfessionalTax(grossSalary, state, date);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error calculating professional tax", error = ex.Message });
            }
        }

        #endregion

        #region TDS Configuration

        [HttpGet("tds-configuration")]
        public async Task<IActionResult> GetTDSConfiguration()
        {
            try
            {
                var tdsConfigurations = new List<object>();
                
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = @"
                        SELECT Id, Financial_Year, Min_Income, Max_Income, Tax_Rate, 
                               Surcharge_Rate, Education_Cess_Rate, Higher_Education_Cess_Rate,
                               Is_Active, Created_Date
                        FROM TDSSlabConfiguration 
                        WHERE Is_Active = 1
                        ORDER BY Min_Income";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                tdsConfigurations.Add(new
                                {
                                    Id = reader["Id"],
                                    FinancialYear = reader["Financial_Year"],
                                    MinIncome = reader["Min_Income"],
                                    MaxIncome = reader["Max_Income"] != DBNull.Value ? reader["Max_Income"] : null,
                                    TaxRate = reader["Tax_Rate"],
                                    SurchargeRate = reader["Surcharge_Rate"] != DBNull.Value ? reader["Surcharge_Rate"] : null,
                                    EducationCessRate = reader["Education_Cess_Rate"] != DBNull.Value ? reader["Education_Cess_Rate"] : null,
                                    HigherEducationCessRate = reader["Higher_Education_Cess_Rate"] != DBNull.Value ? reader["Higher_Education_Cess_Rate"] : null,
                                    IsActive = reader["Is_Active"],
                                    CreatedDate = reader["Created_Date"]
                                });
                            }
                        }
                    }
                }

                return Ok(tdsConfigurations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving TDS configuration", error = ex.Message });
            }
        }

        #endregion

        #region GST Configuration

        [HttpGet("gst-configuration")]
        public async Task<IActionResult> GetGSTConfiguration()
        {
            try
            {
                // Return simplified GST slab structure
                var gstSlabData = new List<object>
                {
                    new {
                        supplyType = "Intrastate",
                        fromState = "Same State",
                        toState = "Same State", 
                        taxApplied = "CGST + SGST",
                        taxSplit = "GST ÷ 2 + GST ÷ 2"
                    },
                    new {
                        supplyType = "Interstate",
                        fromState = "Different State",
                        toState = "Different State",
                        taxApplied = "IGST", 
                        taxSplit = "Full GST"
                    }
                };

                return Ok(gstSlabData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving GST configuration", error = ex.Message });
            }
        }

        [HttpGet("gst-configuration-with-services")]
        public async Task<IActionResult> GetGSTConfigurationWithServices()
        {
            try
            {
                var gstConfigurations = new List<object>();
                
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = @"
                        SELECT 
                            gc.Id, gc.GSTRate, gc.HSNCode, gc.Description, 
                            gc.IsActive, gc.CreatedDate, gc.CreatedBy,
                            gc.LastUpdatedDate, gc.LastUpdatedBy,
                            STUFF((
                                SELECT ', ' + st.ServiceName + ' (' + st.ServiceCode + ')'
                                FROM ServiceType st 
                                WHERE st.GSTRateID = gc.Id AND st.IsActive = 1
                                FOR XML PATH(''), TYPE
                            ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS LinkedServices,
                            (
                                SELECT COUNT(*) 
                                FROM ServiceType st 
                                WHERE st.GSTRateID = gc.Id AND st.IsActive = 1
                            ) AS ServiceCount
                        FROM GSTConfiguration gc 
                        WHERE gc.IsActive = 1
                        ORDER BY gc.GSTRate";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                gstConfigurations.Add(new
                                {
                                    Id = reader["Id"],
                                    GSTRate = reader["GSTRate"],
                                    HSNCode = reader["HSNCode"],
                                    Description = reader["Description"] != DBNull.Value ? reader["Description"] : null,
                                    IsActive = reader["IsActive"],
                                    CreatedDate = reader["CreatedDate"],
                                    CreatedBy = reader["CreatedBy"],
                                    LastUpdatedDate = reader["LastUpdatedDate"] != DBNull.Value ? reader["LastUpdatedDate"] : null,
                                    LastUpdatedBy = reader["LastUpdatedBy"] != DBNull.Value ? reader["LastUpdatedBy"] : null,
                                    LinkedServices = reader["LinkedServices"] != DBNull.Value ? reader["LinkedServices"] : "",
                                    ServiceCount = reader["ServiceCount"]
                                });
                            }
                        }
                    }
                }

                return Ok(new { 
                    message = "GST configurations with linked service types",
                    totalGSTConfigs = gstConfigurations.Count,
                    data = gstConfigurations 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving GST configuration with services", error = ex.Message });
            }
        }

        #endregion

        #region Indian States

        [HttpGet("indian-states")]
        public async Task<IActionResult> GetIndianStates()
        {
            try
            {
                var states = new List<object>();
                
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = @"
                        SELECT Id, StateName, StateCode, IsActive, CreatedDate, CreatedBy,
                               LastUpdatedDate, LastUpdatedBy
                        FROM IndianStates 
                        WHERE IsActive = 1
                        ORDER BY StateName";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                states.Add(new
                                {
                                    Id = reader["Id"],
                                    StateName = reader["StateName"],
                                    StateCode = reader["StateCode"],
                                    IsActive = reader["IsActive"],
                                    CreatedDate = reader["CreatedDate"],
                                    CreatedBy = reader["CreatedBy"],
                                    LastUpdatedDate = reader["LastUpdatedDate"] != DBNull.Value ? reader["LastUpdatedDate"] : null,
                                    LastUpdatedBy = reader["LastUpdatedBy"] != DBNull.Value ? reader["LastUpdatedBy"] : null
                                });
                            }
                        }
                    }
                }

                return Ok(states);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving Indian states", error = ex.Message });
            }
        }

        #endregion

        #region GST Calculations

        [HttpPost("calculate-gst")]
        public async Task<IActionResult> CalculateGST([FromBody] dynamic request)
        {
            try
            {
                decimal amount = request.Amount;
                string hsnCode = request.HSNCode;
                string supplierStateCode = request.SupplierStateCode;
                string recipientStateCode = request.RecipientStateCode;
                DateTime invoiceDate = DateTime.Parse(request.InvoiceDate.ToString());

                // Calculate GST using injected service
                var result = _gstCalculationService.CalculateGST(amount, hsnCode, supplierStateCode, recipientStateCode, invoiceDate);

                return Ok(new
                {
                    Amount = result.Amount,
                    HSNCode = result.HSNCode,
                    SupplierStateCode = result.SupplierState,
                    RecipientStateCode = result.RecipientState,
                    GSTRate = result.GSTRate,
                    IsIntraState = result.IsIntraState,
                    CGST = result.CGST,
                    SGST = result.SGST,
                    IGST = result.IGST,
                    TotalGST = result.TotalGST,
                    TotalAmount = result.TotalAmount,
                    InvoiceDate = result.InvoiceDate,
                    GSTDescription = result.GSTDescription
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error calculating GST", error = ex.Message });
            }
        }

        #endregion

        #region Indian Banks

        [HttpGet("indian-banks")]
        public async Task<IActionResult> GetIndianBanks()
        {
            try
            {
                var banks = new List<object>();
                
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = @"
                        SELECT Id, BankCode, BankName, IFSCCode, MICRCode, BranchName, Address,
                               City, State, IsActive, CreatedDate, CreatedBy, LastUpdatedDate, LastUpdatedBy
                        FROM IndianBanks 
                        WHERE IsActive = 1
                        ORDER BY BankName";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                banks.Add(new
                                {
                                    Id = reader["Id"],
                                    BankCode = reader["BankCode"],
                                    BankName = reader["BankName"],
                                    IFSCCode = reader["IFSCCode"],
                                    MICRCode = reader["MICRCode"] != DBNull.Value ? reader["MICRCode"] : null,
                                    BranchName = reader["BranchName"] != DBNull.Value ? reader["BranchName"] : null,
                                    Address = reader["Address"] != DBNull.Value ? reader["Address"] : null,
                                    City = reader["City"] != DBNull.Value ? reader["City"] : null,
                                    State = reader["State"] != DBNull.Value ? reader["State"] : null,
                                    IsActive = reader["IsActive"],
                                    CreatedDate = reader["CreatedDate"],
                                    CreatedBy = reader["CreatedBy"],
                                    LastUpdatedDate = reader["LastUpdatedDate"] != DBNull.Value ? reader["LastUpdatedDate"] : null,
                                    LastUpdatedBy = reader["LastUpdatedBy"] != DBNull.Value ? reader["LastUpdatedBy"] : null
                                });
                            }
                        }
                    }
                }

                return Ok(banks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving Indian banks", error = ex.Message });
            }
        }

        #endregion

        #region Statutory Calculations

        [HttpPost("calculate-pf")]
        public async Task<IActionResult> CalculatePF([FromBody] dynamic request)
        {
            try
            {
                decimal basicSalary = request.BasicSalary;
                decimal da = request.DA ?? 0;
                decimal totalSalary = basicSalary + da;

                // Get current PF configuration
                decimal pfLimit = 15000; // Default
                decimal employeeRate = 12; // Default
                decimal employerRate = 12; // Default

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = @"
                        SELECT TOP 1 BasicSalaryLimit, EmployeeContributionRate, EmployerContributionRate
                        FROM PFConfiguration 
                        WHERE IsActive = 1 
                        ORDER BY EffectiveDate DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                pfLimit = (decimal)reader["BasicSalaryLimit"];
                                employeeRate = (decimal)reader["EmployeeContributionRate"];
                                employerRate = (decimal)reader["EmployerContributionRate"];
                            }
                        }
                    }
                }

                decimal taxableSalary = Math.Min(totalSalary, pfLimit);
                decimal employeePF = taxableSalary * (employeeRate / 100);
                decimal employerPF = taxableSalary * (employerRate / 100);

                return Ok(new
                {
                    BasicSalary = basicSalary,
                    DearnessAllowance = da,
                    TotalSalary = totalSalary,
                    TaxableSalary = taxableSalary,
                    EmployeeContribution = employeePF,
                    EmployerContribution = employerPF,
                    TotalPF = employeePF + employerPF
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error calculating PF", error = ex.Message });
            }
        }

        [HttpPost("calculate-esi")]
        public async Task<IActionResult> CalculateESI([FromBody] dynamic request)
        {
            try
            {
                decimal grossSalary = request.GrossSalary;

                // Get current ESI configuration
                decimal esiLimit = 21000; // Default
                decimal employeeRate = 0.75M; // Default
                decimal employerRate = 3.25M; // Default

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = @"
                        SELECT TOP 1 GrossSalaryLimit, EmployeeContributionRate, EmployerContributionRate
                        FROM ESIConfiguration 
                        WHERE IsActive = 1 
                        ORDER BY EffectiveDate DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                esiLimit = (decimal)reader["GrossSalaryLimit"];
                                employeeRate = (decimal)reader["EmployeeContributionRate"];
                                employerRate = (decimal)reader["EmployerContributionRate"];
                            }
                        }
                    }
                }

                decimal taxableSalary = Math.Min(grossSalary, esiLimit);
                decimal employeeESI = taxableSalary * (employeeRate / 100);
                decimal employerESI = taxableSalary * (employerRate / 100);

                return Ok(new
                {
                    GrossSalary = grossSalary,
                    TaxableSalary = taxableSalary,
                    EmployeeContribution = employeeESI,
                    EmployerContribution = employerESI,
                    TotalESI = employeeESI + employerESI
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error calculating ESI", error = ex.Message });
            }
        }

        #endregion
    }
}
