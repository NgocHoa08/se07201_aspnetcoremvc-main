using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS.Models;
using SIMS.Services;
using System.Security.Claims;

namespace SIMS.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly StudentService _studentService;

        public StudentController(StudentService studentService)
        {
            _studentService = studentService;
        }

        // Xem danh sách sinh viên
        [Authorize(Roles = "Administrator, User, Manager")]
        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm)
        {
            IEnumerable<Student> students;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                students = await _studentService.SearchStudentsAsync(searchTerm);
                ViewData["SearchTerm"] = searchTerm;
            }
            else
            {
                students = await _studentService.GetAllStudentsAsync();
            }

            // Nếu là User role và không phải Admin/Manager, chỉ hiển thị sinh viên của chính mình
            if (userRole == "User" && !User.IsInRole("Administrator") && !User.IsInRole("Manager"))
            {
                var currentEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                students = students.Where(s => s.Email == currentEmail);
            }

            // Ẩn GPA cho User role (Student) khi xem danh sách
            ViewBag.HideGPAInList = (userRole == "User");
            ViewBag.CurrentUserRole = userRole;

            return View(students);
        }

        // Form thêm sinh viên mới
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        // Xử lý thêm sinh viên
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Student student)
        {
            if (ModelState.IsValid)
            {
                var (success, message) = await _studentService.AddStudentAsync(student);

                if (success)
                {
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction("Index");
                }
                else
                {
                    if (message.Contains("Student code"))
                    {
                        ModelState.AddModelError("StudentCode", message);
                    }
                    else if (message.Contains("Email"))
                    {
                        ModelState.AddModelError("Email", message);
                    }
                    else
                    {
                        ModelState.AddModelError("", message);
                    }
                }
            }
            return View(student);
        }

        // Form sửa sinh viên
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // Xử lý sửa sinh viên
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Student student)
        {
            if (ModelState.IsValid)
            {
                var (success, message) = await _studentService.UpdateStudentAsync(student);

                if (success)
                {
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", message);
                }
            }
            return View(student);
        }

        // Xác nhận xóa sinh viên
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // Xử lý xóa sinh viên
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _studentService.DeleteStudentAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Student deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Error deleting student!";
            }
            return RedirectToAction("Index");
        }

        // Chi tiết sinh viên
        [Authorize(Roles = "Administrator, User, Manager")]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            var currentEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Kiểm tra xem sinh viên này có phải của user hiện tại không
            bool isOwnProfile = (student.Email == currentEmail);

            // Logic hiển thị GPA:
            // - Administrator, Manager: Xem tất cả GPA
            // - User (Student): Chỉ xem GPA của chính mình
            if (userRole == "User" && !isOwnProfile)
            {
                // User xem sinh viên khác → Ẩn GPA
                ViewBag.HideGPA = true;
                ViewBag.IsOtherStudent = true;
                TempData["InfoMessage"] = "You don't have permission to view other students' GPA.";
            }
            else if (userRole == "User" && isOwnProfile)
            {
                // User xem chính mình → Hiện GPA
                ViewBag.HideGPA = false;
                ViewBag.IsOwnProfile = true;
            }
            else
            {
                // Admin, Manager → Hiện tất cả
                ViewBag.HideGPA = false;
            }

            ViewBag.CurrentUserRole = userRole;
            return View(student);
        }

        // Xem profile của chính mình (dành cho User/Student)
        [Authorize(Roles = "User")]
        [HttpGet]
        public async Task<IActionResult> MyProfile()
        {
            var currentEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(currentEmail))
            {
                TempData["ErrorMessage"] = "Email not found in your account!";
                return RedirectToAction("Index", "Dashboard");
            }

            // Tìm student theo email
            var students = await _studentService.GetAllStudentsAsync();
            var myStudent = students.FirstOrDefault(s => s.Email.Equals(currentEmail, StringComparison.OrdinalIgnoreCase));

            if (myStudent == null)
            {
                TempData["ErrorMessage"] = "Student profile not found! Please contact administrator.";
                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.IsOwnProfile = true;
            ViewBag.HideGPA = false; // Hiển thị GPA của chính mình
            return View("Details", myStudent);
        }
    }
}