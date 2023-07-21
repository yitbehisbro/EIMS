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
    public class DeceasedMembersRelativesController : Controller
    {
        private readonly EdrImsProjectContext _context;

        public DeceasedMembersRelativesController(EdrImsProjectContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult GetDeceasedMembersRelatives ()
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
                var returnData = (from manudata in _context.DeceasedMembersRelatives.Where(x=>x.IsDeleted==false)
                                  .Include(x => x.MembersRelative)
                                  .ThenInclude(x => x.RelativesRelation)
                                  .ThenInclude(x => x.RelationType)
                                  .Select(x => new
                                  {
                                      x.Id,
                                      Buried = x.Buried.Value.ToString("dd MMM yyyy h:mm tt"),
                                      Died = x.Died.ToString("dd MMM yyyy h:mm tt"),
                                      x.CauseOfDeath,
                                      x.LegalDocuments,
                                      x.RestingPlace,
                                      RelativesRelation = x.MembersRelative.RelativesRelation.RelationType.Name,
                                      MemberRelativeId = x.MembersRelative.Id,
                                      MembersRelative = x.MembersRelative.FirstName + " " + x.MembersRelative.MiddleName + " " /*+ " " + x.MembersRelative.LastName*/,
                                      x.IsInclusive,
                                      x.IsActive
                                  })
                                  select manudata);
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    returnData = returnData.OrderBy(sortColumn + " " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    returnData = returnData.Where(m => m.MembersRelative.Contains(searchValue));
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

        // GET: DeceasedMembersRelatives

       
        public IActionResult Index()
        {
            return View();
        }
        //public async Task<IActionResult> Index()
        //{
            //var edrImsProjectContext = _context.DeceasedMembersRelatives.Include(d => d.MembersRelative);
            //return View(await edrImsProjectContext.ToListAsync());
        //}


        // GET: DeceasedMembersRelatives/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.DeceasedMembersRelatives == null)
            {
                return NotFound();
            }

            var deceasedMembersRelative = await _context.DeceasedMembersRelatives
                .Include(d => d.MembersRelative)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (deceasedMembersRelative == null)
            {
                return NotFound();
            }

            return View(deceasedMembersRelative);
        }

        // GET: DeceasedMembersRelatives/Create
        public IActionResult Create()
        {
            ViewData["MembersRelativeId"] = new SelectList(_context.MembersRelatives, "Id", "BirthPlace");
            return View();
        }

        // POST: DeceasedMembersRelatives/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MembersRelativeId,Died,Buried,CauseOfDeath,RestingPlace,LegalDocuments,IsInclusive,IsActive")] DeceasedMembersRelative deceasedMembersRelative)
        {
            if (ModelState.IsValid)
            {
                _context.Add(deceasedMembersRelative);
                await _context.SaveChangesAsync();
                TempData["Success"] = "deceasedMembersRelative saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving deceasedMembersRelative. Please review your input.";
            ViewData["MembersRelativeId"] = new SelectList(_context.MembersRelatives, "Id", "BirthPlace", deceasedMembersRelative.MembersRelativeId);
            return View(deceasedMembersRelative);
        }

        // GET: DeceasedMembersRelatives/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.DeceasedMembersRelatives == null)
            {
                return NotFound();
            }

            var deceasedMembersRelative = await _context.DeceasedMembersRelatives.FindAsync(id);
            if (deceasedMembersRelative == null)
            {
                return NotFound();
            }
            ViewData["MembersRelativeId"] = new SelectList(_context.MembersRelatives, "Id", "BirthPlace", deceasedMembersRelative.MembersRelativeId);
            return View(deceasedMembersRelative);
        }

        // POST: DeceasedMembersRelatives/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MembersRelativeId,Died,Buried,CauseOfDeath,RestingPlace,LegalDocuments,IsInclusive,IsActive")] DeceasedMembersRelative deceasedMembersRelative)
        {
            if (id != deceasedMembersRelative.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(deceasedMembersRelative);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DeceasedMembersRelativeExists(deceasedMembersRelative.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "deceasedMembersRelative saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving deceasedMembersRelative. Please review your input.";
            ViewData["MembersRelativeId"] = new SelectList(_context.MembersRelatives, "Id", "BirthPlace", deceasedMembersRelative.MembersRelativeId);
            return View(deceasedMembersRelative);
        }

        // GET: DeceasedMembersRelatives/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.DeceasedMembersRelatives == null)
            {
                return NotFound();
            }

            var deceasedMembersRelative = await _context.DeceasedMembersRelatives
                .Include(d => d.MembersRelative)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (deceasedMembersRelative == null)
            {
                return NotFound();
            }

            return View(deceasedMembersRelative);
        }

        // POST: DeceasedMembersRelatives/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.DeceasedMembersRelatives == null)
            {
                return Problem("Entity set 'EdrImsProjectContext.DeceasedMembersRelatives'  is null.");
            }
            var deceasedMembersRelative = await _context.DeceasedMembersRelatives.FindAsync(id);
            if (deceasedMembersRelative != null)
            {
                 deceasedMembersRelative.IsDeleted = true;
                _context.Update(deceasedMembersRelative);
                //_context.DeceasedMembersRelatives.Remove(deceasedMembersRelative);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DeceasedMembersRelativeExists(int id)
        {
          return _context.DeceasedMembersRelatives.Any(e => e.Id == id);
        }

        public IActionResult Activate(int id)
        {
            var user = _context.DeceasedMembersRelatives.Find(id);
            var memberRelatives = _context.MembersRelatives.Find(user.MembersRelativeId);

            if (memberRelatives != null && memberRelatives.IsActive == false)
            {
                user.IsActive = true;
                memberRelatives.IsActive = true;
                _context.Update(user);
                _context.Update(memberRelatives);
                _context.SaveChanges();
                TempData["Success"] = memberRelatives.FirstName + " " + memberRelatives.MiddleName + " " + memberRelatives.LastName + " activated successfully.";

            }
            else
            {
                TempData["Error"] = "Something wrong happened while operating the operation. Please contact the System Administrator.";
            }
            return RedirectToAction("Index", "DeceasedMembersRelatives");
        }
        public IActionResult Block(int id)
        {
            var user = _context.DeceasedMembersRelatives.Find(id);
            var memberRelatives = _context.MembersRelatives.Find(user.MembersRelativeId);

            if (memberRelatives != null && memberRelatives.IsActive == true)
            {
                user.IsActive = false;
                memberRelatives.IsActive = false;
                _context.Update(user);
                _context.Update(memberRelatives);
                _context.SaveChanges();
                TempData["Success"] = memberRelatives.FirstName + " " + memberRelatives.MiddleName + " " + memberRelatives.LastName + " blocked successfully.";
            }
            else
            {
                TempData["Error"] = "Something wrong happened while operating the operation. Please contact the System Administrator.";
            }

            return RedirectToAction("Index", "DeceasedMembersRelatives");
        }
        
        public IActionResult Inclusive(int id)
        {
            var user = _context.DeceasedMembersRelatives.Find(id);
            var memberRelatives = _context.MembersRelatives.Find(user.MembersRelativeId);
            user.IsInclusive = true;
            _context.Update(user);
            _context.SaveChanges();
            if (memberRelatives != null && !memberRelatives.IsDeleted)
            {
                TempData["Success"] = memberRelatives.FirstName + " " + memberRelatives.MiddleName + " " + memberRelatives.LastName + " included to a death notification payment successfully.";

            }
            else
            {
                TempData["Success"] = memberRelatives.FirstName + " " + memberRelatives.MiddleName + " " + memberRelatives.LastName + " included to a death notification payment successfully.";
            }

            return RedirectToAction("Index", "DeceasedMembersRelatives");
        }
        public IActionResult NotInclusive(int id)
        {
            var user = _context.DeceasedMembersRelatives.Find(id);
            user.IsInclusive = false;
            _context.Update(user);
            _context.SaveChanges();

            var memberRelatives = _context.MembersRelatives.Find(user.MembersRelativeId);
            if(memberRelatives != null && !memberRelatives.IsDeleted)
            {
                TempData["Success"] = memberRelatives.FirstName + " " + memberRelatives.MiddleName + " " + memberRelatives.LastName + " ignored from a death notification payment successfully.";

            }
            else
            {
                TempData["Success"] = memberRelatives.FirstName + " " + memberRelatives.MiddleName + " " + memberRelatives.LastName + " ignored from a death notification payment successfully.";
            }
            return RedirectToAction("Index", "DeceasedMembersRelatives");
        }
    }
}
