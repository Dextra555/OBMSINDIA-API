using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBMS.WebAPI.Models;
using OBMS.WebAPI.Models.Domain;
using OBMS.WebAPI.Repositories.Interface;
using OBMS.WebAPI.Models.DTO;

namespace OBMS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DesignationController : ControllerBase
    {
        private readonly OBMSDbContext _context;
        
        public DesignationController(OBMSDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDesignations(int? departmentId = null)
        {
            try
            {
                var query = _context.Designations.Where(d => d.IsActive);

                if (departmentId.HasValue && departmentId.Value > 0)
                {
                    query = query.Where(d => d.DepartmentId == departmentId.Value);
                }

                var designations = await query
                    .OrderBy(d => d.Desig_Name)
                    .Select(d => new DesignationDTO
                    {
                        DesignationId = d.DesignationId,
                        DesignationCode = d.Desig_Code,
                        DesignationName = d.Desig_Name,
                        Description = d.Description,
                        IsActive = d.IsActive,
                        CreatedDate = d.CreatedDate,
                        CreatedBy = d.CreatedBy,
                        UpdatedDate = d.UpdatedDate,
                        UpdatedBy = d.UpdatedBy,
                        DepartmentId = d.DepartmentId,
                        DepartmentName = d.DepartmentId.HasValue ? 
                            _context.Departments
                                .Where(dept => dept.DepartmentId == d.DepartmentId.Value)
                                .Select(dept => dept.Dept_Name)
                                .FirstOrDefault() : null
                    })
                    .ToListAsync();
                
                return Ok(designations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving designations", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDesignation(int id)
        {
            try
            {
                var designation = await _context.Designations.FindAsync(id);
                
                if (designation == null)
                    return NotFound();
                
                var departmentName = designation.DepartmentId.HasValue ? 
                    _context.Departments
                        .Where(dept => dept.DepartmentId == designation.DepartmentId.Value)
                        .Select(dept => dept.Dept_Name)
                        .FirstOrDefault() : null;
                
                var designationDto = new DesignationDTO
                {
                    DesignationId = designation.DesignationId,
                    DesignationCode = designation.Desig_Code,
                    DesignationName = designation.Desig_Name,
                    Description = designation.Description,
                    IsActive = designation.IsActive,
                    CreatedDate = designation.CreatedDate,
                    CreatedBy = designation.CreatedBy,
                    UpdatedDate = designation.UpdatedDate,
                    UpdatedBy = designation.UpdatedBy,
                    DepartmentId = designation.DepartmentId,
                    DepartmentName = departmentName
                };
                
                return Ok(designationDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving designation", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateDesignation([FromBody] DesignationDTO designationDto)
        {
            try
            {
                // Auto-generate Designation Code if not provided
                string designationCode = designationDto.DesignationCode;
                if (string.IsNullOrWhiteSpace(designationCode))
                {
                    // Generate code from designation name (first 3 letters + sequential number)
                    var namePrefix = new string(designationDto.DesignationName.Take(3).ToArray()).ToUpper();
                    var existingCount = await _context.Designations
                        .CountAsync(d => d.Desig_Code.StartsWith(namePrefix));
                    designationCode = $"{namePrefix}{(existingCount + 1):D3}";
                }

                var designation = new Designation
                {
                    Desig_Code = designationCode,
                    Desig_Name = designationDto.DesignationName,
                    Description = designationDto.Description,
                    DepartmentId = designationDto.DepartmentId,
                    CreatedDate = DateTime.Now,
                    CreatedBy = designationDto.CreatedBy,
                    IsActive = true
                };

                _context.Designations.Add(designation);
                await _context.SaveChangesAsync();

                var createdDto = new DesignationDTO
                {
                    DesignationId = designation.DesignationId,
                    DesignationCode = designation.Desig_Code,
                    DesignationName = designation.Desig_Name,
                    Description = designation.Description,
                    IsActive = designation.IsActive,
                    CreatedDate = designation.CreatedDate,
                    CreatedBy = designation.CreatedBy,
                    UpdatedDate = designation.UpdatedDate,
                    UpdatedBy = designation.UpdatedBy
                };

                return CreatedAtAction(nameof(GetDesignation), new { id = designation.DesignationId }, createdDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating designation", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDesignation(int id, [FromBody] DesignationDTO designationDto)
        {
            try
            {
                if (id != designationDto.DesignationId)
                    return BadRequest();
                
                var existingDesignation = await _context.Designations.FindAsync(id);
                if (existingDesignation == null)
                    return NotFound();
                
                existingDesignation.Desig_Code = designationDto.DesignationCode;
                existingDesignation.Desig_Name = designationDto.DesignationName;
                existingDesignation.Description = designationDto.Description;
                existingDesignation.UpdatedDate = DateTime.Now;
                existingDesignation.UpdatedBy = designationDto.UpdatedBy;
                existingDesignation.DepartmentId = designationDto.DepartmentId;
                await _context.SaveChangesAsync();
                
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating designation", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDesignation(int id)
        {
            try
            {
                var designation = await _context.Designations.FindAsync(id);
                if (designation == null)
                    return NotFound();

                // Check if any employee is mapped to this designation
                var hasEmployees = await _context.Employees
                    .AnyAsync(e => e.DesignationId == id);

                if (hasEmployees)
                {
                    return BadRequest(new {
                        message = "Cannot delete designation. Employees are mapped to this designation.",
                        error = "Designation has employees"
                    });
                }

                designation.IsActive = false;
                designation.UpdatedDate = DateTime.Now;
                designation.UpdatedBy = "System";

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting designation", error = ex.Message });
            }
        }
    }
}
