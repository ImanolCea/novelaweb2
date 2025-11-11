using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using novelaweb2.Models;

namespace novelaweb2.Controllers
{
    public class UserFollowsController : Controller
    {
        private readonly WebNovelasDbContext _context;

        public UserFollowsController(WebNovelasDbContext context)
        {
            _context = context;
        }

        // GET: UserFollows
        public async Task<IActionResult> Index()
        {
            var webNovelasDbContext = _context.UserFollows.Include(u => u.IdSeguidoNavigation).Include(u => u.IdSeguidorNavigation);
            return View(await webNovelasDbContext.ToListAsync());
        }

        // GET: UserFollows/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userFollow = await _context.UserFollows
                .Include(u => u.IdSeguidoNavigation)
                .Include(u => u.IdSeguidorNavigation)
                .FirstOrDefaultAsync(m => m.IdSeguidor == id);
            if (userFollow == null)
            {
                return NotFound();
            }

            return View(userFollow);
        }

        // GET: UserFollows/Create
        public IActionResult Create()
        {
            ViewData["IdSeguido"] = new SelectList(_context.Usuarios, "Id", "Id");
            ViewData["IdSeguidor"] = new SelectList(_context.Usuarios, "Id", "Id");
            return View();
        }

        // POST: UserFollows/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdSeguidor,IdSeguido,FechaSeguimiento")] UserFollow userFollow)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userFollow);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdSeguido"] = new SelectList(_context.Usuarios, "Id", "Id", userFollow.IdSeguido);
            ViewData["IdSeguidor"] = new SelectList(_context.Usuarios, "Id", "Id", userFollow.IdSeguidor);
            return View(userFollow);
        }

        // GET: UserFollows/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userFollow = await _context.UserFollows.FindAsync(id);
            if (userFollow == null)
            {
                return NotFound();
            }
            ViewData["IdSeguido"] = new SelectList(_context.Usuarios, "Id", "Id", userFollow.IdSeguido);
            ViewData["IdSeguidor"] = new SelectList(_context.Usuarios, "Id", "Id", userFollow.IdSeguidor);
            return View(userFollow);
        }

        // POST: UserFollows/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdSeguidor,IdSeguido,FechaSeguimiento")] UserFollow userFollow)
        {
            if (id != userFollow.IdSeguidor)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userFollow);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserFollowExists(userFollow.IdSeguidor))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdSeguido"] = new SelectList(_context.Usuarios, "Id", "Id", userFollow.IdSeguido);
            ViewData["IdSeguidor"] = new SelectList(_context.Usuarios, "Id", "Id", userFollow.IdSeguidor);
            return View(userFollow);
        }

        // GET: UserFollows/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userFollow = await _context.UserFollows
                .Include(u => u.IdSeguidoNavigation)
                .Include(u => u.IdSeguidorNavigation)
                .FirstOrDefaultAsync(m => m.IdSeguidor == id);
            if (userFollow == null)
            {
                return NotFound();
            }

            return View(userFollow);
        }

        // POST: UserFollows/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userFollow = await _context.UserFollows.FindAsync(id);
            if (userFollow != null)
            {
                _context.UserFollows.Remove(userFollow);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserFollowExists(int id)
        {
            return _context.UserFollows.Any(e => e.IdSeguidor == id);
        }
    }
}
