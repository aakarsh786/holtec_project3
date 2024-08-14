using System;
using System.Collections.Generic;

namespace holtec_project3.Models
{
    public partial class Job
    {
        public Job()
        {
            Applicants = new HashSet<Applicant>();
        }

        public int JobId { get; set; }
        public int? DepartmentId { get; set; }
        public string? JobTitle { get; set; }
        public string? JobDescription { get; set; }
        public string? Salary { get; set; }
        public int? Experience { get; set; }
        public string? Jobtype { get; set; }
        public string? Jobrequirements { get; set; }
        public string? Email { get; set; }
        public DateTime? Deadline { get; set; }
        public string? Location { get; set; }

        public virtual Department? Department { get; set; }
        public virtual ICollection<Applicant> Applicants { get; set; }
    }
}
