using Xunit;
using Moq;
using SIMS.Services;
using SIMS.Interfaces;
using SIMS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SE07201.Tests
{
    public class StudentServiceTests
    {
        private readonly Mock<IStudentRepository> _repoMock;
        private readonly StudentService _service;

        public StudentServiceTests()
        {
            _repoMock = new Mock<IStudentRepository>();
            _service = new StudentService(_repoMock.Object);
        }

        [Fact]
        public async Task AddStudent_ShouldReturnError_WhenStudentCodeExists()
        {
            _repoMock.Setup(r => r.StudentCodeExistsAsync("S01")).ReturnsAsync(true);

            var student = new Student { StudentCode = "S01", Email = "a@gmail.com" };

            var result = await _service.AddStudentAsync(student);

            Assert.False(result.success);
            Assert.Equal("Student code already exists!", result.message);
        }

        [Fact]
        public async Task AddStudent_ShouldReturnError_WhenEmailExists()
        {
            _repoMock.Setup(r => r.StudentCodeExistsAsync("S02")).ReturnsAsync(false);
            _repoMock.Setup(r => r.EmailExistsAsync("a@gmail.com")).ReturnsAsync(true);

            var student = new Student { StudentCode = "S02", Email = "a@gmail.com" };

            var result = await _service.AddStudentAsync(student);

            Assert.False(result.success);
            Assert.Equal("Email already exists!", result.message);
        }

        [Fact]
        public async Task AddStudent_ShouldReturnSuccess_WhenValid()
        {
            _repoMock.Setup(r => r.StudentCodeExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _repoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _repoMock.Setup(r => r.AddStudentAsync(It.IsAny<Student>())).ReturnsAsync(true);

            var student = new Student { StudentCode = "OK1", Email = "ok@gmail.com" };

            var result = await _service.AddStudentAsync(student);

            Assert.True(result.success);
            Assert.Equal("Student added successfully!", result.message);
        }

        [Fact]
        public async Task DeleteStudent_ShouldReturnTrue_WhenRepositoryReturnsTrue()
        {
            _repoMock.Setup(r => r.DeleteStudentAsync(1)).ReturnsAsync(true);

            var result = await _service.DeleteStudentAsync(1);

            Assert.True(result);
        }

        [Fact]
        public async Task GetStudentById_ShouldReturnStudent()
        {
            var student = new Student { StudentId = 1 };
            _repoMock.Setup(r => r.GetStudentByIdAsync(1)).ReturnsAsync(student);

            var result = await _service.GetStudentByIdAsync(1);

            Assert.Equal(1, result.StudentId);
        }
    }
}
