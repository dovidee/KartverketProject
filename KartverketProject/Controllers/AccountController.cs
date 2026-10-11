using KartverketProject.Dtos;
using KartverketProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

// Site logic for login and register

namespace KartverketProject.Controllers
{
    public class AccountController : Controller
    {
        // registrer identity
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        // registrer db kontekst
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // ULOGGET

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            // hvis bruker gar tilbake
            if(User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("DataForm", "Obstacle");
            }
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest model)
        {
            // lag ny bruker
            var user = new User { 
                UserName = model.UserName, 
                Email = model.Email, 
                Department = "NLA", // settes av server, ikke fra skjema
                LockoutEnabled = true };
            // skap ny bruker og passord
            var result = await _userManager.CreateAsync(user, model.Password);

            // hvis OK, legg bruker til user og logg inn
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "user");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("DataForm", "Obstacle");
            }

            // samme melding uansett, avslorer ikke om brukernavnet finnes
            ModelState.AddModelError(string.Empty,
                "Could not create account. Try a different username, or a password with at least 6 characters, including an uppercase letter, a lowercase letter, a number, and a symbol.");
            // send til view med DTO model.
            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            // hvis bruker gar tilbake
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("DataForm", "Obstacle");
            }
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            // hvis validering feiler
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Please fill out all forms");
                return View(model);
            }
            
            // logg inn med brute force beskyttelse
            var result = await _signInManager.PasswordSignInAsync(
                model.UserName,
                model.Password,
                isPersistent: false,
                lockoutOnFailure: true);

            // finn brukerens navn
            var currentUser = await _userManager.FindByNameAsync(model.UserName);

            // hvis bruker eksisterer
            if (result.Succeeded && currentUser != null)
            {
                // hent opp rapporter som ikke er sitt eller som er tomme i begrunnelse
                var unseenReports = await _context.Report
                    .Where(r => r.UserId == currentUser.Id
                                && !r.ReportReasonSeen
                                && !string.IsNullOrEmpty(r.ReportReason))
                    .ToListAsync();

                // hvis de har blitt sitt
                if (unseenReports.Any())
                {
                    // send til tempdata for forste load
                    TempData["UnseenReportReasons"] = JsonSerializer.Serialize(
                        unseenReports.Select(r => new { r.ReportId, r.ReportReason })
                    );

                    // marker som sitt sann at bruker ikke ser den igjen
                    foreach (var report in unseenReports)
                    {
                        report.ReportReasonSeen = true;
                    }
                    // eksisterende rapporter blir oppdatert og lagret
                    _context.UpdateRange(unseenReports);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction("DataForm", "Obstacle");
            }

            // hvis login feiler
            ModelState.AddModelError("", "Invalid login attempt");
            return View(model);
        }


        // POST: /Account/MarkReportReasonSeen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkReportReasonSeen([FromBody] int reportId)
        {
            // finn rapport id
            var report = await _context.Report.FindAsync(reportId);
            if (report == null) return NotFound();

            // hent bruker id og sjekk om den er lik rapportid
            var currentUserId = _userManager.GetUserId(User);
            if (report.UserId != currentUserId) return Forbid();

            // rapport har blitt sitt
            report.ReportReasonSeen = true;

            // oppdater rapport og lagre
            _context.Update(report);
            await _context.SaveChangesAsync();

            return Ok();
        }


        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        // BRUKER LOGGET INN

        // GET: /Account/Login
        [Authorize(Policy = "AuthenticatedAll")]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        // GET: /Account/Reports
        [Authorize(Policy = "AuthenticatedAll")]
        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound();

            // hent opp rapporter med hindre og bruker der brukerid er lik brukerid
            var reports = await _context.Report
                .Include(r => r.Obstacle)
                .Include(r => r.User)
                .Where(r => r.UserId == user.Id) // sjekk at rapport id stemmer med bruker id
                .ToListAsync();

            return View(reports);
        }

        [Authorize(Policy = "AuthenticatedAll")]
        [HttpGet("EditReport/{id}")]
        public async Task<IActionResult> EditReport(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // hent opp rapporter og hindre
            var report = await _context.Report
                .Include(r => r.User)
                .Include(r => r.Obstacle)
                .FirstOrDefaultAsync(r => r.ReportId == id);

            if (report == null)
                return NotFound();

            // hvis rapport ikke stemmer med brukerid
            if (report.UserId != currentUser?.Id)
                return Forbid();

            return View(report.Obstacle);
        }

        // POST: /Account/EditReport/{id}
        [Authorize(Policy = "AuthenticatedAll")]
        [HttpPost("EditReport/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReport(int id, Obstacle obstacle)
        {
            // hvis validering feiler
            if (!ModelState.IsValid)
                return View(obstacle);

            var currentUser = await _userManager.GetUserAsync(User);

            // hent opp rapport
            var report = await _context.Report
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReportId == id);

            if (report == null)
                return NotFound();

            // sjekk om bruker eier rapport
            if (report.UserId != currentUser?.Id)
                return Forbid();

            // hent opp nested hindre
            var existing = await _context.Obstacle
                .Include(o => o.ReportEntries)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(o => o.ObstacleId == report.ObstacleId);

            // sjekk om hindre eksisterer
            if (existing == null)
                return NotFound();

            // sett gammel data til ny
            existing.ObstacleName = obstacle.ObstacleName;
            existing.ObstacleHeight = obstacle.ObstacleHeight;
            existing.ObstacleDescription = obstacle.ObstacleDescription;
            existing.ObstacleJSON = obstacle.ObstacleJSON;
            existing.ObstacleSubmittedDate = DateTime.UtcNow;

            // lagre oppdatert hindre
             _context.Update(existing);
             await _context.SaveChangesAsync();

             return RedirectToAction("Reports");
        }

        // GET: /Account/DeleteReport/{id}
        [Authorize(Policy = "AuthenticatedAll")]
        [HttpGet("DeleteReport/{id}")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // hent opp rapport
            var report = await _context.Report
                .Include(r => r.User)
                .Include(r => r.Obstacle)
                .FirstOrDefaultAsync(r => r.ReportId == id);

            if (report == null)
                return NotFound();

            // sjekk om bruker eier rapport
            if (report.UserId != currentUser?.Id)
                return Forbid();

            return View(report);
        }

        // POST: /Account/DeleteReportConfirmed
        [Authorize(Policy = "AuthenticatedAll")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReportConfirmed(int reportId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // hent opp bruker sin rapport
            var report = await _context.Report
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReportId == reportId);

            if (report == null) 
                return NotFound();

            // sjekk om bruker eier rapport
            if (report.UserId != currentUser?.Id)
                return Forbid();

            _context.Report.Remove(report);
            await _context.SaveChangesAsync();

            return RedirectToAction("Reports");
        }

        // REVIEWER DEL

        // GET: /Account/AllReports
        [Authorize(Policy = "AuthenticatedHigh")]
        [HttpGet]
        public async Task<IActionResult> AllReports(string statusFilter, string sortOrder)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var userDepartment = currentUser?.Department;
            var userId = currentUser?.Id;

            // hent hindre
            var obstacles = await _context.Obstacle
                .Include(o => o.ReportEntries)
                    .ThenInclude(r => r.User)
                .Include(o => o.ReportEntries)
                    .ThenInclude(r => r.SharedWith)
                        .ThenInclude(rs => rs.SharedWithUser)
                .ToListAsync(); // materialiser i minne

            // filtrer der bruker er lik org eller delt med
            obstacles = obstacles
                .Where(o => o.ReportEntries.Any(r =>
                    (r.User?.Department == userDepartment) ||
                    r.SharedWith.Any(rs => rs.SharedWithUserId == userId)
                ))
                .ToList();

            // map til dto
            var obstacleDtos = obstacles.Select(o =>
            {
                var report = o.ReportEntries.FirstOrDefault();
                return new ObstacleRequest
                {
                    ObstacleId = o.ObstacleId,
                    ObstacleName = o.ObstacleName,
                    ObstacleSubmittedDate = o.ObstacleSubmittedDate,
                    Department = report?.User?.Department ?? "—",
                    UserName = report?.User?.UserName ?? "—",
                    ReportStatus = report?.ReportStatus ?? "Pending"
                };
            });

            // filtrer status
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                obstacleDtos = obstacleDtos.Where(o => o.ReportStatus == statusFilter);
            }

            // sorter
            obstacleDtos = sortOrder switch
            {
                "name_asc" => obstacleDtos.OrderBy(o => o.ObstacleName),
                "name_desc" => obstacleDtos.OrderByDescending(o => o.ObstacleName),
                "department_asc" => obstacleDtos.OrderBy(o => o.Department),
                "department_desc" => obstacleDtos.OrderByDescending(o => o.Department),
                "username_asc" => obstacleDtos.OrderBy(o => o.UserName),
                "username_desc" => obstacleDtos.OrderByDescending(o => o.UserName),
                "date_asc" => obstacleDtos.OrderBy(o => o.ObstacleSubmittedDate),
                "date_desc" => obstacleDtos.OrderByDescending(o => o.ObstacleSubmittedDate),
                "status_asc" => obstacleDtos.OrderBy(o => o.ReportStatus),
                "status_desc" => obstacleDtos.OrderByDescending(o => o.ReportStatus),
                _ => obstacleDtos.OrderByDescending(o => o.ObstacleSubmittedDate),
            };

            // viewdata 1 respons
            ViewData["SelectedStatus"] = statusFilter ?? "All";
            ViewData["SortOrder"] = sortOrder ?? "";

            return View(obstacleDtos);
        }


        //Access Denied page
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // POST: /Account/UpdateStatus
        [Authorize(Policy = "AuthenticatedHigh")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(ReasonRequest model, string newStatus)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Reason is too long";
                return RedirectToAction("AllReports");
            }

            // kun gyldige statuser
            if (newStatus != "Pending" && newStatus != "Approved" && newStatus != "Rejected")
            {
                TempData["Error"] = "Invalid status";
                return RedirectToAction("AllReports");
            }

            var currentUserId = _userManager.GetUserId(User);

            // hent opp bruker sin rapport som er delt med
            var report = await _context.Report
                .Include(r => r.User)
                .Include(r => r.SharedWith)
                    .ThenInclude(rs => rs.SharedWithUser)
                .FirstOrDefaultAsync(r => r.ReportId == model.ReportId);

            if (report == null)
                return NotFound();

            // eier av rapport, delt med eller same org kan kun oppdatere status
            var currentUser = await _userManager.GetUserAsync(User);
            var isOwner = report.UserId == currentUserId;
            var isShared = report.SharedWith.Any(rs => rs.SharedWithUserId == currentUserId);
            var isSameDepartment = report.User?.Department == currentUser?.Department;

            // vis 403
            if (!isOwner && !isShared && !isSameDepartment)
                return Forbid();

            // lagre grunn til rapport
            report.ReportStatus = newStatus;
            report.ReportReason = model.ReportReason;
            _context.Update(report);
            await _context.SaveChangesAsync();

            return RedirectToAction("AllReports");
        }

        // GET: /Account/GetReviewersForSharing
        [Authorize(Policy = "AuthenticatedHigh")]
        [HttpGet]
        public async Task<IActionResult> GetReviewersForSharing(int obstacleId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var currentDepartment = currentUser?.Department;

            // hent ut rapport
            var report = await _context.Report
                .Include(r => r.Obstacle)
                .FirstOrDefaultAsync(r => r.Obstacle != null && r.Obstacle.ObstacleId == obstacleId);

            if (report == null)
                return NotFound();

            // hent alle som IKKE er i org
            var otherUsers = await _context.Users
                .Where(u => u.Department != currentDepartment)
                .ToListAsync();

            var reviewers = new List<object>();

            foreach (var user in otherUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                // vis kun admin eller reviewer, ikke brukere
                if (roles.Contains("reviewer") || roles.Contains("admin")) 
                {
                    reviewers.Add(new { id = user.Id, username = user.UserName });
                }
            }

            return Json(reviewers);
        }

        // POST: /Account/ShareReport
        [Authorize(Policy = "AuthenticatedHigh")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShareReport(int reportId, List<string> selectedUserIds)
        {
            var currentUserId = _userManager.GetUserId(User);

            // hent opp bruker og rapport
            var report = await _context.Report
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReportId == reportId);

            if (report == null) 
                return NotFound();

            // Kun eier eller org kan dele
            var currentUser = await _userManager.GetUserAsync(User);
            var isOwner = report.UserId == currentUserId;
            var isSameDepartment = report.User?.Department == currentUser?.Department;

            // vis 403
            if (!isOwner && !isSameDepartment) 
                return Forbid();

            // hent opp alle valgte brukere fra select i allreports
            foreach (var userId in selectedUserIds)
            {
                var exists = await _context.ReportShare
                    .AnyAsync(rs => rs.ReportId == reportId && rs.SharedWithUserId == userId);
                // sjekk om rapport og delt med eksisterer
                if (!exists)
                {
                    // del rapporten
                    _context.ReportShare.Add(new ReportShare
                    {
                        ReportId = reportId,
                        SharedWithUserId = userId
                    });
                }
            }

            // lagre endringer
            await _context.SaveChangesAsync();
            return RedirectToAction("AllReports");
        }

        // POST: /Account/StopSharing
        [Authorize(Policy = "AuthenticatedHigh")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StopSharing(int reportId)
        {
            var currentUserId = _userManager.GetUserId(User);

            // hent opp rapport
            var report = await _context.Report
                .Include(r => r.User)
                .Include(r => r.SharedWith)
                .FirstOrDefaultAsync(r => r.ReportId == reportId);

            if (report == null)
                return NotFound();

            // kun eier eller og kan dele
            var currentUser = await _userManager.GetUserAsync(User);
            var isOwner = report.UserId == currentUserId;
            var isSameDepartment = report.User?.Department == currentUser?.Department;

            // vis 403
            if (!isOwner && !isSameDepartment) 
                return Forbid();

            // fjern alle delt med
            _context.ReportShare.RemoveRange(report.SharedWith);
            await _context.SaveChangesAsync();

            return RedirectToAction("AllReports");
        }

        // GET: /Account/GetReportDetails
        [Authorize(Policy = "AuthenticatedHigh")]
        [HttpGet]
        public async Task<IActionResult> GetReportDetails(int obstacleId)
        {
            var currentUserId = _userManager.GetUserId(User);

            // hent opp nested hindre og de som er delt med
            var obstacle = await _context.Obstacle
                .Include(o => o.ReportEntries)
                    .ThenInclude(r => r.User)
                .Include(o => o.ReportEntries)
                    .ThenInclude(r => r.SharedWith)
                        .ThenInclude(rs => rs.SharedWithUser)
                .FirstOrDefaultAsync(o => o.ObstacleId == obstacleId);

            if (obstacle == null) return NotFound();

            var report = obstacle.ReportEntries.FirstOrDefault();

            if (report == null) return NotFound();

            // kun bruker, delt med eller org kan hente detaljer
            var currentUser = await _userManager.GetUserAsync(User);
            var isOwner = report.UserId == currentUserId;
            var isShared = report.SharedWith.Any(rs => rs.SharedWithUserId == currentUserId);
            var isSameDepartment = report.User?.Department == currentUser?.Department;

            // vis 403
            if (!isOwner && !isShared && !isSameDepartment) 
                return Forbid();

            // lag en liste av de som er delt med
            var sharedWith = report.SharedWith
                .Where(s => s.SharedWithUser != null)
                .Select(s => s.SharedWithUser?.UserName)
                .ToList();

            // returner i json all data
            return Json(new
            {
                obstacleName = obstacle.ObstacleName,
                department = report.User?.Department ?? "—",
                username = report.User?.UserName ?? "—",
                date = obstacle.ObstacleSubmittedDate.ToString("yyyy-MM-dd"),
                status = report.ReportStatus,
                description = obstacle.ObstacleDescription,
                obstacleJSON = obstacle.ObstacleJSON,
                sharedWith = sharedWith,
                reportReason = report.ReportReason
            });
        }
    }
}