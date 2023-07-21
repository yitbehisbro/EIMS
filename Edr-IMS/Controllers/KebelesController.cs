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
    public class KebelesController : Controller
    {
        private readonly EdrImsProjectContext _context;

        public KebelesController(EdrImsProjectContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult GetKebeles ()
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
                var returnData = (from manudata in _context.Kebeles.Where(x=>x.IsDeleted==false) 
                                  .Include(x => x.SubCity)
                                  .Select(x => new
                                  {
                                      x.Id,
                                      x.Name,
                                      SubCity = x.SubCity.Name,
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


        public JsonResult GetKebelesBySubCityId(int subCityId)
        {
            var kebeles = _context.Kebeles.Where(k => k.SubCityId == subCityId && !k.IsDeleted)
                .Select(k => new { id = k.Id, name = k.Name })
                .ToList();
            return Json(kebeles);
        }

        // GET: Kebeles


        public IActionResult Index()
        {
            return View();
        }
        //public async Task<IActionResult> Index()
        //{
            //var edrImsProjectContext = _context.Kebeles.Include(k => k.SubCity);
            //return View(await edrImsProjectContext.ToListAsync());
        //}


        // GET: Kebeles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Kebeles == null)
            {
                return NotFound();
            }

            var kebele = await _context.Kebeles
                .Include(k => k.SubCity)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (kebele == null)
            {
                return NotFound();
            }

            return View(kebele);
        }

        // GET: Kebeles/Create
        public IActionResult Create()
        {
            ViewData["SubCityId"] = new SelectList(_context.SubCities, "Id", "Name");
            return View();
        }

        // POST: Kebeles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,SubCityId,IsActive")] Kebele kebele)
        {
            if (ModelState.IsValid)
            {
                _context.Add(kebele);
                await _context.SaveChangesAsync();
                TempData["Success"] = "kebele saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving kebele. Please review your input.";
            ViewData["SubCityId"] = new SelectList(_context.SubCities, "Id", "Name", kebele.SubCityId);
            return View(kebele);
        }

        // GET: Kebeles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Kebeles == null)
            {
                return NotFound();
            }

            var kebele = await _context.Kebeles.FindAsync(id);
            if (kebele == null)
            {
                return NotFound();
            }
            ViewData["SubCityId"] = new SelectList(_context.SubCities, "Id", "Name", kebele.SubCityId);
            return View(kebele);
        }

        // POST: Kebeles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,SubCityId,IsActive")] Kebele kebele)
        {
            if (id != kebele.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(kebele);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KebeleExists(kebele.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "kebele saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving kebele. Please review your input.";
            ViewData["SubCityId"] = new SelectList(_context.SubCities, "Id", "Name", kebele.SubCityId);
            return View(kebele);
        }

        // GET: Kebeles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Kebeles == null)
            {
                return NotFound();
            }

            var kebele = await _context.Kebeles
                .Include(k => k.SubCity)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (kebele == null)
            {
                return NotFound();
            }

            return View(kebele);
        }

        // POST: Kebeles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Kebeles == null)
            {
                return Problem("Entity set 'EdrImsProjectContext.Kebeles'  is null.");
            }
            var kebele = await _context.Kebeles.FindAsync(id);
            if (kebele != null)
            {
                 kebele.IsDeleted = true;
                _context.Update(kebele);
                //_context.Kebeles.Remove(kebele);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KebeleExists(int id)
        {
          return _context.Kebeles.Any(e => e.Id == id);
        }
    }
}
