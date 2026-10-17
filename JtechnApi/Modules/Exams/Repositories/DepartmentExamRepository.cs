using System.Text.Json;
using JtechnApi.Exams.Dtos;
using JtechnApi.Exams.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static JtechnApi.Shares.RepositoryResult;

namespace JtechnApi.Exams.Repositories
{
    public class DepartmentExamRepository : IDepartmentExamRepository
    {
        private readonly DBJtecContext context;
        private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
        public DepartmentExamRepository(DBJtecContext context) => this.context = context;

        public async Task<object> Available(int employeeId)
        {
            if (!await context.Employee.AnyAsync(e => e.Id == employeeId && e.IsActive)) return Array.Empty<object>();
            return await (from exam in context.DepartmentExams.AsNoTracking()
                join department in context.Departments.AsNoTracking() on exam.DepartmentId equals department.Id
                where exam.IsActive && context.EmployeeDepartments.Any(a => a.EmployeeId == employeeId && a.DepartmentId == department.Id)
                orderby department.Name, exam.Name
                select new { exam.Id, exam.Name, exam.Code, exam.Minutes, DepartmentName = department.Name }).ToListAsync();
        }

        public bool CanTake(int employeeId, int examId) => context.Employee.Any(e => e.Id == employeeId && e.IsActive)
            && (from exam in context.DepartmentExams
                join department in context.Departments on exam.DepartmentId equals department.Id
                where exam.Id == examId && exam.IsActive && context.EmployeeDepartments.Any(a => a.EmployeeId == employeeId && a.DepartmentId == department.Id)
                select exam.Id).Any();

        public async Task<object> List(int? departmentId) => await (from exam in context.DepartmentExams.AsNoTracking()
            join department in context.Departments.AsNoTracking() on exam.DepartmentId equals department.Id
            where !departmentId.HasValue || exam.DepartmentId == departmentId.Value
            orderby department.Name, exam.Name
            select new { exam.Id, exam.DepartmentId, DepartmentName = department.Name, exam.Code, exam.Name,
                exam.Description, exam.Minutes, exam.IsActive, exam.CreatedAt }).ToListAsync();

        public async Task<RepositoryResult> GetById(int id)
        {
            var exam = await context.DepartmentExams.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
            return exam == null ? NotFound() : Ok(exam);
        }

        public async Task<RepositoryResult> Save(int? id, DepartmentExamInput input)
        {
            if (input == null) return BadRequest(new { message = "Dữ liệu không hợp lệ." });
            OnlineExamDefinition definition;
            try { definition = ExamDefinitionValidator.Parse(input.DefinitionJson); }
            catch (Exception ex) when (ex is JsonException || ex is ArgumentException) { return BadRequest(new { message = ex.Message }); }
            if (!await context.Departments.AnyAsync(d => d.Id == input.DepartmentId))
                return BadRequest(new { message = "Bộ phận không tồn tại hoặc đã xóa." });
            var code = input.Code.Trim().ToUpperInvariant();
            if (await context.DepartmentExams.IgnoreQueryFilters().AnyAsync(e => e.Code == code && (!id.HasValue || e.Id != id.Value)))
                return Conflict(new { message = "Mã bài thi đã tồn tại." });
            var entity = id.HasValue ? await context.DepartmentExams.FirstOrDefaultAsync(e => e.Id == id.Value) : new DepartmentExam { CreatedAt = DateTime.UtcNow };
            if (entity == null) return NotFound();
            entity.DepartmentId = input.DepartmentId; entity.Code = code; entity.Name = input.Name.Trim();
            entity.Description = input.Description?.Trim(); entity.Minutes = input.Minutes; entity.IsActive = input.IsActive;
            definition.Title = entity.Name; definition.Minutes = entity.Minutes;
            entity.DefinitionJson = JsonSerializer.Serialize(definition, Json);
            if (id.HasValue) entity.UpdatedAt = DateTime.UtcNow; else context.DepartmentExams.Add(entity);
            try { await context.SaveChangesAsync(); }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627))
            { return Conflict(new { message = "Mã bài thi đã tồn tại." }); }
            return id.HasValue ? Ok(new { entity.Id }) : CreatedAtAction(nameof(GetById), new { id = entity.Id }, new { entity.Id });
        }

        public async Task<RepositoryResult> Delete(int id)
        {
            var entity = await context.DepartmentExams.FirstOrDefaultAsync(e => e.Id == id);
            if (entity == null) return NotFound();
            entity.DeletedAt = DateTime.UtcNow; entity.UpdatedAt = entity.DeletedAt;
            await context.SaveChangesAsync(); return NoContent();
        }

        public OnlineExamDefinition LoadDefinition(int id)
        {
            var exam = (from e in context.DepartmentExams.AsNoTracking()
                join d in context.Departments.AsNoTracking() on e.DepartmentId equals d.Id
                where e.Id == id && e.IsActive select e).FirstOrDefault();
            if (exam == null) throw new InvalidOperationException("Bài thi không tồn tại, đã xóa hoặc chưa mở thi.");
            var definition = ExamDefinitionValidator.Parse(exam.DefinitionJson);
            definition.Id = exam.Id; definition.Title = exam.Name; definition.Minutes = exam.Minutes;
            return definition;
        }
    }
}
