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
    public class EdrEventsController : Controller
    {
        private readonly EdrImsProjectContext _context;

        public EdrEventsController(EdrImsProjectContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult GetEdrEvents ()
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
                var returnData = (from manudata in _context.EdrEvents.Where(x=>x.IsDeleted==false)
                                  .Include(e => e.EventAttendanceMode)
                                  .Include(e => e.EventType)
                                  .Include(e => e.EventWeight)
                                  .Select(e => new
                                  {
                                      e.Id,
                                      EventType = e.EventType.Name,
                                      EventAttendanceMode = e.EventAttendanceMode.Name,
                                      EventWeight = e.EventWeight.Name,
                                      e.Location,
                                      e.IsPublished,
                                      DateOfEvent = e.DateOfEvent.ToString("ddd, dd MMM yyyy hh:mm tt"),
                                      e.Description,
                                      e.IsActive
                                  })
                                  select manudata);

                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    // Handle the case where the user is not authenticated
                    // For example, you could redirect them to the login page
                    return Redirect("/");
                }

                var user = _context.Users.FirstOrDefault(u => u.Uuid == userId);

                if (user == null)
                {
                    // Handle the case where no user was found
                    return NotFound("User not found");
                }

                // Get USer Id from the given table
                //int memberId = user.Id;
                var member = _context.Members.FirstOrDefault(u => u.UsersId == user.Id);
                if (member == null)
                {
                    // Handle the case where no customer was found
                    //return NotFound("You are not registered as a Customer");
                    TempData["Error"] = "You are not registered as a Customer. Please contact the Customer Service.";
                    //return RedirectToAction(nameof(Index));
                    return Redirect("/");
                }

                /**
                 * This code makes the event has seen by the participants.
                 * 
                 * Starts
                 * 
                 */
                var eventParticipant = _context.EventParticipants
                     .Where(x => x.IsActive && !x.IsDeleted && x.ParticipantId == member.Id/* && x.EdrEventId == item.Id*/)
                     .ToList();

                if (eventParticipant != null)
                {
                    foreach (var item in eventParticipant)
                    {
                        item.HasSeenEvent = true;
                        _context.Update(item);
                        _context.SaveChanges();
                    }
                }
                /**
                 * This code makes the event has seen by the participants.
                 * 
                 * Ends
                 * 
                 */
                var participant = _context.EventParticipants
                        .Where(x => x.IsActive && !x.IsDeleted && x.ParticipantId == member.Id)
                        .ToList();
                if (participant == null)
                {
                    returnData = (from manudata in _context.EdrEvents.Where(x => x.IsDeleted == false && x.Id == 0)
                                  .Include(e => e.EventAttendanceMode)
                                  .Include(e => e.EventType)
                                  .Include(e => e.EventWeight)
                                  .Select(e => new
                                  {
                                      e.Id,
                                      EventType = e.EventType.Name,
                                      EventAttendanceMode = e.EventAttendanceMode.Name,
                                      EventWeight = e.EventWeight.Name,
                                      e.Location,
                                      e.IsPublished,
                                      DateOfEvent = e.DateOfEvent.ToString("ddd, dd MMM yyyy hh:mm tt"),
                                      e.Description,
                                      e.IsActive
                                  })
                                  select manudata);
                }
                else if (participant != null && User.IsInRole("Edrtegna")) {

                    foreach(var item in participant)
                    {
                        returnData = (from manudata in _context.EdrEvents.Where(x => x.IsDeleted == false && x.IsPublished == true && x.Id == item.EdrEventId)
                                  .Include(e => e.EventAttendanceMode)
                                  .Include(e => e.EventType)
                                  .Include(e => e.EventWeight)
                                  .Select(e => new
                                  {
                                      e.Id,
                                      EventType = e.EventType.Name,
                                      EventAttendanceMode = e.EventAttendanceMode.Name,
                                      EventWeight = e.EventWeight.Name,
                                      e.Location,
                                      e.IsPublished,
                                      DateOfEvent = e.DateOfEvent.ToString("ddd, dd MMM yyyy hh:mm tt"),
                                      e.Description,
                                      e.IsActive
                                  })
                                      select manudata);
                    }
                }
                
                
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    returnData = returnData.OrderBy(sortColumn + " " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    returnData = returnData.Where(m =>
                            m.EventType.Contains(searchValue)||
                            m.EventAttendanceMode.Contains(searchValue)|| 
                            m.EventWeight.Contains(searchValue)|| 
                            m.Location.Contains(searchValue)|| 
                            m.DateOfEvent.Contains(searchValue)
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

        // GET: EdrEvents
        public IActionResult Index()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                // Handle the case where the user is not authenticated
                // For example, you could redirect them to the login page
                return Redirect("/");
            }

            var user = _context.Users.FirstOrDefault(u => u.Uuid == userId);

            if (user == null)
            {
                // Handle the case where no user was found
                return NotFound("User not found");
            }

            // Get USer Id from the given table
            //int memberId = user.Id;
            var member = _context.Members.FirstOrDefault(u => u.UsersId == user.Id);
            if (member == null)
            {
                // Handle the case where no customer was found
                //return NotFound("You are not registered as a Customer");
                TempData["Error"] = "You are not registered as a Customer. Please contact the Customer Service.";
                //return RedirectToAction(nameof(Index));
                return Redirect("/");
            }

            var eventParticipant = _context.EventParticipants
                 .Where(x => x.IsActive && !x.IsDeleted && x.ParticipantId == member.Id/* && x.EdrEventId == item.Id*/)
                 .ToList();

            if (eventParticipant != null)
            {
                foreach(var item in eventParticipant)
                {
                    item.HasSeenEvent = true;
                    _context.Update(item);
                    _context.SaveChanges();
                }
            }
            return View();
        }
        //public async Task<IActionResult> Index()
        //{
            //var edrImsProjectContext = _context.EdrEvents.Include(e => e.EventAttendanceMode).Include(e => e.EventType).Include(e => e.EventWeight);
            //return View(await edrImsProjectContext.ToListAsync());
        //}

        // GET: EdrEvents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.EdrEvents == null)
            {
                return NotFound();
            }

            var edrEvent = await _context.EdrEvents
                .Include(e => e.EventAttendanceMode)
                .Include(e => e.EventType)
                .Include(e => e.EventWeight)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (edrEvent == null)
            {
                return NotFound();
            }

            return View(edrEvent);
        }

        // GET: EdrEvents/Create
        public IActionResult Create()
        {
            if (User.IsInRole("Edrtegna"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            ViewData["EventAttendanceModeId"] = new SelectList(_context.EventAttendanceModes, "Id", "Name");
            ViewData["EventTypeId"] = new SelectList(_context.EventTypes, "Id", "Name");
            ViewData["EventWeightId"] = new SelectList(_context.EventWeights, "Id", "Name");
            return View();
        }

        // POST: EdrEvents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EventTypeId,Location,EventWeightId,EventAttendanceModeId,DateOfEvent,Description,IsActive")] EdrEvent edrEvent)
        {
            if (User.IsInRole("Edrtegna"))
            {
                TempData["Error"] = "You don't have sufficient previlege to access this resource.";
                return RedirectToAction("Details", "EdrEvents", new { id = edrEvent.Id });
            }

            if (ModelState.IsValid)
            {
                edrEvent.IsPublished = false;
                _context.Add(edrEvent);
                await _context.SaveChangesAsync();
                TempData["Success"] = "New event has been created successfully.";
                TempData["Info"] = "Please add participants to the events, please.";
                return RedirectToAction("Details", "EdrEvents", new { id = edrEvent.Id });
                //return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving edrEvent. Please review your input.";
            ViewData["EventAttendanceModeId"] = new SelectList(_context.EventAttendanceModes, "Id", "Name", edrEvent.EventAttendanceModeId);
            ViewData["EventTypeId"] = new SelectList(_context.EventTypes, "Id", "Name", edrEvent.EventTypeId);
            ViewData["EventWeightId"] = new SelectList(_context.EventWeights, "Id", "Name", edrEvent.EventWeightId);
            return View(edrEvent);
        }

        // GET: EdrEvents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            //if (User.IsInRole("Edrtegna"))
            //{
            //    return RedirectToAction("AccessDenied", "Account");
            //}
            if (id == null || _context.EdrEvents == null)
            {
                return NotFound();
            }

            var edrEvent = await _context.EdrEvents.FindAsync(id);
            if (User.IsInRole("Edrtegna"))
            {
                TempData["Error"] = "You don't have sufficient previlege to access this resource.";
                return RedirectToAction("Details", "EdrEvents", new { id = edrEvent.Id });
            }

            if (edrEvent == null)
            {
                return NotFound();
            }
            ViewData["EventAttendanceModeId"] = new SelectList(_context.EventAttendanceModes, "Id", "Name", edrEvent.EventAttendanceModeId);
            ViewData["EventTypeId"] = new SelectList(_context.EventTypes, "Id", "Name", edrEvent.EventTypeId);
            ViewData["EventWeightId"] = new SelectList(_context.EventWeights, "Id", "Name", edrEvent.EventWeightId);
            return View(edrEvent);
        }

        // POST: EdrEvents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EventTypeId,Location,EventWeightId,EventAttendanceModeId,DateOfEvent,Description,IsActive")] EdrEvent edrEvent)
        {
            if (id != edrEvent.Id)
            {
                return NotFound();
            }
            if (User.IsInRole("Edrtegna"))
            {
                TempData["Error"] = "You don't have sufficient previlege to access this resource.";
                return RedirectToAction("Details", "EdrEvents", new { id = edrEvent.Id });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //edrEvent.IsPublished = false;
                    _context.Update(edrEvent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EdrEventExists(edrEvent.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "edrEvent saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving edrEvent. Please review your input.";
            ViewData["EventAttendanceModeId"] = new SelectList(_context.EventAttendanceModes, "Id", "Name", edrEvent.EventAttendanceModeId);
            ViewData["EventTypeId"] = new SelectList(_context.EventTypes, "Id", "Name", edrEvent.EventTypeId);
            ViewData["EventWeightId"] = new SelectList(_context.EventWeights, "Id", "Name", edrEvent.EventWeightId);
            return View(edrEvent);
        }

        // GET: EdrEvents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            //if (User.IsInRole("Edrtegna"))
            //{
            //    return RedirectToAction("AccessDenied", "Account");
            //}
            if (id == null || _context.EdrEvents == null)
            {
                return NotFound();
            }

            var edrEvent = await _context.EdrEvents
                .Include(e => e.EventAttendanceMode)
                .Include(e => e.EventType)
                .Include(e => e.EventWeight)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (User.IsInRole("Edrtegna"))
            {
                TempData["Error"] = "You don't have sufficient previlege to access this resource.";
                return RedirectToAction("Details", "EdrEvents", new { id = edrEvent.Id });
            }

            if (edrEvent == null)
            {
                return NotFound();
            }

            return View(edrEvent);
        }

        // POST: EdrEvents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //if (User.IsInRole("Edrtegna"))
            //{
            //    return RedirectToAction("AccessDenied", "Account");
            //}
            if (_context.EdrEvents == null)
            {
                return Problem("Entity set 'EdrImsProjectContext.EdrEvents'  is null.");
            }
            var edrEvent = await _context.EdrEvents.FindAsync(id);
            if (User.IsInRole("Edrtegna"))
            {
                TempData["Error"] = "You don't have sufficient previlege to access this resource.";
                return RedirectToAction("Details", "EdrEvents", new { id = edrEvent.Id });
            }

            if (edrEvent != null)
            {
                 edrEvent.IsDeleted = true;
                _context.Update(edrEvent);
                //_context.EdrEvents.Remove(edrEvent);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EdrEventExists(int id)
        {
          return _context.EdrEvents.Any(e => e.Id == id);
        }

        public IActionResult Activate(int id)
        {
            var model = _context.EdrEvents.Find(id);
            if (User.IsInRole("Edrtegna"))
            {
                TempData["Error"] = "You don't have sufficient previlege to access this resource.";
                return RedirectToAction("Details", "EdrEvents", new { id = model.Id });
            }

            model.IsActive = true;
                _context.Update(model);
                _context.SaveChanges();
                TempData["Success"] = "edrEvent activated successfully.";
                return RedirectToAction("Index", "EdrEvents");
        }
        public IActionResult Block(int id)
        {
            var model = _context.EdrEvents.Find(id);
            if (User.IsInRole("Edrtegna"))
            {
                TempData["Error"] = "You don't have sufficient previlege to access this resource.";
                return RedirectToAction("Details", "EdrEvents", new { id = model.Id });
            }

            model.IsActive = false;
                _context.Update(model);
                _context.SaveChanges();
                TempData["Success"] = "edrEvent blocked successfully.";
                return RedirectToAction("Index", "EdrEvents");
        }
        public IActionResult Publish(int id)
        {
            var model = _context.EdrEvents.Find(id);
            if (User.IsInRole("Edrtegna"))
            {
                TempData["Error"] = "You don't have sufficient previlege to access this resource.";
                return RedirectToAction("Details", "EdrEvents", new { id = model.Id });
            }

            var areParticipantAdded = _context.EventParticipants
                    .Include(e => e.Participant)
                    //.ThenInclude(e => e.Users)
                    .Where(x => x.EdrEventId == model.Id && x.IsActive && !x.IsDeleted);

            if (areParticipantAdded.Count() <= 0)
            {
                TempData["Error"] = "The event has not been published successfully, for there is no participant added for the event.";
                return RedirectToAction("Details", "EdrEvents", new { id = model.Id });
            }
            model.IsPublished = true;
            _context.Update(model);
            _context.SaveChanges();
            var sendEmail = new SmsController();
            var eventName = _context.EdrEvents.Include(e => e.EventType)
                .Where(x => x.IsActive == true && x.IsDeleted == false && x.Id == id)
                .FirstOrDefault();

            string eventBody = "New " + eventName.EventType.Name + " event has been created. Please visit the site for detail.";
            foreach(var item in areParticipantAdded.ToList())
            {
                if (item.Participant.PhoneNumber != null)
                {
                    //sendEmail.SendSms(item.Participant.PhoneNumber, "\n\nDear " + item.Participant.FirstName + ",\n" + eventBody );
                }
                
            }
            TempData["Success"] = "The event has been published successfully.";
            //return RedirectToAction("Index", "EdrEvents");
            return RedirectToAction("Details", "EdrEvents", new { id = model.Id });
        }
        public IActionResult Unpublish(int id)
        {
            var model = _context.EdrEvents.Find(id);
            if (User.IsInRole("Edrtegna"))
            {
                TempData["Error"] = "You don't have sufficient previlege to access this resource.";
                return RedirectToAction("Details", "EdrEvents", new { id = model.Id });
            }

            var eventViewer = _context.EventParticipants
                .Where(x => x.IsActive && !x.IsDeleted && x.EdrEventId == model.Id)
                .ToList();
            if (eventViewer != null)
            {
                foreach(var item in eventViewer)
                {
                    item.HasSeenEvent = false;
                    _context.Update(item);
                    _context.SaveChanges();
                }
            }
            model.IsPublished = false;
            _context.Update(model);
            _context.SaveChanges();
            TempData["Success"] = "The event has been unpublished successfully.";
            return RedirectToAction("Details", "EdrEvents", new { id = model.Id });
        }

    }
}
