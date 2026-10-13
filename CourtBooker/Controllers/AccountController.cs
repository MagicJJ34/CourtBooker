using System.Security.Claims;
using CourtBooker.Data;
using CourtBooker.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourtBooker.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<User> _hasher = new();

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _context.Users.FirstOrDefaultAsync(
            u => u.Email == model.Email);

        if (user == null || !VerifyPassword(user, model.Password))
        {
            ModelState.AddModelError("", "Nieprawidłowy email lub hasło.");
            return View(model);
        }

        if (!user.PasswordHash.StartsWith("AQAAAA"))
        {
            user.PasswordHash = _hasher.HashPassword(user, model.Password);
            await _context.SaveChangesAsync();
        }

        await SignIn(user);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (await _context.Users.AnyAsync(u => u.Email == model.Email))
        {
            ModelState.AddModelError("Email", "Ten adres e-mail jest już zajęty.");
            return View(model);
        }

        var user = new User
        {
            Name = model.Name,
            Email = model.Email,
            Role = "User"
        };

        user.PasswordHash = _hasher.HashPassword(user, model.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    public async Task<IActionResult> Profile()
    {
        int id = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        return View(new ProfileViewModel
        {
            Name = user.Name,
            Email = user.Email
        });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        int id = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        if (await _context.Users.AnyAsync(
            u => u.Email == model.Email && u.Id != id))
        {
            ModelState.AddModelError("Email", "Ten adres e-mail jest już zajęty.");
        }

        bool changePassword = !string.IsNullOrWhiteSpace(model.NewPassword);

        if (changePassword)
        {
            if (!VerifyPassword(user, model.CurrentPassword))
                ModelState.AddModelError("CurrentPassword", "Nieprawidłowe obecne hasło.");

            if (model.NewPassword != model.ConfirmPassword)
                ModelState.AddModelError("ConfirmPassword", "Hasła nie są zgodne.");
        }

        if (!ModelState.IsValid)
            return View(model);

        user.Name = model.Name;
        user.Email = model.Email;

        if (changePassword)
            user.PasswordHash = _hasher.HashPassword(user, model.NewPassword!);

        await _context.SaveChangesAsync();
        await SignIn(user);

        TempData["Success"] = "Dane konta zostały zapisane.";
        return RedirectToAction(nameof(Profile));
    }

    private bool VerifyPassword(User user, string password)
    {
        if (user.PasswordHash.StartsWith("AQAAAA"))
        {
            return _hasher.VerifyHashedPassword(
                user, user.PasswordHash, password) != PasswordVerificationResult.Failed;
        }

        return user.PasswordHash == password;
    }

    private async Task SignIn(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}
