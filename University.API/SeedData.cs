using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace University.API
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<University.Data.AppDbContext>(serviceProvider);
            var roleManager = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService(serviceProvider);
            var userManager = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<UserManager<University.Data.Entities.ApplicationUser>>(serviceProvider);

            string[] roleNames = { "Admin", "Teacher", "Student" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            if (!context.Teachers.Any())
            {
                var teacher = new University.Data.Entities.Teacher { Name = "Профессор Иванов", Department = "Кафедра ИТ" };
                var student = new University.Data.Entities.Student { Name = "Алексей Смирнов" };

                context.Teachers.Add(teacher);
                context.Students.Add(student);
                await context.SaveChangesAsync();

                var course = new University.Data.Entities.Course { Title = "Разработка на ASP.NET Core", Credits = 4, TeacherId = teacher.Id };
                context.Courses.Add(course);
                await context.SaveChangesAsync();

                if (await userManager.FindByNameAsync("teacher@univ.com") == null)
                {
                    var teacherUser = new University.Data.Entities.ApplicationUser { UserName = "teacher@univ.com", Email = "teacher@univ.com", TeacherId = teacher.Id };
                    await userManager.CreateAsync(teacherUser, "Teacher123!");
                    await userManager.AddToRoleAsync(teacherUser, "Teacher");
                }

                if (await userManager.FindByNameAsync("student@univ.com") == null)
                {
                    var studentUser = new University.Data.Entities.ApplicationUser { UserName = "student@univ.com", Email = "student@univ.com", StudentId = student.Id };
                    await userManager.CreateAsync(studentUser, "Student123!");
                    await userManager.AddToRoleAsync(studentUser, "Student");
                }
            }
        }
    }
}