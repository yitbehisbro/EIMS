using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using EdrIMS.Models;

namespace EdrIMS.Controllers
{
    public class MembershipDuesMonthsController : Controller
    {
        private readonly EdrImsProjectContext _context;

        public MembershipDuesMonthsController(EdrImsProjectContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult GetMembershipDuesMonths ()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;
                var returnData = (from manudata in _context.MembershipDuesMonths.Where(x=>x.IsDeleted==false)
                                  .Include(x => x.MembershipDuesYear)
                                  .Select(x => new
                                  {
                                      x.Id,
                                      x.Name,
                                      MembershipDuesYear = x.MembershipDuesYear.Name,
                                      x.IsActive
                                  })
                                  select manudata);
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    returnData = returnData.OrderBy(sortColumn + " " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    returnData = returnData.Where(m => m.Name.Contains(searchValue));
                }
                recordsTotal = returnData.Count();
                var data = returnData.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw, recordsFiltered = recordsTotal, recordsTotal, data };
                return Ok(jsonData);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // GET: MembershipDuesMonths

       
        public IActionResult Index()
        {
            return View();
        }
        //public async Task<IActionResult> Index()
        //{
            //var edrImsProjectContext = _context.MembershipDuesMonths.Include(m => m.MembershipDuesYear);
            //return View(await edrImsProjectContext.ToListAsync());
        //}


        // GET: MembershipDuesMonths/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.MembershipDuesMonths == null)
            {
                return NotFound();
            }

            var membershipDuesMonth = await _context.MembershipDuesMonths
                .Include(m => m.MembershipDuesYear)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (membershipDuesMonth == null)
            {
                return NotFound();
            }

            return View(membershipDuesMonth);
        }

        // GET: MembershipDuesMonths/Create
        public IActionResult Create()
        {
            ViewData["MembershipDuesYearId"] = new SelectList(_context.MembershipDuesYears, "Id", "Name");
            return View();
        }

        // POST: MembershipDuesMonths/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,MembershipDuesYearId,IsActive")] MembershipDuesMonth membershipDuesMonth)
        {
            if (ModelState.IsValid)
            {
                _context.Add(membershipDuesMonth);
                await _context.SaveChangesAsync();
                TempData["Success"] = "membershipDuesMonth saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving membershipDuesMonth. Please review your input.";
            ViewData["MembershipDuesYearId"] = new SelectList(_context.MembershipDuesYears, "Id", "Name", membershipDuesMonth.MembershipDuesYearId);
            return View(membershipDuesMonth);
        }

        // GET: MembershipDuesMonths/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.MembershipDuesMonths == null)
            {
                return NotFound();
            }

            var membershipDuesMonth = await _context.MembershipDuesMonths.FindAsync(id);
            if (membershipDuesMonth == null)
            {
                return NotFound();
            }
            ViewData["MembershipDuesYearId"] = new SelectList(_context.MembershipDuesYears, "Id", "Name", membershipDuesMonth.MembershipDuesYearId);
            return View(membershipDuesMonth);
        }

        // POST: MembershipDuesMonths/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,MembershipDuesYearId,IsActive")] MembershipDuesMonth membershipDuesMonth)
        {
            if (id != membershipDuesMonth.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(membershipDuesMonth);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MembershipDuesMonthExists(membershipDuesMonth.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "membershipDuesMonth saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving membershipDuesMonth. Please review your input.";
            ViewData["MembershipDuesYearId"] = new SelectList(_context.MembershipDuesYears, "Id", "Name", membershipDuesMonth.MembershipDuesYearId);
            return View(membershipDuesMonth);
        }

        // GET: MembershipDuesMonths/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.MembershipDuesMonths == null)
            {
                return NotFound();
            }

            var membershipDuesMonth = await _context.MembershipDuesMonths
                .Include(m => m.MembershipDuesYear)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (membershipDuesMonth == null)
            {
                return NotFound();
            }

            return View(membershipDuesMonth);
        }

        // POST: MembershipDuesMonths/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.MembershipDuesMonths == null)
            {
                return Problem("Entity set 'EdrImsProjectContext.MembershipDuesMonths'  is null.");
            }
            var membershipDuesMonth = await _context.MembershipDuesMonths.FindAsync(id);
            if (membershipDuesMonth != null)
            {
                 membershipDuesMonth.IsDeleted = true;
                _context.Update(membershipDuesMonth);
                //_context.MembershipDuesMonths.Remove(membershipDuesMonth);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MembershipDuesMonthExists(int id)
        {
          return _context.MembershipDuesMonths.Any(e => e.Id == id);
        }
    }
}
