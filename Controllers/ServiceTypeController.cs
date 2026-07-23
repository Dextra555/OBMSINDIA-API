using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using OBMS.WebAPI.Models.Domain;
using OBMS.WebAPI.Models.DTO;
using OBMS.WebAPI.Models;

namespace OBMS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceTypeController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly OBMSDbContext _context;

        public ServiceTypeController(IConfiguration configuration, OBMSDbContext context)
        {
            _connectionString = configuration.GetConnectionString("obms") 
                ?? "Server=localhost;Database=obmsuat;Trusted_Connection=true;";
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllServiceTypes()
        {
            try
            {
                var serviceTypes = new List<ServiceTypeDto>();
                
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = @"
                        SELECT 
                            st.Id,
                            st.ServiceName,
                            st.ServiceCode,
                            st.Description,
                            st.HSNCode,
                            st.PricingModel,
                            st.IsActive,
                            st.CreatedDate,
                            st.CreatedBy,
                            st.LastUpdatedDate,
                            st.LastUpdatedBy
                        FROM ServiceType st
                        WHERE st.IsActive = 1
                        ORDER BY st.ServiceName";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                serviceTypes.Add(new ServiceTypeDto
                                {
                                    Id = reader["Id"] != DBNull.Value ? (int)reader["Id"] : 0,
                                    ServiceName = reader["ServiceName"]?.ToString() ?? string.Empty,
                                    ServiceCode = reader["ServiceCode"]?.ToString() ?? string.Empty,
                                    Description = reader["Description"]?.ToString(),
                                    HSNCode = reader["HSNCode"]?.ToString() ?? string.Empty,
                                    PricingModel = reader["PricingModel"]?.ToString() ?? "Standard",
                                    IsActive = reader["IsActive"] != DBNull.Value && (bool)reader["IsActive"],
                                    CreatedDate = reader["CreatedDate"] != DBNull.Value ? (DateTime)reader["CreatedDate"] : DateTime.MinValue,
                                    CreatedBy = reader["CreatedBy"]?.ToString() ?? string.Empty,
                                    LastUpdatedDate = reader["LastUpdatedDate"] != DBNull.Value ? (DateTime?)reader["LastUpdatedDate"] : null,
                                    LastUpdatedBy = reader["LastUpdatedBy"]?.ToString()
                                });
                            }
                        }
                    }
                }

                return Ok(serviceTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving service types", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceTypeById(int id)
        {
            try
            {
                ServiceTypeDto serviceType = null;
                
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = @"
                        SELECT 
                            st.Id,
                            st.ServiceName,
                            st.ServiceCode,
                            st.Description,
                            st.HSNCode,
                            st.PricingModel,
                            st.IsActive,
                            st.CreatedDate,
                            st.CreatedBy,
                            st.LastUpdatedDate,
                            st.LastUpdatedBy
                        FROM ServiceType st
                        WHERE st.Id = @Id";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                serviceType = new ServiceTypeDto
                                {
                                    Id = reader["Id"] != DBNull.Value ? (int)reader["Id"] : 0,
                                    ServiceName = reader["ServiceName"]?.ToString() ?? string.Empty,
                                    ServiceCode = reader["ServiceCode"]?.ToString() ?? string.Empty,
                                    Description = reader["Description"]?.ToString(),
                                    HSNCode = reader["HSNCode"]?.ToString() ?? string.Empty,
                                    PricingModel = reader["PricingModel"]?.ToString() ?? "Standard",
                                    IsActive = reader["IsActive"] != DBNull.Value && (bool)reader["IsActive"],
                                    CreatedDate = reader["CreatedDate"] != DBNull.Value ? (DateTime)reader["CreatedDate"] : DateTime.MinValue,
                                    CreatedBy = reader["CreatedBy"]?.ToString() ?? string.Empty,
                                    LastUpdatedDate = reader["LastUpdatedDate"] != DBNull.Value ? (DateTime?)reader["LastUpdatedDate"] : null,
                                    LastUpdatedBy = reader["LastUpdatedBy"]?.ToString()
                                };
                            }
                        }
                    }
                }

                if (serviceType == null)
                {
                    return NotFound(new { message = "Service type not found" });
                }

                return Ok(serviceType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving service type", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateServiceType([FromBody] ServiceTypeCreateDto serviceTypeDto)
        {
            try
            {
                // Generate default ServiceCode if not provided
                string serviceCode = serviceTypeDto.ServiceCode;
                if (string.IsNullOrEmpty(serviceCode))
                {
                    // Generate code from ServiceName (first 3 letters + 3-digit number)
                    var words = serviceTypeDto.ServiceName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var prefix = string.Concat(words.Take(3).Select(w => w.Length > 0 ? char.ToUpper(w[0]) : ' '));
                    if (string.IsNullOrEmpty(prefix))
                    {
                        prefix = "ST";
                    }

                    // Get next sequential number using Entity Framework
                    var count = await _context.ServiceTypes.CountAsync(st => st.ServiceCode != null && st.ServiceCode.StartsWith(prefix));
                    int nextNumber = count + 1;
                    serviceCode = $"{prefix}{nextNumber:D3}";
                }

                // Create new ServiceType entity
                var serviceType = new ServiceType
                {
                    ServiceName = serviceTypeDto.ServiceName,
                    ServiceCode = serviceCode,
                    Description = serviceTypeDto.Description,
                    HSNCode = serviceTypeDto.HSNCode,
                    PricingModel = serviceTypeDto.PricingModel ?? "Standard",
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    CreatedBy = "SYSTEM"
                };

                // Add to database using Entity Framework
                _context.ServiceTypes.Add(serviceType);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Service type created successfully", serviceCode = serviceCode });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating service type", error = ex.Message, details = ex.InnerException?.Message });
            }
        }

        private async Task<string> GenerateServiceCode(string serviceName)
        {
            // Generate ServiceCode from ServiceName (e.g., "Security Services" -> "SS001")
            // Take first letters of each word and add a sequential number
            var words = serviceName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var codePrefix = string.Concat(words.Take(3).Select(w => w.Length > 0 ? char.ToUpper(w[0]) : ' '));
            
            if (string.IsNullOrEmpty(codePrefix))
            {
                codePrefix = "ST"; // Default prefix if ServiceName is empty
            }

            // Get the next sequential number for this prefix
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = @"
                    SELECT COUNT(*) + 1 
                    FROM ServiceType 
                    WHERE ServiceCode LIKE @Prefix + '%'";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Prefix", codePrefix);
                    var result = await command.ExecuteScalarAsync();
                    int nextNumber = result != null ? Convert.ToInt32(result) : 1;
                    
                    return $"{codePrefix}{nextNumber:D3}"; // Format as SS001, SS002, etc.
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateServiceType(int id, [FromBody] ServiceTypeUpdateDto serviceTypeDto)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = @"
                        UPDATE ServiceType 
                        SET ServiceName = @ServiceName,
                            Description = @Description,
                            HSNCode = @HSNCode,
                            PricingModel = @PricingModel,
                            IsActive = @IsActive,
                            LastUpdatedDate = GETDATE(),
                            LastUpdatedBy = @LastUpdatedBy
                        WHERE Id = @Id";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.Parameters.AddWithValue("@ServiceName", serviceTypeDto.ServiceName);
                        command.Parameters.AddWithValue("@Description", (object?)serviceTypeDto.Description ?? DBNull.Value);
                        command.Parameters.AddWithValue("@HSNCode", serviceTypeDto.HSNCode);
                        command.Parameters.AddWithValue("@PricingModel", (object?)serviceTypeDto.PricingModel ?? DBNull.Value);
                        command.Parameters.AddWithValue("@IsActive", serviceTypeDto.IsActive);
                        command.Parameters.AddWithValue("@LastUpdatedBy", "SYSTEM");

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        
                        if (rowsAffected == 0)
                        {
                            return NotFound(new { message = "Service type not found" });
                        }
                    }
                }

                return Ok(new { message = "Service type updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating service type", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceType(int id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = "UPDATE ServiceType SET IsActive = 0, LastUpdatedDate = GETDATE(), LastUpdatedBy = @LastUpdatedBy WHERE Id = @Id";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.Parameters.AddWithValue("@LastUpdatedBy", "SYSTEM");

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        
                        if (rowsAffected == 0)
                        {
                            return NotFound(new { message = "Service type not found" });
                        }
                    }
                }

                return Ok(new { message = "Service type deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting service type", error = ex.Message });
            }
        }
    }
}
