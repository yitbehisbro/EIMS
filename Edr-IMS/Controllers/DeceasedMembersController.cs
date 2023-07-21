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
    public class DeceasedMembersController : Controller
    {
        private readonly EdrImsProjectContext _context;

        public DeceasedMembersController(EdrImsProjectContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult GetDeceasedMembers ()
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
                var returnData = (from manudata in _context.DeceasedMembers.Where(x=>x.IsDeleted==false)
                                  .Include(x => x.Member)
                                  .Select(x => new
                                  {
                                      x.Id,
                                      Buried = x.Buried.Value.ToString("dd MMMM yyyy hh:mm tt"),
                                      Died = x.Died.ToString("dd MMMM yyyy hh:mm tt"),
                                      x.CauseOfDeath,
                                      x.LegalDocuments,
                                      x.RestingPlace,
                                      Member = x.Member.FirstName + " " + x.Member.MiddleName + " " + x.Member.LastName,
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

        // GET: DeceasedMembers

       
        public IActionResult Index()
        {
            return View();
        }
        //public async Task<IActionResult> Index()
        //{
            //var edrImsProjectContext = _context.DeceasedMembers.Include(d => d.Member);
            //return View(await edrImsProjectContext.ToListAsync());
        //}


        // GET: DeceasedMembers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.DeceasedMembers == null)
            {
                return NotFound();
            }

            var deceasedMember = await _context.DeceasedMembers
                .Include(d => d.Member)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (deceasedMember == null)
            {
                return NotFound();
            }

            return View(deceasedMember);
        }

        // GET: DeceasedMembers/Create
        public IActionResult Create()
        {
            ViewData["MemberId"] = new SelectList(_context.Members.Where(x => x.IsDeleted == false)
                .Select(x => new
                {
                    x.Id,
                    Member = x.FirstName + " " + x.MiddleName + " " + x.LastName
                }), "Id", "Member");
            return View();
        }

        // POST: DeceasedMembers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MemberId,Died,Buried,CauseOfDeath,RestingPlace,LegalDocuments,IsActive")] DeceasedMember deceasedMember)
        {
            if (ModelState.IsValid)
            {
                _context.Add(deceasedMember);
                await _context.SaveChangesAsync();
                TempData["Success"] = "deceasedMember saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving deceasedMember. Please review your input.";
            ViewData["MemberId"] = new SelectList(_context.Members.Where(x => x.IsDeleted == false)
                .Select(x => new
                {
                    x.Id,
                    Member = x.FirstName + " " + x.MiddleName + " " + x.LastName
                }), "Id", "Member", deceasedMember.MemberId);
            return View(deceasedMember);
        }

        // GET: DeceasedMembers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.DeceasedMembers == null)
            {
                return NotFound();
            }

            var deceasedMember = await _context.DeceasedMembers.FindAsync(id);
            if (deceasedMember == null)
            {
                return NotFound();
            }
            ViewData["MemberId"] = new SelectList(_context.Members.Where(x => x.IsDeleted == false)
                .Select(x => new
                {
                    x.Id,
                    Member = x.FirstName + " " + x.MiddleName + " " + x.LastName
                }), "Id", "Member", deceasedMember.MemberId);
            return View(deceasedMember);
        }

        // POST: DeceasedMembers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MemberId,Died,Buried,CauseOfDeath,RestingPlace,LegalDocuments,IsActive")] DeceasedMember deceasedMember)
        {
            if (id != deceasedMember.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(deceasedMember);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DeceasedMemberExists(deceasedMember.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "deceasedMember saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving deceasedMember. Please review your input.";
            ViewData["MemberId"] = new SelectList(_context.Members.Where(x => x.IsDeleted == false)
                .Select(x => new
                {
                    x.Id,
                    Member = x.FirstName + " " + x.MiddleName + " " + x.LastName
                }), "Id", "Member", deceasedMember.MemberId);
            return View(deceasedMember);
        }

        // GET: DeceasedMembers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.DeceasedMembers == null)
            {
                return NotFound();
            }

            var deceasedMember = await _context.DeceasedMembers
                .Include(d => d.Member)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (deceasedMember == null)
            {
                return NotFound();
            }

            return View(deceasedMember);
        }

        // POST: DeceasedMembers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.DeceasedMembers == null)
            {
                return Problem("Entity set 'EdrImsProjectContext.DeceasedMembers'  is null.");
            }
            var deceasedMember = await _context.DeceasedMembers.FindAsync(id);
            if (deceasedMember != null)
            {
                 deceasedMember.IsDeleted = true;
                _context.Update(deceasedMember);
                //_context.DeceasedMembers.Remove(deceasedMember);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DeceasedMemberExists(int id)
        {
          return _context.DeceasedMembers.Any(e => e.Id == id);
        }

        public IActionResult Activate(int id)
        {
            var user = _context.DeceasedMembers.Find(id);
            var members = _context.Members
                .Where(x => !x.IsDeleted && !x.IsActive && x.Id == user.MemberId)
                .ToList();

            foreach (var member in members)
            {
                if (members != null)
                {
                    user.IsActive = member.IsActive = true;
                    _context.Update(member);

                }
            }
            user.IsActive = true;
            _context.Update(user);
            _context.SaveChanges();
            TempData["Success"] = "Member activated successfully.";

            return RedirectToAction("Index", "DeceasedMembers");
        }
        public IActionResult Block(int id)
        {
            var user = _context.DeceasedMembers.Find(id);
            var members = _context.Members
                .Where(x => !x.IsDeleted && x.IsActive && x.Id == user.MemberId)
                .ToList();

            foreach (var member in members)
            {
                if (members != null)
                {
                    user.IsActive = member.IsActive = false;
                    _context.Update(member);

                }
            }
            user.IsActive = false;
            _context.Update(user);
            _context.SaveChanges();
            TempData["Success"] = "Member blocked successfully.";
            return RedirectToAction("Index", "DeceasedMembers");
        }

    }
}
