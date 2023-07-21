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
    public class EventAttendanceModesController : Controller
    {
        private readonly EdrImsProjectContext _context;

        public EventAttendanceModesController(EdrImsProjectContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult GetEventAttendanceModes ()
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
                var returnData = (from manudata in _context.EventAttendanceModes.Where(x=>x.IsDeleted==false) select manudata);
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

        // GET: EventAttendanceModes

       
        public IActionResult Index()
        {
            return View();
        }
        //public async Task<IActionResult> Index()
        //{
              //return View(await _context.EventAttendanceModes.Where(x=>!x.IsDeleted).ToListAsync());
        //}


        // GET: EventAttendanceModes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.EventAttendanceModes == null)
            {
                return NotFound();
            }

            var eventAttendanceMode = await _context.EventAttendanceModes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eventAttendanceMode == null)
            {
                return NotFound();
            }

            return View(eventAttendanceMode);
        }

        // GET: EventAttendanceModes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EventAttendanceModes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,IsActive")] EventAttendanceMode eventAttendanceMode)
        {
            if (ModelState.IsValid)
            {
                _context.Add(eventAttendanceMode);
                await _context.SaveChangesAsync();
                TempData["Success"] = "eventAttendanceMode saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving eventAttendanceMode. Please review your input.";
            return View(eventAttendanceMode);
        }

        // GET: EventAttendanceModes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.EventAttendanceModes == null)
            {
                return NotFound();
            }

            var eventAttendanceMode = await _context.EventAttendanceModes.FindAsync(id);
            if (eventAttendanceMode == null)
            {
                return NotFound();
            }
            return View(eventAttendanceMode);
        }

        // POST: EventAttendanceModes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,IsActive")] EventAttendanceMode eventAttendanceMode)
        {
            if (id != eventAttendanceMode.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(eventAttendanceMode);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventAttendanceModeExists(eventAttendanceMode.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "eventAttendanceMode saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving eventAttendanceMode. Please review your input.";
            return View(eventAttendanceMode);
        }

        // GET: EventAttendanceModes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.EventAttendanceModes == null)
            {
                return NotFound();
            }

            var eventAttendanceMode = await _context.EventAttendanceModes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eventAttendanceMode == null)
            {
                return NotFound();
            }

            return View(eventAttendanceMode);
        }

        // POST: EventAttendanceModes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.EventAttendanceModes == null)
            {
                return Problem("Entity set 'EdrImsProjectContext.EventAttendanceModes'  is null.");
            }
            var eventAttendanceMode = await _context.EventAttendanceModes.FindAsync(id);
            if (eventAttendanceMode != null)
            {
                 eventAttendanceMode.IsDeleted = true;
                _context.Update(eventAttendanceMode);
                //_context.EventAttendanceModes.Remove(eventAttendanceMode);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventAttendanceModeExists(int id)
        {
          return _context.EventAttendanceModes.Any(e => e.Id == id);
        }

        public IActionResult Activate(int id)
        {
                var model = _context.EventAttendanceModes.Find(id);
                model.IsActive = true;
                _context.Update(model);
                _context.SaveChanges();
                            TempData["Success"] = "eventAttendanceMode activated successfully.";
                return RedirectToAction("Index", "EventAttendanceModes");
        }
        public IActionResult Block(int id)
        {
                var model = _context.EventAttendanceModes.Find(id);
                model.IsActive = false;
                _context.Update(model);
                _context.SaveChanges();
                TempData["Success"] = "eventAttendanceMode blocked successfully.";
                return RedirectToAction("Index", "EventAttendanceModes");
        }

    }
}
