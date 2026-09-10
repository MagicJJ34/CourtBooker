using CourtBooker.Data;
using CourtBooker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourtBooker.Controllers;

public class CourtsController : Controller
{
    private readonly AppDbContext _context;

    public CourtsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var courts = await _context.Courts.ToListAsync();
        return View(courts);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Surface,PricePerHour")] Court court)
    {
        ModelState.Remove("Reservations");

        if (ModelState.IsValid)
        {
            _context.Add(court);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(court);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var court = await _context.Courts.FindAsync(id);
        if (court == null) return NotFound();

        return View(court);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Surface,PricePerHour")] Court court)
    {
        if (id != court.Id) return NotFound();

        ModelState.Remove("Reservations");

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(court);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourtExists(court.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(court);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var court = await _context.Courts.FirstOrDefaultAsync(m => m.Id == id);
        if (court == null) return NotFound();

        return View(court);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var court = await _context.Courts.FindAsync(id);
        if (court != null)
        {
            _context.Courts.Remove(court);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private bool CourtExists(int id)
    {
        return _context.Courts.Any(e => e.Id == id);
    }
}