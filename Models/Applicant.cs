using System;
using System.Collections.Generic;

namespace holtec_project3.Models
{
    public partial class Applicant
    {
        public int ApplicantId { get; set; }
        public int? JobId { get; set; }
        public int? Userid { get; set; }

        public virtual Job? Job { get; set; }
        public virtual Signup? User { get; set; }
    }
}
