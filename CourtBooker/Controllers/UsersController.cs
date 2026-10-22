using System.Security.Claims;
using CourtBooker.Data;
using CourtBooker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourtBooker.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<User> _hasher = new();

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _context.Users
            .OrderBy(u => u.Name)
            .ToListAsync();

        return View(users);
    }

    public IActionResult Create()
    {
        return View(new CreateUserViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            model.Name = model.Name.Trim();
            model.Email = model.Email.Trim();

            bool exists = await _context.Users.AnyAsync(u =>
                u.Email.ToLower() == model.Email.ToLower());

            if (exists)
                ModelState.AddModelError("Email", "Ten e-mail jest już zajęty.");
        }

        if (!ModelState.IsValid)
            return View(model);

        var user = new User
        {
            Name = model.Name,
            Email = model.Email,
            Role = "User",
            IsActive = true
        };

        user.PasswordHash = _hasher.HashPassword(user, model.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Użytkownik został dodany.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        return View(new EditUserViewModel
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditUserViewModel model)
    {
        if (id != model.Id)
            return NotFound();

        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        if (ModelState.IsValid)
        {
            model.Name = model.Name.Trim();
            model.Email = model.Email.Trim();

            if (model.Role != "User" && model.Role != "Admin")
                ModelState.AddModelError("Role", "Nieprawidłowa rola.");

            bool exists = await _context.Users.AnyAsync(u =>
                u.Id != id &&
                u.Email.ToLower() == model.Email.ToLower());

            if (exists)
                ModelState.AddModelError("Email", "Ten e-mail jest już zajęty.");

            string? currentId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (user.Id.ToString() == currentId && model.Role != "Admin")
            {
                ModelState.AddModelError(
                    "Role", "Nie możesz odebrać sobie uprawnień administratora.");
            }
        }

        if (!ModelState.IsValid)
            return View(model);

        user.Name = model.Name;
        user.Email = model.Email;
        user.Role = model.Role;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Dane użytkownika zostały zapisane.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        string? currentId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (user.Id.ToString() == currentId)
        {
            TempData["Error"] = "Nie możesz dezaktywować własnego konta.";
            return RedirectToAction(nameof(Index));
        }

        user.IsActive = !user.IsActive;

        await _context.SaveChangesAsync();

        TempData["Success"] = user.IsActive
            ? "Konto użytkownika zostało aktywowane."
            : "Konto użytkownika zostało dezaktywowane. Historia rezerwacji została zachowana.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        if (user.Id.ToString() ==
            User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            TempData["Error"] = "Nie możesz usunąć własnego konta.";
            return RedirectToAction(nameof(Index));
        }

        return View(user);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        if (user.Id.ToString() ==
            User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            TempData["Error"] = "Nie możesz usunąć własnego konta.";
            return RedirectToAction(nameof(Index));
        }

        bool hasReservations = await _context.Reservations
            .AnyAsync(r => r.UserId == id);

        if (hasReservations)
        {
            TempData["Error"] =
                "Ten użytkownik ma historię rezerwacji. Zamiast usuwać konto, możesz je dezaktywować.";
            return RedirectToAction(nameof(Index));
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Użytkownik został usunięty.";
        return RedirectToAction(nameof(Index));
    }
}
