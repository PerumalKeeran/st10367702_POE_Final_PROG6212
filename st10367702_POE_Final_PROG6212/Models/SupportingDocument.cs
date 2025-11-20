using System;

namespace st10367702_POE_Final_PROG6212.Models
{
    public class SupportingDocument
    {
        public int SupportingDocumentId { get; set; }

        // Foreign key to Claim
        public int ClaimId { get; set; }

        public string FileName { get; set; } = string.Empty;   
        public string FilePath { get; set; } = string.Empty;   
        public string ContentType { get; set; } = string.Empty;
        public DateTime UploadedOn { get; set; } = DateTime.Now;

        public Claim? Claim { get; set; }
    }
}
