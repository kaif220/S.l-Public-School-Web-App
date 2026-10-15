using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIPublicSchool.Data;
using SIPublicSchool.Models;
namespace SIPublicSchool.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _db;
    public AdminController(AppDbContext db) { _db = db; }

    public async Task<IActionResult> Dashboard()
    {
        var vm = new AdminDashVm
        {
            Students = await _db.Students.CountAsync(),
            Teachers = await _db.Teachers.CountAsync(),
            Enquiries = await _db.Enquiries.CountAsync(),
            Notices = await _db.Notices.CountAsync(),
            FeesCollected = (await _db.Fees.Where(f => f.IsPaid).Select(f => f.Amount).ToListAsync()).Sum(),
            FeesPending = (await _db.Fees.Where(f => !f.IsPaid).Select(f => f.Amount).ToListAsync()).Sum()
        };
        var groups = await _db.Students.GroupBy(s => s.ClassName).Select(g => new { g.Key, C = g.Count() }).ToListAsync();
        foreach (var c in SchoolData.Classes)
        {
            var g = groups.FirstOrDefault(x => x.Key == c);
            if (g != null) { vm.ClassLabels.Add("Class " + c); vm.ClassCounts.Add(g.C); }
        }
        ViewBag.Recent = await _db.Enquiries.OrderByDescending(e => e.CreatedOn).Take(5).ToListAsync();
        return View(vm);
    }

    // ---------- Students ----------
    public async Task<IActionResult> Students(string q)
    {
        var query = _db.Students.Include(s => s.AppUser).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(s => s.AppUser.FullName.Contains(q) || s.RollNo.Contains(q) || s.ClassName == q);
        ViewBag.Q = q;
        return View(await query.OrderBy(s => s.ClassName).ThenBy(s => s.RollNo).ToListAsync());
    }

    public IActionResult CreateStudent() => View(new StudentForm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateStudent(StudentForm f)
    {
        if (!ModelState.IsValid) return View(f);
        if (await _db.Users.AnyAsync(u => u.Email == f.Email))
        {
            ModelState.AddModelError("Email", "This email is already registered.");
            return View(f);
        }
        var user = new AppUser { FullName = f.FullName, Email = f.Email, Role = "Student" };
        user.PasswordHash = new PasswordHasher<AppUser>().HashPassword(user, f.Password);
        _db.Students.Add(new Student { AppUser = user, RollNo = f.RollNo, ClassName = f.ClassName, Section = f.Section, ParentName = f.ParentName, Phone = f.Phone, Address = f.Address });
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Student added successfully.";
        return RedirectToAction(nameof(Students));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteStudent(int id)
    {
        var s = await _db.Students.Include(x => x.AppUser).FirstOrDefaultAsync(x => x.Id == id);
        if (s != null) { _db.Users.Remove(s.AppUser); await _db.SaveChangesAsync(); }
        TempData["Msg"] = "Student deleted.";
        return RedirectToAction(nameof(Students));
    }

    // ---------- Teachers ----------
    public async Task<IActionResult> Teachers() =>
        View(await _db.Teachers.Include(t => t.AppUser).OrderBy(t => t.AppUser.FullName).ToListAsync());

    public IActionResult CreateTeacher() => View(new TeacherForm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTeacher(TeacherForm f)
    {
        if (!ModelState.IsValid) return View(f);
        if (await _db.Users.AnyAsync(u => u.Email == f.Email))
        {
            ModelState.AddModelError("Email", "This email is already registered.");
            return View(f);
        }
        var user = new AppUser { FullName = f.FullName, Email = f.Email, Role = "Teacher" };
        user.PasswordHash = new PasswordHasher<AppUser>().HashPassword(user, f.Password);
        _db.Teachers.Add(new Teacher { AppUser = user, Subject = f.Subject, Qualification = f.Qualification, Phone = f.Phone });
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Teacher added successfully.";
        return RedirectToAction(nameof(Teachers));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTeacher(int id)
    {
        var t = await _db.Teachers.Include(x => x.AppUser).FirstOrDefaultAsync(x => x.Id == id);
        if (t != null) { _db.Users.Remove(t.AppUser); await _db.SaveChangesAsync(); }
        TempData["Msg"] = "Teacher deleted.";
        return RedirectToAction(nameof(Teachers));
    }

    // ---------- Notices ----------
    public async Task<IActionResult> Notices() => View(await _db.Notices.OrderByDescending(n => n.PostedOn).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddNotice(Notice n)
    {
        if (ModelState.IsValid) { n.PostedOn = DateTime.Now; _db.Notices.Add(n); await _db.SaveChangesAsync(); TempData["Msg"] = "Notice published."; }
        return RedirectToAction(nameof(Notices));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteNotice(int id)
    {
        var n = await _db.Notices.FindAsync(id);
        if (n != null) { _db.Notices.Remove(n); await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Notices));
    }

    // ---------- Events ----------
    public async Task<IActionResult> Events() => View(await _db.Events.OrderBy(e => e.Date).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddEvent(SchoolEvent e)
    {
        if (ModelState.IsValid) { _db.Events.Add(e); await _db.SaveChangesAsync(); TempData["Msg"] = "Event added."; }
        return RedirectToAction(nameof(Events));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var e = await _db.Events.FindAsync(id);
        if (e != null) { _db.Events.Remove(e); await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Events));
    }

    // ---------- Enquiries ----------
    public async Task<IActionResult> Enquiries() => View(await _db.Enquiries.OrderByDescending(e => e.CreatedOn).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEnquiry(int id)
    {
        var e = await _db.Enquiries.FindAsync(id);
        if (e != null) { _db.Enquiries.Remove(e); await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Enquiries));
    }

    // ---------- Fees ----------
    public async Task<IActionResult> Fees()
    {
        ViewBag.Students = await _db.Students.Include(s => s.AppUser).OrderBy(s => s.ClassName).ToListAsync();
        return View(await _db.Fees.Include(f => f.Student).ThenInclude(s => s.AppUser).OrderBy(f => f.IsPaid).ThenBy(f => f.DueDate).ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddFee(FeeRecord f)
    {
        ModelState.Remove(nameof(FeeRecord.Student));
        if (ModelState.IsValid && f.StudentId > 0) { _db.Fees.Add(f); await _db.SaveChangesAsync(); TempData["Msg"] = "Fee added."; }
        return RedirectToAction(nameof(Fees));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkPaid(int id)
    {
        var f = await _db.Fees.FindAsync(id);
        if (f != null) { f.IsPaid = true; f.PaidOn = DateTime.Now; await _db.SaveChangesAsync(); TempData["Msg"] = "Marked as paid."; }
        return RedirectToAction(nameof(Fees));
    }
}
