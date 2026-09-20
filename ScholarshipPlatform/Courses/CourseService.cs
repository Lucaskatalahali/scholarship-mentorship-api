using Microsoft.EntityFrameworkCore;
using ScholarshipPlatform.Courses.Dtos;
using ScholarshipPlatform.Data;

namespace ScholarshipPlatform.Courses;

public class CourseService
{
    private readonly AppDbContext _db;

    public CourseService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CourseResponseDto?> CreateCourse(CreateCourseDto dto)
    {
        var trimmedName = dto.Name.Trim();
        var exists = await _db.Courses.AnyAsync(c => c.Name == trimmedName);

        if(exists) return null;

        var course = new Course
        {
            Name = trimmedName
        };

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        return new CourseResponseDto(course.Id, course.Name);
    }

    public async Task<bool?> UpdateCourse(int id, UpdateCourseDto dto)
    {
        var trimmedName = dto.Name.Trim();
        var course = await _db.Courses.FindAsync(id);

        if(course is null) return null; //Not Found

        if(await _db.Courses.AnyAsync(c => c.Name == trimmedName))
            return false; //Conflict

        course.Name = trimmedName;

        await _db.SaveChangesAsync();

        return true; //Success
    }

    public async Task<List<CourseResponseDto>> GetCourses()
    {
        return await _db.Courses
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Name
            )).ToListAsync();
    }
}