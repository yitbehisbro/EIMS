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
    public class RelationTypesController : Controller
    {
        private readonly EdrImsProjectContext _context;

        public RelationTypesController(EdrImsProjectContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult GetRelationTypes ()
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
                var returnData = (from manudata in _context.RelationTypes.Where(x=>x.IsDeleted==false) select manudata);
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

        // GET: RelationTypes

       
        public IActionResult Index()
        {
            return View();
        }
        //public async Task<IActionResult> Index()
        //{
              //return View(await _context.RelationTypes.Where(x=>!x.IsDeleted).ToListAsync());
        //}


        // GET: RelationTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.RelationTypes == null)
            {
                return NotFound();
            }

            var relationType = await _context.RelationTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (relationType == null)
            {
                return NotFound();
            }

            return View(relationType);
        }

        // GET: RelationTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RelationTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,IsActive")] RelationType relationType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(relationType);
                await _context.SaveChangesAsync();
                TempData["Success"] = "relationType saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving relationType. Please review your input.";
            return View(relationType);
        }

        // GET: RelationTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.RelationTypes == null)
            {
                return NotFound();
            }

            var relationType = await _context.RelationTypes.FindAsync(id);
            if (relationType == null)
            {
                return NotFound();
            }
            return View(relationType);
        }

        // POST: RelationTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,IsActive")] RelationType relationType)
        {
            if (id != relationType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(relationType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RelationTypeExists(relationType.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "relationType saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving relationType. Please review your input.";
            return View(relationType);
        }

        // GET: RelationTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.RelationTypes == null)
            {
                return NotFound();
            }

            var relationType = await _context.RelationTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (relationType == null)
            {
                return NotFound();
            }

            return View(relationType);
        }

        // POST: RelationTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.RelationTypes == null)
            {
                return Problem("Entity set 'EdrImsProjectContext.RelationTypes'  is null.");
            }
            var relationType = await _context.RelationTypes.FindAsync(id);
            if (relationType != null)
            {
                 relationType.IsDeleted = true;
                _context.Update(relationType);
                //_context.RelationTypes.Remove(relationType);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RelationTypeExists(int id)
        {
          return _context.RelationTypes.Any(e => e.Id == id);
        }
    }
}
