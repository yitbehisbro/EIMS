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
    public class RelativesRelationsController : Controller
    {
        private readonly EdrImsProjectContext _context;

        public RelativesRelationsController(EdrImsProjectContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult GetRelativesRelations()
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
                var returnData = (from manudata in _context.RelativesRelations.Where(x => x.IsDeleted == false)
                                  .Include(x => x.Member)
                                  .Include(x => x.RelationType)
                                  .Select(x => new
                                  {
                                      x.Id,
                                      Member = x.Member.FirstName + " " + x.Member.MiddleName + " " + x.Member.LastName,
                                      RelationType = x.RelationType.Name,
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

        // GET: RelativesRelations


        public IActionResult Index()
        {
            return View();
        }
        //public async Task<IActionResult> Index()
        //{
        //var edrImsProjectContext = _context.RelativesRelations.Include(r => r.Member);
        //return View(await edrImsProjectContext.ToListAsync());
        //}


        // GET: RelativesRelations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.RelativesRelations == null)
            {
                return NotFound();
            }

            var relativesRelation = await _context.RelativesRelations
                .Include(r => r.Member)
                .ThenInclude(r => r.Gender)
                .Include(r => r.RelationType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (relativesRelation == null)
            {
                return NotFound();
            }

            return View(relativesRelation);
        }

        // GET: RelativesRelations/Create
        public IActionResult Create()
        {
            ViewData["MemberId"] = new SelectList(_context.Members.Where(x => x.IsActive && !x.IsDeleted)
                .Select(x => new
                {
                    x.Id,
                    Member = x.FirstName + " " + x.MiddleName + " " + x.LastName
                })
                , "Id", "Member");
            
            ViewData["RelationTypeId"] = new SelectList(_context.RelationTypes.Where(x => x.IsActive && !x.IsDeleted)
                .Select(x => new
                {
                    x.Id,
                    RelationType = x.Name
                })
                , "Id", "RelationType");
            return View();
        }

        // POST: RelativesRelations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MemberId,RelationTypeId,IsActive")] RelativesRelation relativesRelation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(relativesRelation);
                await _context.SaveChangesAsync();
                TempData["Success"] = "relativesRelation saved successfully.";
                return RedirectToAction("Details", "Members", new {id = relativesRelation.MemberId});
            }
            TempData["Error"] = "An error occured while saving relativesRelation. Please review your input.";
            if (TempData["Error"] != null)
            {
                return RedirectToAction("Details", "Members", new { id = relativesRelation.MemberId });
                
            }
            ViewData["MemberId"] = new SelectList(_context.Members
                .Where(x => x.IsActive && !x.IsDeleted)
                .Select(x => new
                {
                    x.Id,
                    Member = x.FirstName + " " + x.MiddleName + " " + x.LastName
                })
                , "Id", "Member", relativesRelation.MemberId);

            ViewData["RelationTypeId"] = new SelectList(_context.RelationTypes.Where(x => x.IsActive && !x.IsDeleted)
                .Select(x => new
                {
                    x.Id,
                    RelationType = x.Name
                })
                , "Id", "RelationType", relativesRelation.RelationTypeId);
            return View(relativesRelation);
        }

        // GET: RelativesRelations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.RelativesRelations == null)
            {
                return NotFound();
            }

            var relativesRelation = await _context.RelativesRelations.FindAsync(id);
            if (relativesRelation == null)
            {
                return NotFound();
            }
            ViewData["MemberId"] = new SelectList(_context.Members.Where(x => x.IsActive && !x.IsDeleted)
                .Select(x => new
                {
                    x.Id,
                    Member = x.FirstName + " " + x.MiddleName + " " + x.LastName
                })
                , "Id", "Member", relativesRelation.MemberId);

            ViewData["RelationTypeId"] = new SelectList(_context.RelationTypes.Where(x => x.IsActive && !x.IsDeleted)
                .Select(x => new
                {
                    x.Id,
                    RelationType = x.Name
                })
                , "Id", "RelationType", relativesRelation.RelationTypeId);
            return View(relativesRelation);
        }

        // POST: RelativesRelations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MemberId,RelationTypeId,IsActive")] RelativesRelation relativesRelation)
        {
            if (id != relativesRelation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(relativesRelation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RelativesRelationExists(relativesRelation.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "relativesRelation saved successfully.";
                return RedirectToAction("Details", "Members", new {id = relativesRelation.MemberId});
                //return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving relativesRelation. Please review your input.";
            if (TempData["Error"] != null)
            {
                return RedirectToAction("Details", "Members", new { id = relativesRelation.MemberId });
            }
            ViewData["MemberId"] = new SelectList(_context.Members.Where(x => x.IsActive && !x.IsDeleted)
                .Select(x => new
                {
                    x.Id,
                    Member = x.FirstName + " " + x.MiddleName + " " + x.LastName
                })
                , "Id", "Member", relativesRelation.MemberId);
            ViewData["RelationTypeId"] = new SelectList(_context.RelationTypes.Where(x => x.IsActive && !x.IsDeleted)
                .Select(x => new
                {
                    x.Id,
                    RelationType = x.Name
                })
                , "Id", "RelationType", relativesRelation.RelationTypeId);
            return View(relativesRelation);
        }

        // GET: RelativesRelations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.RelativesRelations == null)
            {
                return NotFound();
            }

            var relativesRelation = await _context.RelativesRelations
                .Include(r => r.Member)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (relativesRelation == null)
            {
                return NotFound();
            }

            return View(relativesRelation);
        }

        // POST: RelativesRelations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.RelativesRelations == null)
            {
                return Problem("Entity set 'EdrImsProjectContext.RelativesRelations'  is null.");
            }
            var relativesRelation = await _context.RelativesRelations.FindAsync(id);
            if (relativesRelation != null)
            {
                relativesRelation.IsDeleted = true;
                _context.Update(relativesRelation);
                TempData["Success"] = "You deleted a relatives relation successfully!";
                //_context.RelativesRelations.Remove(relativesRelation);
            }

            await _context.SaveChangesAsync();
            //return RedirectToAction(nameof(Index));
            return RedirectToAction("Details", "Members", new { id = relativesRelation.MemberId });
        }

        private bool RelativesRelationExists(int id)
        {
            return _context.RelativesRelations.Any(e => e.Id == id);
        }

        public IActionResult Activate(int id)
        {
            var model = _context.RelativesRelations.Find(id);
            if (User.IsInRole("Edrtegna"))
            {
                TempData["Error"] = "You don't have sufficient previlege to access this resource.";
                return RedirectToAction("Details", "Members", new { id = model.MemberId });
            }

            model.IsActive = true;
            _context.Update(model);
            _context.SaveChanges();
            TempData["Success"] = "Members relation activated successfully.";
            //return RedirectToAction("Index", "EdrEvents");
            return RedirectToAction("Details", "Members", new { id = model.MemberId });

        }
        public IActionResult Block(int id)
        {
            var model = _context.RelativesRelations.Find(id);
            if (User.IsInRole("Edrtegna"))
            {
                TempData["Error"] = "You don't have sufficient previlege to access this resource.";
                return RedirectToAction("Details", "Members", new { id = model.MemberId });
            }

            model.IsActive = false;
            _context.Update(model);
            _context.SaveChanges();
            TempData["Success"] = "Members relation blocked successfully.";
            //return RedirectToAction("Index", "EdrEvents");
            return RedirectToAction("Details", "Members", new { id = model.MemberId });

        }
    }
}
