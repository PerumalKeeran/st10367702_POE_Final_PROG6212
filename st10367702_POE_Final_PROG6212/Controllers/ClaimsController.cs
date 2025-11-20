using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using st10367702_POE_Final_PROG6212.Data;
using st10367702_POE_Final_PROG6212.Models;

namespace st10367702_POE_Final_PROG6212.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClaimsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Lecturer claim form
        [HttpGet]
        public IActionResult LecturerClaim()
        {
            var model = new Claim
            {
                Year = DateTime.Now.Year,
                Month = DateTime.Now.Month
            };

            return View(model);
        }

        // POST: Save lecturer claim + upload optional file
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LecturerClaim(Claim claim, IFormFile? supportingFile)
        {
            if (!ModelState.IsValid)
            {
                return View(claim);
            }

            // Duplicate prevention: same email + month + year
            bool alreadyExists = await _context.Claims.AnyAsync(c =>
                c.LecturerEmail == claim.LecturerEmail &&
                c.Month == claim.Month &&
                c.Year == claim.Year);

            if (alreadyExists)
            {
                ModelState.AddModelError(string.Empty,
                    "You have already submitted a claim for this month and year.");
                return View(claim);
            }

            // Auto calculate total
            claim.TotalAmount = claim.HoursWorked * claim.HourlyRate;
            claim.Status = "Pending";          // overall status
            claim.IsCoordinatorApproved = false;
            claim.IsManagerApproved = false;
            claim.SubmittedDate = DateTime.Now;

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            // Handle file upload (optional)
            if (supportingFile != null && supportingFile.Length > 0)
            {
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
                var extension = Path.GetExtension(supportingFile.FileName).ToLower();

                if (allowedExtensions.Contains(extension))
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await supportingFile.CopyToAsync(stream);
                    }

                    var doc = new SupportingDocument
                    {
                        ClaimId = claim.ClaimId,
                        FileName = supportingFile.FileName,
                        FilePath = "/uploads/" + uniqueFileName,
                        ContentType = supportingFile.ContentType,
                        UploadedOn = DateTime.Now
                    };

                    _context.SupportingDocuments.Add(doc);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    TempData["UploadWarning"] =
                        "File type not allowed. Please upload PDF, Word or Excel documents.";
                }
            }

            TempData["Message"] = "Claim submitted successfully and is pending coordinator approval.";
            return RedirectToAction(nameof(LecturerClaim));
        }

        // 🔹 LECTURER STATUS – track submitted claims by email
        [HttpGet]
        public async Task<IActionResult> LecturerClaimsStatus(string? email)
        {
            List<Claim> claims = new();

            if (!string.IsNullOrWhiteSpace(email))
            {
                claims = await _context.Claims
                    .Where(c => c.LecturerEmail == email)
                    .OrderByDescending(c => c.SubmittedDate)
                    .ToListAsync();

                ViewBag.Email = email;
            }

            return View(claims);
        }

        // 🔹 COORDINATOR DASHBOARD (first approver)
        [HttpGet]
        public async Task<IActionResult> CoordinatorDashboard(string? statusFilter = "All")
        {
            var query = _context.Claims
                .Include(c => c.SupportingDocuments)
                .OrderByDescending(c => c.SubmittedDate)
                .AsQueryable();

            // Coordinator mainly sees claims that are not rejected yet
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                query = query.Where(c => c.Status == statusFilter);
            }

            ViewBag.StatusFilter = statusFilter ?? "All";

            var claims = await query.ToListAsync();
            return View(claims);
        }

        // POST: Coordinator approves
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveCoordinator(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            claim.IsCoordinatorApproved = true;

            // After coordinator approval, send to manager
            claim.Status = "Pending Manager";
            claim.ReviewedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(CoordinatorDashboard));
        }

        // POST: Coordinator rejects
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectCoordinator(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            claim.IsCoordinatorApproved = false;
            claim.Status = "Rejected";
            claim.ReviewedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(CoordinatorDashboard));
        }

        // 🔹 MANAGER DASHBOARD (second approver)
        [HttpGet]
        public async Task<IActionResult> ManagerDashboard(string? statusFilter = "Pending")
        {
            var query = _context.Claims
                .Include(c => c.SupportingDocuments)
                .OrderByDescending(c => c.SubmittedDate)
                .AsQueryable();

            // Manager sees only claims approved by coordinator and not yet decided by manager
            query = query.Where(c =>
                c.IsCoordinatorApproved == true &&
                c.Status != "Rejected");

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                if (statusFilter == "Pending")
                {
                    query = query.Where(c => c.IsManagerApproved == false || c.Status == "Pending Manager");
                }
                else if (statusFilter == "Approved")
                {
                    query = query.Where(c => c.Status == "Approved");
                }
                else if (statusFilter == "Rejected")
                {
                    query = query.Where(c => c.Status == "Rejected");
                }
            }

            ViewBag.StatusFilter = statusFilter ?? "Pending";

            var claims = await query.ToListAsync();
            return View(claims);
        }

        // POST: Manager approves
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveManager(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            // Only approve if coordinator has already approved and not rejected
            if (!claim.IsCoordinatorApproved || claim.Status == "Rejected")
            {
                return RedirectToAction(nameof(ManagerDashboard));
            }

            claim.IsManagerApproved = true;
            claim.Status = "Approved";
            claim.ReviewedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManagerDashboard));
        }

        // POST: Manager rejects
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectManager(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            if (!claim.IsCoordinatorApproved)
            {
                return RedirectToAction(nameof(ManagerDashboard));
            }

            claim.IsManagerApproved = false;
            claim.Status = "Rejected";
            claim.ReviewedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManagerDashboard));
        }

        // 🔹 HR view: only approved claims
        [HttpGet]
        public async Task<IActionResult> HrDashboard()
        {
            var approvedClaims = await _context.Claims
                .Where(c => c.Status == "Approved")
                .OrderByDescending(c => c.SubmittedDate)
                .ToListAsync();

            return View(approvedClaims);
        }

        // HR export: CSV for approved claims
        [HttpGet]
        public async Task<FileResult> ExportApprovedClaimsCsv()
        {
            var approvedClaims = await _context.Claims
                .Where(c => c.Status == "Approved")
                .OrderBy(c => c.LecturerName)
                .ThenBy(c => c.Year)
                .ThenBy(c => c.Month)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Lecturer,Email,Month,Year,HoursWorked,HourlyRate,TotalAmount,SubmittedDate,ReviewedDate");

            foreach (var c in approvedClaims)
            {
                sb.AppendLine($"{c.LecturerName}," +
                              $"{c.LecturerEmail}," +
                              $"{c.Month}," +
                              $"{c.Year}," +
                              $"{c.HoursWorked}," +
                              $"{c.HourlyRate}," +
                              $"{c.TotalAmount}," +
                              $"{c.SubmittedDate:yyyy-MM-dd}," +
                              $"{(c.ReviewedDate.HasValue ? c.ReviewedDate.Value.ToString("yyyy-MM-dd") : "")}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "ApprovedClaims.csv");
        }
    }
}
