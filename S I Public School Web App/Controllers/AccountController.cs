using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIPublicSchool.Data;
using SIPublicSchool.Models;
namespace SIPublicSchool.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    public AccountController(AppDbContext db) { _db = db; }

    public IActionResult Login() => View(new LoginVm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == vm.Email);
        var hasher = new PasswordHasher<AppUser>();
        if (user == null || hasher.VerifyHashedPassword(user, user.PasswordHash, vm.Password) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(vm);
        }
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role)
        };
        var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(new ClaimsPrincipal(id));
        return user.Role switch
        {
            "Admin" => RedirectToAction("Dashboard", "Admin"),
            "Teacher" => RedirectToAction("Dashboard", "Teacher"),
            _ => RedirectToAction("Dashboard", "Student")
        };
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => View();
}
