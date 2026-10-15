using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIPublicSchool.Data;
using SIPublicSchool.Models;
namespace SIPublicSchool.Controllers;

[Authorize(Roles = "Teacher")]
public class TeacherController : Controller
{
    private readonly AppDbContext _db;
    public TeacherController(AppDbContext db) { _db = db; }
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

    public async Task<IActionResult> Dashboard()
    {
        var t = await _db.Teachers.Include(x => x.AppUser).FirstAsync(x => x.AppUserId == UserId);
        var vm = new TeacherDashVm
        {
            Teacher = t,
            TotalStudents = await _db.Students.CountAsync(),
            HomeworkPosted = await _db.Homeworks.CountAsync(h => h.TeacherName == t.AppUser.FullName),
            TodayPresent = await _db.Attendance.CountAsync(a => a.Date == DateTime.Today && a.Present),
            Notices = await _db.Notices.OrderByDescending(n => n.PostedOn).Take(3).ToListAsync(),
            Events = await _db.Events.Where(e => e.Date >= DateTime.Today).OrderBy(e => e.Date).Take(3).ToListAsync()
        };
        return View(vm);
    }

    public async Task<IActionResult> Students(string cls, string section)
    {
        var q = _db.Students.Include(s => s.AppUser).AsQueryable();
        if (!string.IsNullOrEmpty(cls)) q = q.Where(s => s.ClassName == cls);
        if (!string.IsNullOrEmpty(section)) q = q.Where(s => s.Section == section);
        ViewBag.Cls = cls; ViewBag.Section = section;
        return View(await q.OrderBy(s => s.ClassName).ThenBy(s => s.RollNo).ToListAsync());
    }

    // ---------- Attendance ----------
    public async Task<IActionResult> Attendance(string cls, string section, DateTime? date)
    {
        var vm = new AttendanceVm { ClassName = cls ?? "10", Section = section ?? "A", Date = date ?? DateTime.Today };
        vm.Students = await _db.Students.Include(s => s.AppUser)
            .Where(s => s.ClassName == vm.ClassName && s.Section == vm.Section).OrderBy(s => s.RollNo).ToListAsync();
        var ids = vm.Students.Select(s => s.Id).ToList();
        var present = await _db.Attendance.Where(a => a.Date == vm.Date.Date && ids.Contains(a.StudentId) && a.Present)
            .Select(a => a.StudentId).ToListAsync();
        vm.PresentIds = present.ToHashSet();
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveAttendance(string cls, string section, DateTime date, int[] presentIds)
    {
        presentIds ??= Array.Empty<int>();
        var students = await _db.Students.Where(s => s.ClassName == cls && s.Section == section).ToListAsync();
        var ids = students.Select(s => s.Id).ToList();
        var old = _db.Attendance.Where(a => a.Date == date.Date && ids.Contains(a.StudentId));
        _db.Attendance.RemoveRange(old);
        foreach (var s in students)
            _db.Attendance.Add(new AttendanceRecord { StudentId = s.Id, Date = date.Date, Present = presentIds.Contains(s.Id) });
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Attendance saved for " + date.ToString("dd MMM yyyy");
        return RedirectToAction(nameof(Attendance), new { cls, section, date });
    }

    // ---------- Marks ----------
    public async Task<IActionResult> Marks()
    {
        ViewBag.Students = await _db.Students.Include(s => s.AppUser).OrderBy(s => s.ClassName).ThenBy(s => s.RollNo).ToListAsync();
        var t = await _db.Teachers.FirstAsync(x => x.AppUserId == UserId);
        ViewBag.Subject = t.Subject;
        return View(await _db.Results.Include(r => r.Student).ThenInclude(s => s.AppUser).OrderByDescending(r => r.Id).Take(30).ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddResult(ExamResult r)
    {
        ModelState.Remove(nameof(ExamResult.Student));
        if (ModelState.IsValid && r.StudentId > 0 && r.Marks >= 0 && r.Marks <= r.MaxMarks)
        {
            _db.Results.Add(r); await _db.SaveChangesAsync(); TempData["Msg"] = "Marks saved.";
        }
        else TempData["Msg"] = "Invalid marks entered.";
        return RedirectToAction(nameof(Marks));
    }

    // ---------- Homework ----------
    public async Task<IActionResult> Homework()
    {
        var t = await _db.Teachers.FirstAsync(x => x.AppUserId == UserId);
        ViewBag.Subject = t.Subject;
        return View(await _db.Homeworks.OrderByDescending(h => h.Id).ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddHomework(Homework h)
    {
        if (ModelState.IsValid)
        {
            h.TeacherName = User.Identity.Name;
            _db.Homeworks.Add(h); await _db.SaveChangesAsync(); TempData["Msg"] = "Homework posted.";
        }
        return RedirectToAction(nameof(Homework));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteHomework(int id)
    {
        var h = await _db.Homeworks.FindAsync(id);
        if (h != null) { _db.Homeworks.Remove(h); await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Homework));
    }
}
