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
    public class SubCitiesController : Controller
    {
        private readonly EdrImsProjectContext _context;

        public SubCitiesController(EdrImsProjectContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult GetSubCities ()
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
                var returnData = (from manudata in _context.SubCities.Where(x=>x.IsDeleted==false)
                                  .Include(x => x.City)
                                  .Select(x => new
                                  {
                                      x.Id,
                                      x.Name,
                                      City = x.City.Name,
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

        public JsonResult GetSubCitiesByCityId(int cityId)
        {
            var subCities = _context.SubCities.Where(s => s.CityId == cityId && !s.IsDeleted)
                .Select(s => new { id = s.Id, name = s.Name })
                .ToList();
            return Json(subCities);
        }

        // GET: SubCities


        public IActionResult Index()
        {
            return View();
        }
        //public async Task<IActionResult> Index()
        //{
            //var edrImsProjectContext = _context.SubCities.Include(s => s.City);
            //return View(await edrImsProjectContext.ToListAsync());
        //}


        // GET: SubCities/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.SubCities == null)
            {
                return NotFound();
            }

            var subCity = await _context.SubCities
                .Include(s => s.City)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subCity == null)
            {
                return NotFound();
            }

            return View(subCity);
        }

        // GET: SubCities/Create
        public IActionResult Create()
        {
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name");
            return View();
        }

        // POST: SubCities/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,CityId,IsActive")] SubCity subCity)
        {
            if (ModelState.IsValid)
            {
                _context.Add(subCity);
                await _context.SaveChangesAsync();
                TempData["Success"] = "subCity saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving subCity. Please review your input.";
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", subCity.CityId);
            return View(subCity);
        }

        // GET: SubCities/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.SubCities == null)
            {
                return NotFound();
            }

            var subCity = await _context.SubCities.FindAsync(id);
            if (subCity == null)
            {
                return NotFound();
            }
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", subCity.CityId);
            return View(subCity);
        }

        // POST: SubCities/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CityId,IsActive")] SubCity subCity)
        {
            if (id != subCity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(subCity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubCityExists(subCity.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "subCity saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving subCity. Please review your input.";
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", subCity.CityId);
            return View(subCity);
        }

        // GET: SubCities/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.SubCities == null)
            {
                return NotFound();
            }

            var subCity = await _context.SubCities
                .Include(s => s.City)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subCity == null)
            {
                return NotFound();
            }

            return View(subCity);
        }

        // POST: SubCities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.SubCities == null)
            {
                return Problem("Entity set 'EdrImsProjectContext.SubCities'  is null.");
            }
            var subCity = await _context.SubCities.FindAsync(id);
            if (subCity != null)
            {
                 subCity.IsDeleted = true;
                _context.Update(subCity);
                //_context.SubCities.Remove(subCity);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SubCityExists(int id)
        {
          return _context.SubCities.Any(e => e.Id == id);
        }
    }
}
