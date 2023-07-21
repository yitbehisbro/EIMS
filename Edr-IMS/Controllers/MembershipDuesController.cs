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
    public class MembershipDuesController : Controller
    {
        private readonly EdrImsProjectContext _context;

        public MembershipDuesController(EdrImsProjectContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult GetMembershipDues ()
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
                var returnData = (from manudata in _context.MembershipDues.Where(x=>x.IsDeleted==false)
                                  .Include(x => x.Currency)
                                  .Include(x => x.MembershipDuesType)
                                  .Include(x => x.MembershipDuesMonth)
                                  .Select(x => new
                                  {
                                      x.Id,
                                      Amount = x.Amount + " " + x.Currency.Name,
                                      MembershipDuesType = x.MembershipDuesType.Name,
                                      MembershipDuesMonth = x.MembershipDuesMonth.Name,
                                      x.IsActive
                                  })
                                  select manudata);
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    returnData = returnData.OrderBy(sortColumn + " " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    returnData = returnData.Where(m => m.MembershipDuesMonth.Contains(searchValue));
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

        // GET: MembershipDues

       
        public IActionResult Index()
        {
            return View();
        }
        //public async Task<IActionResult> Index()
        //{
            //var edrImsProjectContext = _context.MembershipDues.Include(m => m.Currency).Include(m => m.MembershipDuesMonth).Include(m => m.MembershipDuesType);
            //return View(await edrImsProjectContext.ToListAsync());
        //}


        // GET: MembershipDues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.MembershipDues == null)
            {
                return NotFound();
            }

            var membershipDue = await _context.MembershipDues
                .Include(m => m.Currency)
                .Include(m => m.MembershipDuesMonth)
                .Include(m => m.MembershipDuesType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (membershipDue == null)
            {
                return NotFound();
            }

            return View(membershipDue);
        }

        // GET: MembershipDues/Create
        public IActionResult Create()
        {
            ViewData["CurrencyId"] = new SelectList(_context.Currencies, "Id", "Name");
            ViewData["MembershipDuesMonthId"] = new SelectList(_context.MembershipDuesMonths, "Id", "Name");
            ViewData["MembershipDuesTypeId"] = new SelectList(_context.MembershipDuesTypes, "Id", "Name");
            return View();
        }

        // POST: MembershipDues/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Amount,CurrencyId,MembershipDuesMonthId,MembershipDuesTypeId,IsActive")] MembershipDue membershipDue)
        {
            if (ModelState.IsValid)
            {
                _context.Add(membershipDue);
                await _context.SaveChangesAsync();
                TempData["Success"] = "membershipDue saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving membershipDue. Please review your input.";
            ViewData["CurrencyId"] = new SelectList(_context.Currencies, "Id", "Name", membershipDue.CurrencyId);
            ViewData["MembershipDuesMonthId"] = new SelectList(_context.MembershipDuesMonths, "Id", "Name", membershipDue.MembershipDuesMonthId);
            ViewData["MembershipDuesTypeId"] = new SelectList(_context.MembershipDuesTypes, "Id", "Name", membershipDue.MembershipDuesTypeId);
            return View(membershipDue);
        }

        // GET: MembershipDues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.MembershipDues == null)
            {
                return NotFound();
            }

            var membershipDue = await _context.MembershipDues.FindAsync(id);
            if (membershipDue == null)
            {
                return NotFound();
            }
            ViewData["CurrencyId"] = new SelectList(_context.Currencies, "Id", "Name", membershipDue.CurrencyId);
            ViewData["MembershipDuesMonthId"] = new SelectList(_context.MembershipDuesMonths, "Id", "Name", membershipDue.MembershipDuesMonthId);
            ViewData["MembershipDuesTypeId"] = new SelectList(_context.MembershipDuesTypes, "Id", "Name", membershipDue.MembershipDuesTypeId);
            return View(membershipDue);
        }

        // POST: MembershipDues/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Amount,CurrencyId,MembershipDuesMonthId,MembershipDuesTypeId,IsActive")] MembershipDue membershipDue)
        {
            if (id != membershipDue.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(membershipDue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MembershipDueExists(membershipDue.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "membershipDue saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving membershipDue. Please review your input.";
            ViewData["CurrencyId"] = new SelectList(_context.Currencies, "Id", "Name", membershipDue.CurrencyId);
            ViewData["MembershipDuesMonthId"] = new SelectList(_context.MembershipDuesMonths, "Id", "Name", membershipDue.MembershipDuesMonthId);
            ViewData["MembershipDuesTypeId"] = new SelectList(_context.MembershipDuesTypes, "Id", "Name", membershipDue.MembershipDuesTypeId);
            return View(membershipDue);
        }

        // GET: MembershipDues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.MembershipDues == null)
            {
                return NotFound();
            }

            var membershipDue = await _context.MembershipDues
                .Include(m => m.Currency)
                .Include(m => m.MembershipDuesMonth)
                .Include(m => m.MembershipDuesType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (membershipDue == null)
            {
                return NotFound();
            }

            return View(membershipDue);
        }

        // POST: MembershipDues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.MembershipDues == null)
            {
                return Problem("Entity set 'EdrImsProjectContext.MembershipDues'  is null.");
            }
            var membershipDue = await _context.MembershipDues.FindAsync(id);
            if (membershipDue != null)
            {
                 membershipDue.IsDeleted = true;
                _context.Update(membershipDue);
                //_context.MembershipDues.Remove(membershipDue);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MembershipDueExists(int id)
        {
          return _context.MembershipDues.Any(e => e.Id == id);
        }
    }
}
