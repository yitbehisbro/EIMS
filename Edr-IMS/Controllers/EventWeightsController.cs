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
    public class EventWeightsController : Controller
    {
        private readonly EdrImsProjectContext _context;

        public EventWeightsController(EdrImsProjectContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult GetEventWeights ()
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
                var returnData = (from manudata in _context.EventWeights.Where(x=>x.IsDeleted==false) select manudata);
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

        // GET: EventWeights

       
        public IActionResult Index()
        {
            return View();
        }
        //public async Task<IActionResult> Index()
        //{
              //return View(await _context.EventWeights.Where(x=>!x.IsDeleted).ToListAsync());
        //}


        // GET: EventWeights/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.EventWeights == null)
            {
                return NotFound();
            }

            var eventWeight = await _context.EventWeights
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eventWeight == null)
            {
                return NotFound();
            }

            return View(eventWeight);
        }

        // GET: EventWeights/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EventWeights/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,IsActive")] EventWeight eventWeight)
        {
            if (ModelState.IsValid)
            {
                _context.Add(eventWeight);
                await _context.SaveChangesAsync();
                TempData["Success"] = "eventWeight saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving eventWeight. Please review your input.";
            return View(eventWeight);
        }

        // GET: EventWeights/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.EventWeights == null)
            {
                return NotFound();
            }

            var eventWeight = await _context.EventWeights.FindAsync(id);
            if (eventWeight == null)
            {
                return NotFound();
            }
            return View(eventWeight);
        }

        // POST: EventWeights/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,IsActive")] EventWeight eventWeight)
        {
            if (id != eventWeight.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(eventWeight);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventWeightExists(eventWeight.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "eventWeight saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving eventWeight. Please review your input.";
            return View(eventWeight);
        }

        // GET: EventWeights/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.EventWeights == null)
            {
                return NotFound();
            }

            var eventWeight = await _context.EventWeights
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eventWeight == null)
            {
                return NotFound();
            }

            return View(eventWeight);
        }

        // POST: EventWeights/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.EventWeights == null)
            {
                return Problem("Entity set 'EdrImsProjectContext.EventWeights'  is null.");
            }
            var eventWeight = await _context.EventWeights.FindAsync(id);
            if (eventWeight != null)
            {
                 eventWeight.IsDeleted = true;
                _context.Update(eventWeight);
                //_context.EventWeights.Remove(eventWeight);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventWeightExists(int id)
        {
          return _context.EventWeights.Any(e => e.Id == id);
        }

        public IActionResult Activate(int id)
        {
                var model = _context.EventWeights.Find(id);
                model.IsActive = true;
                _context.Update(model);
                _context.SaveChanges();
                TempData["Success"] = "eventWeight activated successfully.";
                return RedirectToAction("Index", "EventWeights");
        }
        public IActionResult Block(int id)
        {
                var model = _context.EventWeights.Find(id);
                model.IsActive = false;
                _context.Update(model);
                _context.SaveChanges();
                TempData["Success"] = "eventWeight blocked successfully.";
                return RedirectToAction("Index", "EventWeights");
        }

    }
}
