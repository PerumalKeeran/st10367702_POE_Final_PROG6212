using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace st10367702_POE_Final_PROG6212.Models
{
    public class Claim
    {
        public int ClaimId { get; set; }

        // You can use LecturerId later if needed
        public int LecturerId { get; set; }

        [Required]
        [Display(Name = "Lecturer Name")]
        [StringLength(100)]
        public string LecturerName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Lecturer Email")]
        [EmailAddress]
        public string LecturerEmail { get; set; } = string.Empty;

        [Required]
        [Range(1, 12, ErrorMessage = "Month must be between 1 and 12.")]
        public int Month { get; set; }

        [Required]
        [Range(2020, 2100, ErrorMessage = "Year must be reasonable.")]
        public int Year { get; set; }

        [Required]
        [Range(0.5, 300, ErrorMessage = "Hours worked must be between 0.5 and 300.")]
        [Display(Name = "Hours Worked")]
        public decimal HoursWorked { get; set; }

        [Required]
        [Range(1, 5000, ErrorMessage = "Hourly rate must be greater than zero.")]
        [Display(Name = "Hourly Rate (R)")]
        public decimal HourlyRate { get; set; }

        [Display(Name = "Total Amount (R)")]
        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending";

        // NEW: separate approvals
        [Display(Name = "Coordinator Approved")]
        public bool IsCoordinatorApproved { get; set; }

        [Display(Name = "Manager Approved")]
        public bool IsManagerApproved { get; set; }

        [Display(Name = "Submitted On")]
        public DateTime SubmittedDate { get; set; } = DateTime.Now;

        [Display(Name = "Reviewed On")]
        public DateTime? ReviewedDate { get; set; }

        [Display(Name = "Coordinator / Manager Comments")]
        [StringLength(500)]
        public string? ReviewerComments { get; set; }

        // Navigation for documents
        public ICollection<SupportingDocument> SupportingDocuments { get; set; }
            = new List<SupportingDocument>();
    }
}
