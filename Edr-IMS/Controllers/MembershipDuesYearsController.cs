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
    public class MembershipDuesYearsController : Controller
    {
        private readonly EdrImsProjectContext _context;

        public MembershipDuesYearsController(EdrImsProjectContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult GetMembershipDuesYears ()
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
                var returnData = (from manudata in _context.MembershipDuesYears.Where(x=>x.IsDeleted==false) select manudata);
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

        // GET: MembershipDuesYears

       
        public IActionResult Index()
        {
            return View();
        }
        //public async Task<IActionResult> Index()
        //{
              //return View(await _context.MembershipDuesYears.Where(x=>!x.IsDeleted).ToListAsync());
        //}


        // GET: MembershipDuesYears/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.MembershipDuesYears == null)
            {
                return NotFound();
            }

            var membershipDuesYear = await _context.MembershipDuesYears
                .FirstOrDefaultAsync(m => m.Id == id);
            if (membershipDuesYear == null)
            {
                return NotFound();
            }

            return View(membershipDuesYear);
        }

        // GET: MembershipDuesYears/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MembershipDuesYears/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,IsActive")] MembershipDuesYear membershipDuesYear)
        {
            if (ModelState.IsValid)
            {
                _context.Add(membershipDuesYear);
                await _context.SaveChangesAsync();
                TempData["Success"] = "membershipDuesYear saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving membershipDuesYear. Please review your input.";
            return View(membershipDuesYear);
        }

        // GET: MembershipDuesYears/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.MembershipDuesYears == null)
            {
                return NotFound();
            }

            var membershipDuesYear = await _context.MembershipDuesYears.FindAsync(id);
            if (membershipDuesYear == null)
            {
                return NotFound();
            }
            return View(membershipDuesYear);
        }

        // POST: MembershipDuesYears/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,IsActive")] MembershipDuesYear membershipDuesYear)
        {
            if (id != membershipDuesYear.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(membershipDuesYear);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MembershipDuesYearExists(membershipDuesYear.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "membershipDuesYear saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving membershipDuesYear. Please review your input.";
            return View(membershipDuesYear);
        }

        // GET: MembershipDuesYears/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.MembershipDuesYears == null)
            {
                return NotFound();
            }

            var membershipDuesYear = await _context.MembershipDuesYears
                .FirstOrDefaultAsync(m => m.Id == id);
            if (membershipDuesYear == null)
            {
                return NotFound();
            }

            return View(membershipDuesYear);
        }

        // POST: MembershipDuesYears/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.MembershipDuesYears == null)
            {
                return Problem("Entity set 'EdrImsProjectContext.MembershipDuesYears'  is null.");
            }
            var membershipDuesYear = await _context.MembershipDuesYears.FindAsync(id);
            if (membershipDuesYear != null)
            {
                 membershipDuesYear.IsDeleted = true;
                _context.Update(membershipDuesYear);
                //_context.MembershipDuesYears.Remove(membershipDuesYear);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MembershipDuesYearExists(int id)
        {
          return _context.MembershipDuesYears.Any(e => e.Id == id);
        }
    }
}
