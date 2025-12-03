using Xunit;
using SIMS.Models;
using System;

namespace SE07201.Tests
{
    public class StudentModelTests
    {
        [Fact]
        public void Student_ShouldSetPropertiesCorrectly()
        {
            var now = DateTime.Now;

            var student = new Student
            {
                StudentId = 1,
                StudentCode = "ST01",
                FullName = "Nguyen Van A",
                Email = "a@gmail.com",
                Phone = "0123456789",
                GPA = 3.2m,
                Address = "Hanoi"
            };

            Assert.Equal(1, student.StudentId);
            Assert.Equal("ST01", student.StudentCode);
            Assert.Equal("Nguyen Van A", student.FullName);
            Assert.Equal("a@gmail.com", student.Email);
            Assert.Equal("0123456789", student.Phone);
            Assert.Equal(3.2m, student.GPA);
            Assert.Equal("Hanoi", student.Address);
        }
    }
}
