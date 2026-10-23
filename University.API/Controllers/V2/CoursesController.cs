using Asp.Versioning;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University.API.Models;
using University.Data;
using University.Data.Entities;

namespace University.API.Controllers.V2
{
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize] // Весь контроллер V2 требует JWT-токен по умолчанию
    public class CoursesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CoursesController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Пункт 6: Расширенный контракт V2 (включает имя преподавателя и кол-во студентов)
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetCoursesV2()
        {
            var courses = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .Select(c => new CourseV2Dto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Credits = c.Credits,
                    TeacherName = c.Teacher != null ? c.Teacher.Name : "Не назначен",
                    StudentsCount = c.Enrollments.Count
                })
                .ToListAsync();

            return Ok(courses);
        }

        // Пункт 5: Бизнес-логика проверки владельца ресурса (Преподаватель меняет оценку только СВОЕГО курса)
        [HttpPut("{courseId}/students/{studentId}/grade")]
        [Authorize(Roles = "Teacher")] // Доступ только для роли Teacher
        public async Task<IActionResult> UpdateGrade(int courseId, int studentId, [FromBody] int newGrade)
        {
            // 1. Получаем ID текущего авторизованного пользователя из JWT-токена
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.Users
                .Include(u => u.Teacher)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (currentUser?.TeacherId == null)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Вы не зарегистрированы как преподаватель университета!" });

            // 2. Находим курс в базе данных
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                return NotFound(new { message = "Курс не найден!" });

            // 3. ПРОВЕРКА ВЛАДЕЛЬЦА: Ведет ли данный преподаватель этот курс?
            if (course.TeacherId != currentUser.TeacherId)
            {
                // Если курс чужой — возвращаем строго 403 Forbidden согласно Пункту 5 задания
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Доступ запрещен! Вы не можете выставлять оценки на чужом курсе!" });
            }

            // 4. Если проверка прошла — обновляем оценку
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);

            if (enrollment == null)
                return NotFound(new { message = "Студент не записан на данный курс!" });

            enrollment.Grade = newGrade;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Оценка успешно обновлена преподавателем курса!" });
        }
    }
}