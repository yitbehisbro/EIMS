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
    public class EventTypesController : Controller
    {
        private readonly EdrImsProjectContext _context;

        public EventTypesController(EdrImsProjectContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult GetEventTypes ()
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
                var returnData = (from manudata in _context.EventTypes.Where(x=>x.IsDeleted==false) select manudata);
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

        // GET: EventTypes

       
        public IActionResult Index()
        {
            return View();
        }
        //public async Task<IActionResult> Index()
        //{
              //return View(await _context.EventTypes.Where(x=>!x.IsDeleted).ToListAsync());
        //}


        // GET: EventTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.EventTypes == null)
            {
                return NotFound();
            }

            var eventType = await _context.EventTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eventType == null)
            {
                return NotFound();
            }

            return View(eventType);
        }

        // GET: EventTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EventTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,IsActive")] EventType eventType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(eventType);
                await _context.SaveChangesAsync();
                TempData["Success"] = "eventType saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving eventType. Please review your input.";
            return View(eventType);
        }

        // GET: EventTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.EventTypes == null)
            {
                return NotFound();
            }

            var eventType = await _context.EventTypes.FindAsync(id);
            if (eventType == null)
            {
                return NotFound();
            }
            return View(eventType);
        }

        // POST: EventTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,IsActive")] EventType eventType)
        {
            if (id != eventType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(eventType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventTypeExists(eventType.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "eventType saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving eventType. Please review your input.";
            return View(eventType);
        }

        // GET: EventTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.EventTypes == null)
            {
                return NotFound();
            }

            var eventType = await _context.EventTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eventType == null)
            {
                return NotFound();
            }

            return View(eventType);
        }

        // POST: EventTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.EventTypes == null)
            {
                return Problem("Entity set 'EdrImsProjectContext.EventTypes'  is null.");
            }
            var eventType = await _context.EventTypes.FindAsync(id);
            if (eventType != null)
            {
                 eventType.IsDeleted = true;
                _context.Update(eventType);
                //_context.EventTypes.Remove(eventType);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventTypeExists(int id)
        {
          return _context.EventTypes.Any(e => e.Id == id);
        }

        public IActionResult Activate(int id)
        {
                var model = _context.EventTypes.Find(id);
                model.IsActive = true;
                _context.Update(model);
                _context.SaveChanges();
                TempData["Success"] = "eventType activated successfully.";
                return RedirectToAction("Index", "EventTypes");
        }
        public IActionResult Block(int id)
        {
                var model = _context.EventTypes.Find(id);
                model.IsActive = false;
                _context.Update(model);
                _context.SaveChanges();
                TempData["Success"] = "eventType blocked successfully.";
                return RedirectToAction("Index", "EventTypes");
        }

    }
}
