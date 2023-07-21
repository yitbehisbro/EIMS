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
    public class MembersController : Controller
    {
        private readonly EdrImsProjectContext _context;

        public MembersController(EdrImsProjectContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult GetMembers ()
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
                var returnData = (from manudata in _context.Members.Where(x=>x.IsDeleted==false)
                                  .Include(m => m.City)
                                  .Include(m => m.Gender)
                                  .Include(m => m.Kebele)
                                  .Include(m => m.RegisteredByNavigation)
                                  .Include(m => m.SubCity)
                                  .Include(m => m.Users)
                                  .Select(x => new
                                  {
                                      x.Id,
                                      x.FileNumber,
                                      Name = x.FirstName + " " + x.MiddleName + " " + x.LastName,
                                      Users = x.Users.Name + " " + x.Users.FatherName,
                                      Gender = x.Gender.Name,
                                      City = x.City.Name,
                                      SubCity = x.SubCity.Name,
                                      Kebele = x.Kebele.Name,
                                      x.BirthPlace,
                                      x.DateOfBirth,
                                      RegisteredBy = x.RegisteredByNavigation.Name,
                                      x.PhoneNumber,
                                      x.PhotoUrl,
                                      x.IsAlive,
                                      x.DateOfRegistration,
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

        // GET: Members

       
        public IActionResult Index()
        {
            return View();
        }
        //public async Task<IActionResult> Index()
        //{
            //var edrImsProjectContext = _context.Members.Include(m => m.City).Include(m => m.Gender).Include(m => m.Kebele).Include(m => m.RegisteredByNavigation).Include(m => m.SubCity).Include(m => m.Users);
            //return View(await edrImsProjectContext.ToListAsync());
        //}


        // GET: Members/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Members == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .Include(m => m.City)
                .Include(m => m.Gender)
                .Include(m => m.Kebele)
                .Include(m => m.RegisteredByNavigation)
                .Include(m => m.SubCity)
                .Include(m => m.Users)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // GET: Members/Create
        public IActionResult Create()
        {
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name");
            ViewData["GenderId"] = new SelectList(_context.Genders, "Id", "Name");
            ViewData["KebeleId"] = new SelectList(_context.Kebeles, "Id", "Name");
            ViewData["RegisteredBy"] = new SelectList(_context.Users, "Id", "FatherName");
            ViewData["SubCityId"] = new SelectList(_context.SubCities, "Id", "Name");
            ViewData["UsersId"] = new SelectList(_context.Users
                .Where(x => x.IsDeleted == false && x.IsActive == true)
                .Select(x => new
                {
                    x.Id,
                    Name = x.Name + " " + x.FatherName
                })
                , "Id", "Name");
            return View();
        }

        // POST: Members/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UsersId,FileNumber,FirstName,MiddleName,LastName,GenderId,BirthPlace,DateOfBirth,CityId,SubCityId,KebeleId,PhoneNumber,PhotoUrl,DateOfRegistration,RegisteredBy,IsAlive,IsActive")] Member member)
        {
            if (ModelState.IsValid)
            {
                _context.Add(member);
                await _context.SaveChangesAsync();
                TempData["Success"] = "member saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving member. Please review your input.";
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", member.CityId);
            ViewData["GenderId"] = new SelectList(_context.Genders, "Id", "Name", member.GenderId);
            ViewData["KebeleId"] = new SelectList(_context.Kebeles, "Id", "Name", member.KebeleId);
            ViewData["RegisteredBy"] = new SelectList(_context.Users, "Id", "FatherName", member.RegisteredBy);
            ViewData["SubCityId"] = new SelectList(_context.SubCities, "Id", "Name", member.SubCityId);
            ViewData["UsersId"] = new SelectList(_context.Users.Where(x => x.IsDeleted == false && x.IsActive == true)
                .Select(x => new
                {
                    x.Id,
                    Name = x.Name + " " + x.FatherName
                })
                , "Id", "Name", member.UsersId);
            return View(member);
        }

        // GET: Members/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Members == null)
            {
                return NotFound();
            }

            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", member.CityId);
            ViewData["GenderId"] = new SelectList(_context.Genders, "Id", "Name", member.GenderId);
            ViewData["KebeleId"] = new SelectList(_context.Kebeles, "Id", "Name", member.KebeleId);
            ViewData["RegisteredBy"] = new SelectList(_context.Users, "Id", "FatherName", member.RegisteredBy);
            ViewData["SubCityId"] = new SelectList(_context.SubCities, "Id", "Name", member.SubCityId);
            ViewData["UsersId"] = new SelectList(_context.Users.Where(x => x.IsDeleted == false && x.IsActive == true)
                .Select(x => new
                {
                    x.Id,
                    Name = x.Name + " " + x.FatherName
                })
                , "Id", "Name", member.UsersId);
            return View(member);
        }

        // POST: Members/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UsersId,FileNumber,FirstName,MiddleName,LastName,GenderId,BirthPlace,DateOfBirth,CityId,SubCityId,KebeleId,PhoneNumber,PhotoUrl,DateOfRegistration,RegisteredBy,IsAlive,IsActive")] Member member)
        {
            if (id != member.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(member);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberExists(member.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "member saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving member. Please review your input.";
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", member.CityId);
            ViewData["GenderId"] = new SelectList(_context.Genders, "Id", "Name", member.GenderId);
            ViewData["KebeleId"] = new SelectList(_context.Kebeles, "Id", "Name", member.KebeleId);
            ViewData["RegisteredBy"] = new SelectList(_context.Users, "Id", "FatherName", member.RegisteredBy);
            ViewData["SubCityId"] = new SelectList(_context.SubCities, "Id", "Name", member.SubCityId);
            ViewData["UsersId"] = new SelectList(_context.Users.Where(x => x.IsDeleted == false && x.IsActive == true)
                .Select(x => new
                {
                    x.Id,
                    Name = x.Name + " " + x.FatherName
                })
                , "Id", "Name", member.UsersId);
            return View(member);
        }

        // GET: Members/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Members == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .Include(m => m.City)
                .Include(m => m.Gender)
                .Include(m => m.Kebele)
                .Include(m => m.RegisteredByNavigation)
                .Include(m => m.SubCity)
                .Include(m => m.Users)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // POST: Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Members == null)
            {
                return Problem("Entity set 'EdrImsProjectContext.Members'  is null.");
            }
            var member = await _context.Members.FindAsync(id);
            if (member != null)
            {
                 member.IsDeleted = true;
                _context.Update(member);
                //_context.Members.Remove(member);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MemberExists(int id)
        {
          return _context.Members.Any(e => e.Id == id);
        }

        public IActionResult Activate(int id)
        {
            var user = _context.Members.Find(id);
            var deceasedMembers = _context.DeceasedMembers
                .Where(x => !x.IsDeleted && !x.IsActive && x.MemberId == user.Id)
                .ToList();

            foreach (var member in deceasedMembers)
            {
                if (deceasedMembers != null && !member.IsActive)
                {
                    user.IsActive = member.IsActive = true;
                    _context.Update(member);

                }
            }
            user.IsActive = true;
            _context.Update(user);
            _context.SaveChanges();
            TempData["Success"] = "Member activated successfully.";

            return RedirectToAction("Index", "Members");
        }
        public IActionResult Block(int id)
        {
            var user = _context.Members.Find(id);
            var deceasedMembers = _context.DeceasedMembers
                .Where(x => !x.IsDeleted && x.IsActive && x.MemberId == user.Id)
                .ToList();

            foreach (var member in deceasedMembers)
            {
                if (deceasedMembers != null && member.IsActive)
                {
                    user.IsActive = member.IsActive = false;
                    _context.Update(member);

                }
            }
            user.IsActive = false;
            _context.Update(user);
            _context.SaveChanges();
            TempData["Success"] = "Member blocked successfully.";
            return RedirectToAction("Index", "Members");
        }

        public IActionResult Alive(int id)
        {
            var user = _context.Members.Find(id);
            user.IsAlive = true;
            _context.Update(user);
            var deceased = _context.DeceasedMembers.Where(x => x.IsDeleted == false && x.MemberId == user.Id).ToList();

            if (deceased != null)
            {
                foreach (var member in deceased)
                {
                    member.IsDeleted = true;
                    _context.Update(member);
                }
            }
            _context.SaveChanges();
            TempData["Success"] = "Member reverted to alive member's list successfully.";

            return RedirectToAction("Index", "Members");
        }
        public IActionResult Deceased(int id)
        {
            var user = _context.Members.Find(id);
            user.IsAlive = false;
            _context.Update(user);
            var deceased = new DeceasedMember
            {
                Died = DateTime.Now,
                RestingPlace = "Unknown",
                CauseOfDeath = "Unknown",
                Buried = DateTime.Now.AddDays(1),
                MemberId = user.Id,
                IsActive = user.IsActive
            };
            _context.Add(deceased);
            _context.SaveChanges();
            TempData["Success"] = "You marked " + user.FirstName + " " + user.LastName + " as deceased successfully.";
            return RedirectToAction("Index", "Members");
        }

    }
}
