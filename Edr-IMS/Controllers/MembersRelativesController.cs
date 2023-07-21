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
    public class MembersRelativesController : Controller
    {
        private readonly EdrImsProjectContext _context;
        const int MAX_FILE_SIZE = 2 * 1024 * 1024;

        public MembersRelativesController(EdrImsProjectContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult GetMembersRelatives()
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
                var returnData = (from manudata in _context.MembersRelatives.Where(x => x.IsDeleted == false)
                                .Include(m => m.City)
                                .Include(m => m.Gender)
                                .Include(m => m.Kebele)
                                .Include(m => m.RegisteredByNavigation)
                                .Include(m => m.RelativesRelation)
                                .ThenInclude(m => m.RelationType)
                                .Include(m => m.RelativesRelation)
                                .ThenInclude(m => m.Member)
                                .Include(m => m.SubCity)
                                .Select(x => new
                                {
                                    x.Id,
                                    Member = x.RelativesRelation.Member.FirstName + " " + x.RelativesRelation.Member.MiddleName + " " + x.RelativesRelation.Member.LastName,
                                    Name = x.FirstName + " " + x.MiddleName + " " /*+ " " + x.LastName*/,
                                    Gender = x.Gender.Name,
                                    City = x.City.Name,
                                    SubCity = x.SubCity.Name,
                                    Kebele = x.Kebele.Name,
                                    RelativesRelation = x.RelativesRelation.RelationType.Name,
                                    x.BirthPlace,
                                    x.DateOfBirth,
                                    RegisteredBy = x.RegisteredByNavigation.Name + " " + x.RegisteredByNavigation.FatherName,
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
                    returnData = returnData
                        .Where(m => 
                        m.Name.Contains(searchValue) || 
                        m.Member.Contains(searchValue) ||
                        m.City.Contains(searchValue) ||
                        m.Gender.Contains(searchValue) ||
                        m.RelativesRelation.Contains(searchValue) ||                      
                        m.PhoneNumber.Contains(searchValue)                        
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

        // GET: MembersRelatives


        public IActionResult Index()
        {
            return View();
        }
        //public async Task<IActionResult> Index()
        //{
        //var edrImsProjectContext = _context.MembersRelatives.Include(m => m.City).Include(m => m.Gender).Include(m => m.Kebele).Include(m => m.RegisteredByNavigation).Include(m => m.RelativesRelation).Include(m => m.SubCity);
        //return View(await edrImsProjectContext.ToListAsync());
        //}


        // GET: MembersRelatives/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.MembersRelatives == null)
            {
                return NotFound();
            }

            var membersRelative = await _context.MembersRelatives
                .Include(m => m.City)
                .Include(m => m.Gender)
                .Include(m => m.Kebele)
                .Include(m => m.RegisteredByNavigation)
                .Include(m => m.RelativesRelation)
                .Include(m => m.SubCity)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (membersRelative == null)
            {
                return NotFound();
            }

            return View(membersRelative);
        }

        // GET: MembersRelatives/Create
        public IActionResult Create()
        {
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name");
            ViewData["GenderId"] = new SelectList(_context.Genders, "Id", "Name");
            ViewData["KebeleId"] = new SelectList(_context.Kebeles, "Id", "Name");
            ViewData["RegisteredBy"] = new SelectList(_context.Users.Where(x => x.IsDeleted == false && x.IsActive == true)
                .Select(x => new
                {
                    x.Id,
                    Name = x.Name + " " + x.FatherName,
                })
                , "Id", "Name");
            ViewData["RelativesRelationId"] = new SelectList(_context.RelativesRelations
                .Include(x => x.Member)
                .Include(x => x.RelationType)
                .Where(x => x.IsActive && !x.IsDeleted)
                .Select(x => new
                {
                    x.Id,
                    Name = x.Member.FirstName + " " + x.Member.MiddleName + " (" + x.RelationType.Name + ")",
                })
                , "Id", "Name");
            ViewData["SubCityId"] = new SelectList(_context.SubCities, "Id", "Name");
            return View();
        }

        // POST: MembersRelatives/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RelativesRelationId,FirstName,MiddleName,LastName,GenderId,BirthPlace,DateOfBirth,CityId,SubCityId,KebeleId,PhoneNumber,PhotoUrl,DateOfRegistration,RegisteredBy,IsAlive,IsActive")] MembersRelative membersRelative, IFormFile file)
        {
            if (file == null)
            {
                TempData["Error"] = "Please select a file to upload.";
                //return RedirectToAction(nameof(Index));
                return RedirectToAction("Details", "RelativesRelations", new { id = membersRelative.RelativesRelationId });
            }
            else if (file.Length > MAX_FILE_SIZE)
            {
                ModelState.AddModelError("PhotoUrl", "The file is too large. Maximum allowed size is 2 MB.");
                TempData["Error"] = "The file is too large. Maximum allowed size is 2 MB.";
                //return RedirectToAction(nameof(Index));
                return RedirectToAction("Details", "RelativesRelations", new { id = membersRelative.RelativesRelationId });
            }
            var allowedContentTypes = new[] { "image/jpeg", "image/png" };
            if (!allowedContentTypes.Contains(file.ContentType))
            {
                ModelState.AddModelError("PhotoUrl", "Invalid file type. Only JPEG and PNGsupported.");
                TempData["Error"] = "Invalid file type. Only JPEG, PNG and PDF supported.";
                //return RedirectToAction(nameof(Index));
                return RedirectToAction("Details", "RelativesRelations", new { id = membersRelative.RelativesRelationId });

            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(file.FileName);
            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("PhotoUrl", "Invalid file extension. Only .JPEG, .PNG and .PDF supported.");
                TempData["Error"] = "Invalid file extension. Only .JPEG, and .PNG supported.";
                //return RedirectToAction(nameof(Index));
                return RedirectToAction("Details", "RelativesRelations", new { id = membersRelative.RelativesRelationId });
            }

            if (ModelState.IsValid)
            {
                if (User.Identity.IsAuthenticated)
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
                        return Redirect("/");
                    }
                    // Create a folder named MembersDocuments/MembersPhoto/<RelativesRelationId>/<filename>
                    var folderPath = Path.Combine("MembersDocuments", "MembersRelativesPhoto", membersRelative.RelativesRelationId.ToString());
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    // Move the file uploaded to this path and rename the file with name of the file + timestamp
                    //var fileName = Path.GetFileNameWithoutExtension(file.FileName) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(file.FileName);
                    var membersRelativeName = membersRelative.FirstName + " " + membersRelative.MiddleName + " " + membersRelative.LastName;

                    var fileName = membersRelativeName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(file.FileName);

                    var filePath = Path.Combine(folderPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Add the path to the DB to the row named URL
                    membersRelative.PhotoUrl = filePath;
                    membersRelative.DateOfRegistration = DateTime.Now;
                    membersRelative.RegisteredBy = user.Id;

                    _context.Add(membersRelative);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "membersRelative saved successfully.";
                    //return RedirectToAction(nameof(Index));
                    return RedirectToAction("Details", "RelativesRelations", new { id = membersRelative.RelativesRelationId });
                }
                else
                {
                    return Redirect("/");
                }
            }
            TempData["Error"] = "An error occured while saving membersRelative. Please review your input.";
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", membersRelative.CityId);
            ViewData["GenderId"] = new SelectList(_context.Genders, "Id", "Name", membersRelative.GenderId);
            ViewData["KebeleId"] = new SelectList(_context.Kebeles, "Id", "Name", membersRelative.KebeleId);
            ViewData["RegisteredBy"] = new SelectList(_context.Users.Where(x => x.IsDeleted == false && x.IsActive == true)
                .Select(x => new
                {
                    x.Id,
                    Name = x.Name + " " + x.FatherName,
                })
                , "Id", "Name", membersRelative.RegisteredBy);
            ViewData["RelativesRelationId"] = new SelectList(_context.RelativesRelations.Include(x => x.Member)
                .Include(x => x.RelationType)
                .Where(x => x.IsActive && !x.IsDeleted)
                .Select(x => new
                {
                    x.Id,
                    Name = x.Member.FirstName + " " + x.Member.MiddleName + " (" + x.RelationType.Name + ")",
                })
                , "Id", "Name", membersRelative.RelativesRelationId);
            ViewData["SubCityId"] = new SelectList(_context.SubCities, "Id", "Name", membersRelative.SubCityId);
            return View(membersRelative);
        }

        // GET: MembersRelatives/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.MembersRelatives == null)
            {
                return NotFound();
            }

            var membersRelative = await _context.MembersRelatives.FindAsync(id);
            if (membersRelative == null)
            {
                return NotFound();
            }
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", membersRelative.CityId);
            ViewData["GenderId"] = new SelectList(_context.Genders, "Id", "Name", membersRelative.GenderId);
            ViewData["KebeleId"] = new SelectList(_context.Kebeles, "Id", "Name", membersRelative.KebeleId);
            ViewData["RegisteredBy"] = new SelectList(_context.Users.Where(x => x.IsDeleted == false && x.IsActive == true)
                .Select(x => new
                {
                    x.Id,
                    Name = x.Name + " " + x.FatherName,
                })
                , "Id", "Name", membersRelative.RegisteredBy);
            ViewData["RelativesRelationId"] = new SelectList(_context.RelativesRelations.Include(x => x.Member)
                .Include(x => x.RelationType)
                .Where(x => x.IsActive && !x.IsDeleted)
                .Select(x => new
                {
                    x.Id,
                    Name = x.Member.FirstName + " " + x.Member.MiddleName + " (" + x.RelationType.Name + ")",
                })
                , "Id", "Name", membersRelative.RelativesRelationId);
            ViewData["SubCityId"] = new SelectList(_context.SubCities, "Id", "Name", membersRelative.SubCityId);
            return View(membersRelative);
        }

        // POST: MembersRelatives/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RelativesRelationId,FirstName,MiddleName,LastName,GenderId,BirthPlace,DateOfBirth,CityId,SubCityId,KebeleId,PhoneNumber,PhotoUrl,DateOfRegistration,RegisteredBy,IsAlive,IsActive")] MembersRelative membersRelative, IFormFile file)
        {
            if (file == null)
            {
                TempData["Error"] = "Please select a file to upload.";
                //return RedirectToAction(nameof(Index));
                return RedirectToAction("Details", "RelativesRelations", new { id = membersRelative.RelativesRelationId });
            }
            else if (file.Length > MAX_FILE_SIZE)
            {
                ModelState.AddModelError("PhotoUrl", "The file is too large. Maximum allowed size is 2 MB.");
                TempData["Error"] = "The file is too large. Maximum allowed size is 2 MB.";
                //return RedirectToAction(nameof(Index));
                return RedirectToAction("Details", "RelativesRelations", new { id = membersRelative.RelativesRelationId });
            }
            var allowedContentTypes = new[] { "image/jpeg", "image/png" };
            if (!allowedContentTypes.Contains(file.ContentType))
            {
                ModelState.AddModelError("PhotoUrl", "Invalid file type. Only JPEG and PNGsupported.");
                TempData["Error"] = "Invalid file type. Only JPEG, PNG and PDF supported.";
                //return RedirectToAction(nameof(Index));
                return RedirectToAction("Details", "RelativesRelations", new { id = membersRelative.RelativesRelationId });

            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(file.FileName);
            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("PhotoUrl", "Invalid file extension. Only .JPEG, .PNG and .PDF supported.");
                TempData["Error"] = "Invalid file extension. Only .JPEG, and .PNG supported.";
                //return RedirectToAction(nameof(Index));
                return RedirectToAction("Details", "RelativesRelations", new { id = membersRelative.RelativesRelationId });
            }

            if (id != membersRelative.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (User.Identity.IsAuthenticated)
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
                            return Redirect("/");
                        }
                        // Create a folder named MembersDocuments/MembersPhoto/<RelativesRelationId>/<filename>
                        var folderPath = Path.Combine("MembersDocuments", "MembersRelativesPhoto", membersRelative.RelativesRelationId.ToString());
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        // Move the file uploaded to this path and rename the file with name of the file + timestamp
                        //var fileName = Path.GetFileNameWithoutExtension(file.FileName) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(file.FileName);
                        var membersRelativeName = membersRelative.FirstName + " " + membersRelative.MiddleName + " " + membersRelative.LastName;

                        var fileName = membersRelativeName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(file.FileName);

                        var filePath = Path.Combine(folderPath, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Add the path to the DB to the row named URL
                        membersRelative.PhotoUrl = filePath;
                        membersRelative.DateOfRegistration = DateTime.Now;
                        membersRelative.RegisteredBy = user.Id;

                        _context.Update(membersRelative);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "membersRelative saved successfully.";
                        //return RedirectToAction(nameof(Index));
                        return RedirectToAction("Details", "RelativesRelations", new { id = membersRelative.RelativesRelationId });
                    }
                    else
                    {
                        return Redirect("/");
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MembersRelativeExists(membersRelative.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                //TempData["Success"] = "membersRelative saved successfully.";
                //return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "An error occured while saving membersRelative. Please review your input.";
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", membersRelative.CityId);
            ViewData["GenderId"] = new SelectList(_context.Genders, "Id", "Name", membersRelative.GenderId);
            ViewData["KebeleId"] = new SelectList(_context.Kebeles, "Id", "Name", membersRelative.KebeleId);
            ViewData["RegisteredBy"] = new SelectList(_context.Users.Where(x => x.IsDeleted == false && x.IsActive == true)
                .Select(x => new
                {
                    x.Id,
                    Name = x.Name + " " + x.FatherName,
                })
                , "Id", "Name", membersRelative.RegisteredBy);
            ViewData["RelativesRelationId"] = new SelectList(_context.RelativesRelations.Include(x => x.Member)
                .Include(x => x.RelationType)
                .Where(x => x.IsActive && !x.IsDeleted)
                .Select(x => new
                {
                    x.Id,
                    Name = x.Member.FirstName + " " + x.Member.MiddleName + " (" + x.RelationType.Name + ")",
                })
                , "Id", "Name", membersRelative.RelativesRelationId);
            ViewData["SubCityId"] = new SelectList(_context.SubCities, "Id", "Name", membersRelative.SubCityId);
            return View(membersRelative);
        }

        // GET: MembersRelatives/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.MembersRelatives == null)
            {
                return NotFound();
            }

            var membersRelative = await _context.MembersRelatives
                .Include(m => m.City)
                .Include(m => m.Gender)
                .Include(m => m.Kebele)
                .Include(m => m.RegisteredByNavigation)
                .Include(m => m.RelativesRelation)
                .Include(m => m.SubCity)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (membersRelative == null)
            {
                return NotFound();
            }

            return View(membersRelative);
        }

        // POST: MembersRelatives/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.MembersRelatives == null)
            {
                return Problem("Entity set 'EdrImsProjectContext.MembersRelatives'  is null.");
            }
            var membersRelative = await _context.MembersRelatives.FindAsync(id);
            if (membersRelative != null)
            {
                membersRelative.IsDeleted = true;
                _context.Update(membersRelative);
                //_context.MembersRelatives.Remove(membersRelative);
                TempData["Error"] = "Member's relative has been deleted successfully";

            }

            await _context.SaveChangesAsync();
            //return RedirectToAction(nameof(Index));
            return RedirectToAction("Details", "RelativesRelations", new { id = membersRelative.RelativesRelationId });

        }

        private bool MembersRelativeExists(int id)
        {
            return _context.MembersRelatives.Any(e => e.Id == id);
        }
        public IActionResult Activate(int id)
        {
            var user = _context.MembersRelatives.Find(id);
            var membersRelatives = _context.DeceasedMembersRelatives
                .Where(x => !x.IsDeleted && !x.IsActive && x.MembersRelativeId == user.Id)
                .ToList();

            foreach (var member in membersRelatives)
            {
                if (membersRelatives != null && !member.IsActive)
                {
                    user.IsActive = member.IsActive = true;
                    _context.Update(member);

                }
            }
            user.IsActive = true;
            _context.Update(user);
            _context.SaveChanges();
            TempData["Success"] = "Member's relative activated successfully.";

            //return RedirectToAction("Index", "MembersRelatives");
            return RedirectToAction("Details", "RelativesRelations", new { id = user.RelativesRelationId });

        }
        public IActionResult Block(int id)
        {
            var user = _context.MembersRelatives.Find(id);
            var membersRelatives = _context.DeceasedMembersRelatives
                .Where(x => !x.IsDeleted && x.IsActive && x.MembersRelativeId == user.Id)
                .ToList();

            foreach (var member in membersRelatives)
            {
                if (membersRelatives != null && member.IsActive)
                {
                    user.IsActive = member.IsActive = false;
                    _context.Update(member);

                }
            }
            
            user.IsActive = false;
            _context.Update(user);
            _context.SaveChanges();
            TempData["Success"] = "Member's relative blocked successfully.";
            //return RedirectToAction("Index", "MembersRelatives");
            return RedirectToAction("Details", "RelativesRelations", new { id = user.RelativesRelationId });

        }

        public IActionResult Alive(int id)
        {
            var user = _context.MembersRelatives.Find(id);
            user.IsAlive = true;
            _context.Update(user);
            var deceased = _context.DeceasedMembersRelatives.Where(x => x.IsDeleted == false && x.MembersRelativeId == user.Id).ToList();

            if (deceased != null)
            {
                foreach (var member in deceased)
                {
                    member.IsDeleted = true;
                    _context.Update(member);
                }
            }
            _context.SaveChanges();
            TempData["Success"] = "Member's relative reverted to alive member's list successfully.";

            //return RedirectToAction("Index", "MembersRelatives");
            return RedirectToAction("Details", "RelativesRelations", new { id = user.RelativesRelationId });

        }
        public IActionResult Deceased(int id)
        {
            var user = _context.MembersRelatives.Find(id);
            user.IsAlive = false;
            _context.Update(user);
            var deceased = new DeceasedMembersRelative
            {
                Died = DateTime.Now,
                RestingPlace = "Unknown",
                CauseOfDeath = "Unknown",
                Buried = DateTime.Now.AddDays(1),
                IsInclusive = true,
                MembersRelativeId = user.Id,
                IsActive = user.IsActive
            };
            _context.Add(deceased);
            _context.SaveChanges();
            TempData["Success"] = "You marked " + user.FirstName + " " + user.MiddleName + " " + user.LastName + " as deceased successfully. May their soul rest in peace 🙏🙏";
            TempData["Info"] = user.FirstName + " " + user.MiddleName + " " + user.LastName + " has been automatically included for payment in the death notification." +
                " If you believe that this individual should not be included in this round of payments, please mark them as" +
                " 'Ignored' in the Deceased Relative's Members List.";
            //return RedirectToAction("Index", "DeceasedMembersRelatives");
            return RedirectToAction("Details", "RelativesRelations", new { id = user.RelativesRelationId });

        }
    }
}
