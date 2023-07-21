using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using EdrIMS.Models;
using System.Security.Claims;

namespace EdrIMS.Controllers
{
    public class EventParticipantsController : Controller
    {
        private readonly EdrImsProjectContext _context;

        public EventParticipantsController(EdrImsProjectContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult GetEventParticipants ()
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
                var returnData = (from manudata in _context.EventParticipants.Where(x => x.IsDeleted == false 
                                    && !x.EdrEvent.IsDeleted && x.EdrEvent.IsActive)
                                  .Include(e => e.EdrEvent)
                                  .ThenInclude(e => e.EventType)
                                  .Include(e => e.Participant)
                                  .Select(x => new
                                  {
                                      x.Id,
                                      EdrEvent = x.EdrEvent.EventType.Name,
                                      DateOfEvent = x.EdrEvent.DateOfEvent.ToString("ddd, dd MMM yyyy hh:mm tt"),
                                      x.HasSeenEvent,
                                      Participant = x.Participant.FirstName + " " + x.Participant.MiddleName,
                                      EventParticipant = x.Participant.FirstName + " " + x.Participant.MiddleName,
                                      x.IsParticipated,
                                      x.IsActive
                                  })
                                  select manudata);
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    returnData = returnData.OrderBy(sortColumn + " " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    returnData = returnData.Where(m =>
                                    m.EdrEvent.Contains(searchValue) ||
                                    m.DateOfEvent.Contains(searchValue) ||
                                    m.EventParticipant.Contains(searchValue)
                                    );
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

        // GET: EventParticipants

       
        public IActionResult Index()
        {
              return View();
        }

        //public async Task<IActionResult> Index()
        //{
            //var edrImsProjectContext = _context.EventParticipants.Include(e => e.EdrEvent).Include(e => e.Participant);
            //return View(await edrImsProjectContext.ToListAsync());
        //}


        // GET: EventParticipants/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.EventParticipants == null)
            {
                return NotFound();
            }

            var eventParticipant = await _context.EventParticipants
                .Include(e => e.EdrEvent)
                .Include(e => e.Participant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eventParticipant == null)
            {
                return NotFound();
            }

            return View(eventParticipant);
        }

        // GET: EventParticipants/Create
        public IActionResult Create()
        {
            if (User.IsInRole("Edrtegna"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            ViewData["EdrEventId"] = new SelectList(_context.EdrEvents
                .Include(x => x.EventType)
                .Where(x => !x.IsDeleted && x.IsActive/* && x.IsPublished == false*/)
                .Select(x => new
                {
                    x.Id,
                    Name = x.EventType.Name + "("+ x.DateOfEvent.ToString("ddd, dd MMM yyyy hh:mm tt") + ")",
                })
                , "Id", "Name");
            ViewData["ParticipantId"] = new SelectList(_context.Members
                .Where(x => !x.IsDeleted && x.IsActive && x.IsAlive)
                .Select(x => new
                {
                    x.Id,
                    Name = x.FirstName + " " + x.MiddleName + " " + x.LastName,
                })
                , "Id", "Name");
            return View();
        }

        // POST: EventParticipants/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EdrEventId,ParticipantId,IsParticipated,IsActive")] EventParticipant eventParticipant)
        {
            if (ModelState.IsValid)
            {
                if (User.IsInRole("Edrtegna"))
                {
                    TempData["Error"] = "You don't have sufficient previlege to access this resource.";
                    return RedirectToAction("Details", "EdrEvents", new { id = eventParticipant.EdrEventId });
                }
                if (eventParticipant.ParticipantId == 0)
                {
                    var eventParticipants = _context.Members
                            .Where(x => !x.IsDeleted && x.IsActive && x.IsAlive)
                            .Select(m => new EventParticipant
                            {
                                ParticipantId = m.Id,
                                HasSeenEvent = false,
                                IsParticipated = false,
                                IsActive = true,
                                IsDeleted = false,
                                EdrEventId = eventParticipant.EdrEventId
                            })
                            .ToList();

                    _context.EventParticipants.AddRange(eventParticipants);
                    _context.SaveChanges();

                    TempData["Success"] = "Your request to add all members to the event has been completed successfully.";
                    //return RedirectToAction(nameof(Index));
                    return RedirectToAction("Details", "EdrEvents", new {id = eventParticipant.EdrEventId});
                }

                eventParticipant.HasSeenEvent = false;
                eventParticipant.IsParticipated = false;

                _context.Add(eventParticipant);
                await _context.SaveChangesAsync();
                TempData["Success"] = "eventParticipant saved successfully.";
                //return RedirectToAction(nameof(Index));
                return RedirectToAction("Details", "EdrEvents", new { id = eventParticipant.EdrEventId });
            }
            TempData["Error"] = "An error occured while saving eventParticipant. Please review your input.";
            if (TempData["Error"] != null)
            {
                return RedirectToAction("Details", "EdrEvents", new { id = eventParticipant.EdrEventId });
            }
            ViewData["EdrEventId"] = new SelectList(_context.EdrEvents
                .Include(x => x.EventType)
                .Where(x => !x.IsDeleted && x.IsActive && x.IsPublished == false)
                .Select(x => new
                {
                    x.Id,
                    Name = x.EventType.Name + "(" + x.DateOfEvent.ToString("ddd, dd MMM yyyy hh:mm tt") + ")",
                })
                , "Id", "Name", eventParticipant.EdrEventId);
            ViewData["ParticipantId"] = new SelectList(_context.Members.Where(x => !x.IsDeleted && x.IsActive && x.IsAlive).Select(x => new { x.Id, Name = x.FirstName + " " + x.MiddleName + " " + x.LastName, }), "Id", "Name", eventParticipant.ParticipantId);
            return View(eventParticipant);
        }

        // GET: EventParticipants/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            //if (User.IsInRole("Edrtegna"))
            //{
            //    return RedirectToAction("AccessDenied", "Account");
            //}
            if (id == null || _context.EventParticipants == null)
            {
                return NotFound();
            }

            var eventParticipant = await _context.EventParticipants.FindAsync(id);
            if (User.IsInRole("Edrtegna"))
            {
                TempData["Error"] = "You don't have sufficient previlege to access this resource.";
                return RedirectToAction("Details", "EdrEvents", new { id = eventParticipant.EdrEventId });
            }
            if (eventParticipant == null)
            {
                return NotFound();
            }
            ViewData["EdrEventId"] = new SelectList(_context.EdrEvents
                .Include(x => x.EventType)
                .Where(x => !x.IsDeleted && x.IsActive && x.IsPublished == false)
                .Select(x => new
                {
                    x.Id,
                    Name = x.EventType.Name + "(" + x.DateOfEvent.ToString("ddd, dd MMM yyyy hh:mm tt") + ")"   
                })
                , "Id", "Name", eventParticipant.EdrEventId);

            ViewData["ParticipantId"] = new SelectList(_context.Members
                .Where(x => !x.IsDeleted && x.IsActive && x.IsAlive)
                .Select(x => new
                {
                    x.Id,
                    Name = x.FirstName + " " + x.MiddleName + " " + x.LastName,
                })
                , "Id", "Name", eventParticipant.ParticipantId);
            return View(eventParticipant);
        }

        // POST: EventParticipants/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EdrEventId,ParticipantId,IsParticipated,IsActive")] EventParticipant eventParticipant)
        {
            if (User.IsInRole("Edrtegna"))
            {
                TempData["Error"] = "You don't have sufficient previlege to access this resource.";
                return RedirectToAction("Details", "EdrEvents", new { id = eventParticipant.EdrEventId });
            }
            if (id != eventParticipant.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    eventParticipant.IsParticipated = false;
                    _context.Update(eventParticipant);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventParticipantExists(eventParticipant.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "eventParticipant saved successfully.";
                //return RedirectToAction(nameof(Index));
                return RedirectToAction("Details", "EdrEvents", new { id = eventParticipant.EdrEventId });
            }
            TempData["Error"] = "An error occured while saving eventParticipant. Please review your input.";
            if (TempData["Error"] != null)
            {
                return RedirectToAction("Details", "EdrEvents", new { id = eventParticipant.EdrEventId });
            }
            ViewData["EdrEventId"] = new SelectList(_context.EdrEvents
                .Include(x => x.EventType)
                .Where(x => !x.IsDeleted && x.IsActive && x.IsPublished == false)
                .Select(x => new
                {
                    x.Id,
                    Name = x.EventType.Name + "(" + x.DateOfEvent.ToString("ddd, dd MMM yyyy hh:mm tt") + ")",
                })
                , "Id", "Name"
                , eventParticipant.EdrEventId);
            ViewData["ParticipantId"] = new SelectList(_context.Members
                .Where(x => !x.IsDeleted && x.IsActive && x.IsAlive)
                .Select(x => new
                {
                    x.Id,
                    Name = x.FirstName + " " + x.MiddleName + " " + x.LastName,
                })
                , "Id", "Name", eventParticipant.ParticipantId);
            return View(eventParticipant);
        }

        // GET: EventParticipants/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            //if (User.IsInRole("Edrtegna"))
            //{
            //    return RedirectToAction("AccessDenied", "Account");
            //}
            if (id == null || _context.EventParticipants == null)
            {
                return NotFound();
            }

            var eventParticipant = await _context.EventParticipants
                .Include(e => e.EdrEvent)
                .Include(e => e.Participant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (User.IsInRole("Edrtegna"))
            {
                TempData["Error"] = "You don't have sufficient previlege to access this resource.";
                return RedirectToAction("Details", "EdrEvents", new { id = eventParticipant.EdrEventId });
            }
            if (eventParticipant == null)
            {
                return NotFound();
            }

            return View(eventParticipant);
        }

        // POST: EventParticipants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventParticipant = await _context.EventParticipants.FindAsync(id);

            if (User.IsInRole("Edrtegna"))
            {
                TempData["Error"] = "You don't have sufficient previlege to access this resource.";
                return RedirectToAction("Details", "EdrEvents", new { id = eventParticipant.EdrEventId });
            }
            if (_context.EventParticipants == null)
            {
                return Problem("Entity set 'EdrImsProjectContext.EventParticipants'  is null.");
            }
            if (eventParticipant != null)
            {
                 eventParticipant.IsDeleted = true;
                _context.Update(eventParticipant);
                TempData["Success"] = "The participant has been deleted successfully!";
            }
            
            await _context.SaveChangesAsync();
            //return RedirectToAction(nameof(Index));
            return RedirectToAction("Details", "EdrEvents", new { id = eventParticipant.EdrEventId });
        }

        private bool EventParticipantExists(int id)
        {
          return _context.EventParticipants.Any(e => e.Id == id);
        }

        public IActionResult Activate(int id)
        {
            var model = _context.EventParticipants.Find(id);
            if (User.IsInRole("Edrtegna"))
            {
                TempData["Error"] = "You don't have sufficient previlege to access this resource.";
                return RedirectToAction("Details", "EdrEvents", new { id = model.EdrEventId });
            }
            model.IsActive = true;
            _context.Update(model);
            _context.SaveChanges();
            TempData["Success"] = "eventParticipant activated successfully.";
            //return RedirectToAction("Index", "EventParticipants");
            return RedirectToAction("Details", "EdrEvents", new { id = model.EdrEventId });
        }
        public IActionResult Block(int id)
        {
            var model = _context.EventParticipants.Find(id);
            if (User.IsInRole("Edrtegna"))
            {
                TempData["Error"] = "You don't have sufficient previlege to access this resource.";
                return RedirectToAction("Details", "EdrEvents", new { id = model.EdrEventId });
            }
            
                model.IsActive = false;
                _context.Update(model);
                _context.SaveChanges();
                TempData["Success"] = "eventParticipant blocked successfully.";
                //return RedirectToAction("Index", "EventParticipants");
                return RedirectToAction("Details", "EdrEvents", new { id = model.EdrEventId });
        }

    }
}
