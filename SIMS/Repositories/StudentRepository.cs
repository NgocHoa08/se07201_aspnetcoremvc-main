using Microsoft.EntityFrameworkCore;
using SIMS.Interfaces;
using SIMS.Models;
using SIMS.SimsDbContext;

namespace SIMS.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly SimDbContext _context;

        public StudentRepository(SimDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
            return await _context.Students
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();
        }

        public async Task<Student> GetStudentByIdAsync(int id)
        {
            return await _context.Students.FindAsync(id);
        }

        public async Task<bool> AddStudentAsync(Student student)
        {
            try
            {
                student.CreatedDate = DateTime.Now;
                _context.Students.Add(student);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateStudentAsync(Student student)
        {
            try
            {
                student.UpdatedDate = DateTime.Now;
                _context.Students.Update(student);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
            try
            {
                var student = await _context.Students.FindAsync(id);
                if (student != null)
                {
                    _context.Students.Remove(student);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> StudentCodeExistsAsync(string studentCode)
        {
            return await _context.Students
                .AnyAsync(s => s.StudentCode == studentCode);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Students
                .AnyAsync(s => s.Email == email);
        }

        public async Task<IEnumerable<Student>> SearchStudentsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllStudentsAsync();
            }

            return await _context.Students
                .Where(s => s.StudentCode.Contains(searchTerm) ||
                           s.FullName.Contains(searchTerm) ||
                           s.Email.Contains(searchTerm) ||
                           (s.Major != null && s.Major.Contains(searchTerm)))
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();
        }
    }
}