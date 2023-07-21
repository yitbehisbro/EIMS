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
    public class MembershipDuesPaymentsController : Controller
    {
        private readonly EdrImsProjectContext _context;

        public MembershipDuesPaymentsController(EdrImsProjectContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult GetMembershipDuesPayments ()
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
                var returnData = (from manudata in _context.MembershipDuesPayments.Where(x=>x.IsDeleted==false) 
                                  .Include(x => x.Member)
                                  .Include(x => x.MembershipDuesYears)
                                  .Include(x => x.MembershipDuesMonths)
                                  .Select(x => new
                                  {
                                      x.Id,
                                      x.Amount,
                                      x.IsPaid,
                                      Member = x.Member.FirstName + " " + x.Member.MiddleName + " " + x.Member.LastName,
                                      MembershipDuesYears = x.MembershipDuesYears.Name,
                                      MembershipDuesMonths = x.MembershipDuesMonths.Name,
                                      x.IsActive
                                  })
                                  select manudata);
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    returnData = returnData.OrderBy(sortColumn + " " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    returnData = returnData.Where(m => m.Member.Contains(searchValue));
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

        // GET: MembershipDuesPayments

       
        public IActionResult Index()
        {
            return View();
        }
        //public async Task<IActionResult> Index()
        //{
            //var edrImsProjectContext = _context.MembershipDuesPayments.Include(m => m.Member).Include(m => m.MembershipDuesMonths).Include(m => m.MembershipDuesYears);
            //return View(await edrImsProjectContext.ToListAsync());
        //}


        // GET: MembershipDuesPayments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.MembershipDuesPayments == null)
            {
                return NotFound();
            }

            var membershipDuesPayment = await _context.MembershipDuesPayments
                .Include(m => m.Member)
                .Include(m => m.MembershipDuesMonths)
                .Include(m => m.MembershipDuesYears)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (membershipDuesPayment == null)
            {
                return NotFound();
            }

            return View(membershipDuesPayment);
        }

        // GET: MembershipDuesPayments/Create
        public IActionResult Create()
        {
            ViewData["MemberId"] = new SelectList(_context.Members
                .Where(x => x.IsDeleted == false)
                .Select(x => new
            {
                x.Id,
                Member = x.FirstName + " " + x.MiddleName + " " + x.LastName
            }), "Id", "Member");
            ViewData["MembershipDuesMonthsId"] = new SelectList(_context.MembershipDuesMonths, "Id", "Name");
            ViewData["MembershipDuesYearsId"] = new SelectList(_context.MembershipDuesYears, "Id", "Name");
            return View();
        }

        // POST: MembershipDuesPayments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MembershipDuesYearsId,MembershipDuesMonthsId,MemberId,Amount,IsPaid,IsActive")] MembershipDuesPayment membershipDuesPayment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(membershipDuesPayment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "membershipDuesPayment saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving membershipDuesPayment. Please review your input.";
            ViewData["MemberId"] = new SelectList(_context.Members.Where(x => x.IsDeleted == false)
                .Select(x => new
                {
                    x.Id,
                    Member = x.FirstName + " " + x.MiddleName + " " + x.LastName
                }), "Id", "Member", membershipDuesPayment.MemberId);
            ViewData["MembershipDuesMonthsId"] = new SelectList(_context.MembershipDuesMonths, "Id", "Name", membershipDuesPayment.MembershipDuesMonthsId);
            ViewData["MembershipDuesYearsId"] = new SelectList(_context.MembershipDuesYears, "Id", "Name", membershipDuesPayment.MembershipDuesYearsId);
            return View(membershipDuesPayment);
        }

        // GET: MembershipDuesPayments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.MembershipDuesPayments == null)
            {
                return NotFound();
            }

            var membershipDuesPayment = await _context.MembershipDuesPayments.FindAsync(id);
            if (membershipDuesPayment == null)
            {
                return NotFound();
            }
            ViewData["MemberId"] = new SelectList(_context.Members.Where(x => x.IsDeleted == false)
                .Select(x => new
                {
                    x.Id,
                    Member = x.FirstName + " " + x.MiddleName + " " + x.LastName
                }), "Id", "Member", membershipDuesPayment.MemberId);
            ViewData["MembershipDuesMonthsId"] = new SelectList(_context.MembershipDuesMonths, "Id", "Name", membershipDuesPayment.MembershipDuesMonthsId);
            ViewData["MembershipDuesYearsId"] = new SelectList(_context.MembershipDuesYears, "Id", "Name", membershipDuesPayment.MembershipDuesYearsId);
            return View(membershipDuesPayment);
        }

        // POST: MembershipDuesPayments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MembershipDuesYearsId,MembershipDuesMonthsId,MemberId,Amount,IsPaid,IsActive")] MembershipDuesPayment membershipDuesPayment)
        {
            if (id != membershipDuesPayment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(membershipDuesPayment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MembershipDuesPaymentExists(membershipDuesPayment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "membershipDuesPayment saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving membershipDuesPayment. Please review your input.";
            ViewData["MemberId"] = new SelectList(_context.Members.Where(x => x.IsDeleted == false).Select(x => new { x.Id, Member = x.FirstName + " " + x.MiddleName + " " + x.LastName }), "Id", "Member", membershipDuesPayment.MemberId);
            ViewData["MembershipDuesMonthsId"] = new SelectList(_context.MembershipDuesMonths, "Id", "Name", membershipDuesPayment.MembershipDuesMonthsId);
            ViewData["MembershipDuesYearsId"] = new SelectList(_context.MembershipDuesYears, "Id", "Name", membershipDuesPayment.MembershipDuesYearsId);
            return View(membershipDuesPayment);
        }

        // GET: MembershipDuesPayments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.MembershipDuesPayments == null)
            {
                return NotFound();
            }

            var membershipDuesPayment = await _context.MembershipDuesPayments
                .Include(m => m.Member)
                .Include(m => m.MembershipDuesMonths)
                .Include(m => m.MembershipDuesYears)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (membershipDuesPayment == null)
            {
                return NotFound();
            }

            return View(membershipDuesPayment);
        }

        // POST: MembershipDuesPayments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.MembershipDuesPayments == null)
            {
                return Problem("Entity set 'EdrImsProjectContext.MembershipDuesPayments'  is null.");
            }
            var membershipDuesPayment = await _context.MembershipDuesPayments.FindAsync(id);
            if (membershipDuesPayment != null)
            {
                 membershipDuesPayment.IsDeleted = true;
                _context.Update(membershipDuesPayment);
                //_context.MembershipDuesPayments.Remove(membershipDuesPayment);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MembershipDuesPaymentExists(int id)
        {
          return _context.MembershipDuesPayments.Any(e => e.Id == id);
        }
    }
}
