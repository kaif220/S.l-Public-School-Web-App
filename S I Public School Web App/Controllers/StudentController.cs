using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIPublicSchool.Data;
using SIPublicSchool.Models;
namespace SIPublicSchool.Controllers;

[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly AppDbContext _db;
    public StudentController(AppDbContext db) { _db = db; }
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
    private Task<Student> Me() => _db.Students.Include(s => s.AppUser).FirstAsync(s => s.AppUserId == UserId);

    public async Task<IActionResult> Dashboard()
    {
        var me = await Me();
        var att = await _db.Attendance.Where(a => a.StudentId == me.Id).ToListAsync();
        var res = await _db.Results.Where(r => r.StudentId == me.Id).ToListAsync();
        var vm = new StudentDashVm
        {
            Student = me,
            AttendancePercent = att.Count == 0 ? 0 : Math.Round(att.Count(a => a.Present) * 100.0 / att.Count, 1),
            AvgPercent = res.Count == 0 ? 0 : Math.Round(res.Sum(r => r.Marks) * 100.0 / res.Sum(r => r.MaxMarks), 1),
            PendingFees = (await _db.Fees.Where(f => f.StudentId == me.Id && !f.IsPaid).Select(f => f.Amount).ToListAsync()).Sum(),
            HomeworkCount = await _db.Homeworks.CountAsync(h => h.ClassName == me.ClassName && h.DueDate >= DateTime.Today),
            Notices = await _db.Notices.OrderByDescending(n => n.PostedOn).Take(3).ToListAsync()
        };
        foreach (var g in res.GroupBy(r => r.Subject))
        {
            vm.SubjectLabels.Add(g.Key);
            vm.SubjectPercents.Add(Math.Round(g.Sum(r => r.Marks) * 100.0 / g.Sum(r => r.MaxMarks), 1));
        }
        return View(vm);
    }

    public async Task<IActionResult> Attendance()
    {
        var me = await Me();
        var list = await _db.Attendance.Where(a => a.StudentId == me.Id).OrderByDescending(a => a.Date).ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Results()
    {
        var me = await Me();
        return View(await _db.Results.Where(r => r.StudentId == me.Id).OrderBy(r => r.Exam).ThenBy(r => r.Subject).ToListAsync());
    }

    public async Task<IActionResult> Homework()
    {
        var me = await Me();
        return View(await _db.Homeworks.Where(h => h.ClassName == me.ClassName).OrderByDescending(h => h.DueDate).ToListAsync());
    }

    public async Task<IActionResult> Fees()
    {
        var me = await Me();
        return View(await _db.Fees.Where(f => f.StudentId == me.Id).OrderBy(f => f.DueDate).ToListAsync());
    }

    public async Task<IActionResult> Profile() => View(await Me());
}
