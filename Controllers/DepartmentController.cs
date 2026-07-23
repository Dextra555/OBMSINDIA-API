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
    public class DepartmentController : ControllerBase
    {
        private readonly OBMSDbContext _context;
        
        public DepartmentController(OBMSDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            try
            {
                var departments = await _context.Departments
                    .Where(d => d.IsActive)
                    .OrderBy(d => d.Dept_Name)
                    .Select(d => new DepartmentDTO
                    {
                        DepartmentId = d.DepartmentId,
                        DepartmentCode = d.Dept_Code,
                        DepartmentName = d.Dept_Name,
                        Description = d.Description,
                        IsActive = d.IsActive,
                        CreatedDate = d.CreatedDate,
                        CreatedBy = d.CreatedBy,
                        UpdatedDate = d.UpdatedDate,
                        UpdatedBy = d.UpdatedBy
                    })
                    .ToListAsync();
                
                return Ok(departments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving departments", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDepartment(int id)
        {
            try
            {
                var department = await _context.Departments.FindAsync(id);
                
                if (department == null)
                    return NotFound();
                
                var departmentDto = new DepartmentDTO
                {
                    DepartmentId = department.DepartmentId,
                    DepartmentCode = department.Dept_Code,
                    DepartmentName = department.Dept_Name,
                    Description = department.Description,
                    IsActive = department.IsActive,
                    CreatedDate = department.CreatedDate,
                    CreatedBy = department.CreatedBy,
                    UpdatedDate = department.UpdatedDate,
                    UpdatedBy = department.UpdatedBy
                };
                
                return Ok(departmentDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving department", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateDepartment([FromBody] DepartmentDTO departmentDto)
        {
            try
            {
                // Auto-generate Department Code if not provided
                string departmentCode = departmentDto.DepartmentCode;
                if (string.IsNullOrWhiteSpace(departmentCode))
                {
                    // Generate code from department name (first 3 letters + sequential number)
                    var namePrefix = new string(departmentDto.DepartmentName.Take(3).ToArray()).ToUpper();
                    var existingCount = await _context.Departments
                        .CountAsync(d => d.Dept_Code.StartsWith(namePrefix));
                    departmentCode = $"{namePrefix}{(existingCount + 1):D3}";
                }

                var department = new Department
                {
                    Dept_Code = departmentCode,
                    Dept_Name = departmentDto.DepartmentName,
                    Description = departmentDto.Description,
                    CreatedDate = DateTime.Now,
                    CreatedBy = "System",
                    IsActive = true
                };
                
                _context.Departments.Add(department);
                await _context.SaveChangesAsync();
                
                var createdDto = new DepartmentDTO
                {
                    DepartmentId = department.DepartmentId,
                    DepartmentCode = department.Dept_Code,
                    DepartmentName = department.Dept_Name,
                    Description = department.Description,
                    IsActive = department.IsActive,
                    CreatedDate = department.CreatedDate,
                    CreatedBy = department.CreatedBy,
                    UpdatedDate = department.UpdatedDate,
                    UpdatedBy = department.UpdatedBy
                };
                
                return CreatedAtAction(nameof(GetDepartment), new { id = department.DepartmentId }, createdDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating department", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, [FromBody] DepartmentDTO departmentDto)
        {
            try
            {
                if (id != departmentDto.DepartmentId)
                    return BadRequest();
                
                var existingDepartment = await _context.Departments.FindAsync(id);
                if (existingDepartment == null)
                    return NotFound();
                
                existingDepartment.Dept_Code = departmentDto.DepartmentCode;
                existingDepartment.Dept_Name = departmentDto.DepartmentName;
                existingDepartment.Description = departmentDto.Description;
                existingDepartment.UpdatedDate = DateTime.Now;
                existingDepartment.UpdatedBy = departmentDto.UpdatedBy;
                
                await _context.SaveChangesAsync();
                
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating department", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            try
            {
                var department = await _context.Departments.FindAsync(id);
                if (department == null)
                    return NotFound();
                
                // Check if any employee is mapped to this department
                var hasEmployees = await _context.Employees
                    .AnyAsync(e => e.DepartmentId == id);
                
                if (hasEmployees)
                {
                    return BadRequest(new { 
                        message = "Cannot delete department. Employees are mapped to this department.",
                        error = "Department has employees"
                    });
                }
                
                // Check if any designation is mapped to this department
                var hasDesignations = await _context.Designations
                    .AnyAsync(d => d.DepartmentId == id && d.IsActive);
                
                if (hasDesignations)
                {
                    return BadRequest(new { 
                        message = "Cannot delete department. Designations are mapped to this department.",
                        error = "Department has active designations"
                    });
                }
                
                department.IsActive = false;
                department.UpdatedDate = DateTime.Now;
                department.UpdatedBy = "System";
                
                await _context.SaveChangesAsync();
                
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting department", error = ex.Message });
            }
        }
    }
}
