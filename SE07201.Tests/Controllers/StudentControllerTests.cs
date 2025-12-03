using Xunit;
using Moq;
using SIMS.Controllers;
using SIMS.Services;
using SIMS.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SE07201.Tests
{
    public class StudentControllerTests
    {
        private StudentController CreateControllerWithUser(string role, string email)
        {
            var serviceMock = new Mock<StudentService>(null);
            var controller = new StudentController(serviceMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.Email, email)
            }, "TestAuthentication"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = user
                }
            };

            return controller;
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenStudentNull()
        {
            var serviceMock = new Mock<StudentService>(null);
            serviceMock.Setup(s => s.GetStudentByIdAsync(1)).ReturnsAsync((Student)null);

            var controller = new StudentController(serviceMock.Object);

            var result = await controller.Details(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_HidesGPA_ForUserViewingOtherStudent()
        {
            var student = new Student
            {
                StudentId = 5,
                Email = "other@gmail.com",
                GPA = 3.5m
            };

            var serviceMock = new Mock<StudentService>(null);
            serviceMock.Setup(s => s.GetStudentByIdAsync(5)).ReturnsAsync(student);

            var controller = CreateControllerWithUser("User", "me@gmail.com");

            var result = await controller.Details(5);

            Assert.IsType<ViewResult>(result);
            Assert.True((bool)controller.ViewBag.HideGPA);
        }

        [Fact]
        public async Task Details_ShowsGPA_ForUserViewingOwnProfile()
        {
            var student = new Student
            {
                StudentId = 5,
                Email = "me@gmail.com",
                GPA = 3.5m
            };

            var serviceMock = new Mock<StudentService>(null);
            serviceMock.Setup(s => s.GetStudentByIdAsync(5)).ReturnsAsync(student);

            var controller = CreateControllerWithUser("User", "me@gmail.com");

            var result = await controller.Details(5);

            Assert.IsType<ViewResult>(result);
            Assert.False((bool)controller.ViewBag.HideGPA);
        }

        [Fact]
        public async Task Details_ShowsGPA_ForAdmin()
        {
            var student = new Student
            {
                StudentId = 5,
                Email = "any@gmail.com",
                GPA = 4.0m
            };

            var serviceMock = new Mock<StudentService>(null);
            serviceMock.Setup(s => s.GetStudentByIdAsync(5)).ReturnsAsync(student);

            var controller = CreateControllerWithUser("Administrator", "admin@gmail.com");

            var result = await controller.Details(5);

            Assert.IsType<ViewResult>(result);
            Assert.False((bool)controller.ViewBag.HideGPA);
        }
    }
}
