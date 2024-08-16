using holtec_project3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using X.PagedList.Extensions;
using holtec_project3.ViewModels;

namespace holtec_project3.Controllers
{
    public class homieController : Controller
    {
        holtec3Context db=new holtec3Context();
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Signup()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Signup(Signup sup)
        {
            db.Signups.Add(sup);
            db.SaveChanges();
            //ViewBag.uid = sup.Userid;
            //ViewBag.Keep();
            TempData["UserId"] = sup.Userid;
            TempData["UserId1"] = sup.Userid;
            return RedirectToAction("Userinfo");
        }

        //Homepage
        public IActionResult Homepage()
        {
            return View();
        }

        //About Us
        public IActionResult AboutUs()
        {
            return View();
        }

        public IActionResult Userinfo()
        {

            return View();
        }

        //Register Wizard--> Userinfo
        [HttpPost]
        public IActionResult Userinfo(Alluserdatum audi, IFormFile Resume, IFormFile Profilepic)
        {
            //audi.Alluserid = 8;
            audi.Userid = (int)TempData["UserId"];
            audi.Alluserid= (int)TempData["UserId"];
            if (Resume != null && Resume.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    Resume.CopyTo(memoryStream);
                    audi.Resume = memoryStream.ToArray();
                }
            }

            if (Profilepic != null && Profilepic.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    Profilepic.CopyTo(memoryStream);
                    audi.Profilepic = memoryStream.ToArray();
                }
            }
            db.Alluserdata.Add(audi);
            db.SaveChanges();
            var user = db.Signups.FirstOrDefault(d => d.Userid == audi.Userid);
            TempData["naam"] = user.Username;
            TempData["email"] = user.Email;
            TempData["fullname"] = $"{audi.Firstname} {audi.Lastname}";
            return RedirectToAction("Homepage");
        }

        [HttpPost]
        public IActionResult Login( string Username, string Password)
        {
            // Look for a user with the provided username and password
            var user = db.Signups.FirstOrDefault(d => d.Username == Username && d.Password == Password);

            if (user != null)
            {
                var userData = db.Alluserdata.FirstOrDefault(d => d.Userid == user.Userid);

                if (userData != null)
                {
                    TempData["fullname"] = $"{userData.Firstname} {userData.Lastname}";
                }

                TempData["naam"]=user.Username;
                TempData["email"]=user.Email;

                


                
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),   
                    new Claim("UserId", user.Userid.ToString()) 
                };

                
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = false, 
                };

                
                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties).GetAwaiter().GetResult();

                //return RedirectToAction("Userprofile");
                return RedirectToAction("Homepage");
            }
            else
            {
             
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View();
            }
        }

        
        public IActionResult Userprofile()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(); // The user is not authenticated
            }

            int userId = int.Parse(userIdClaim);

            
            int jobsAppliedCount = db.Applicants.Count(a => a.Userid == userId);

            
            ViewBag.JobsAppliedCount = jobsAppliedCount;

            var recentJobApplied = (from applicant in db.Applicants
                                    join job in db.Jobs on applicant.JobId equals job.JobId
                                    where applicant.Userid == userId
                                    orderby applicant.ApplicantId descending 
                                    select job).FirstOrDefault();

            if (recentJobApplied != null)
            {
                ViewBag.JobTitle = recentJobApplied.JobTitle;
                
                ViewBag.JobLocation = recentJobApplied.Location;
                ViewBag.JobSalary = recentJobApplied.Salary;
                ViewBag.JobExperience = recentJobApplied.Experience;
                ViewBag.JobType = recentJobApplied.Jobtype;
            }



            ViewBag.Fullname = TempData.Peek("fullname")?.ToString();
            ViewBag.naam = TempData.Peek("naam")?.ToString();
            ViewBag.email = TempData.Peek("email")?.ToString();
            return View();
        }

        //User will see all details
        public IActionResult Userdata() 
        {
            ViewBag.Fullname = TempData.Peek("fullname")?.ToString();
            ViewBag.Email= TempData.Peek("email")?.ToString();
            return View(); 
        }

        //action for all users
        //public IActionResult Usermanagement()
        //{
        //    return View(db.Alluserdata.ToList());
        //}

        public IActionResult Usermanagement(string searchString)
        {
            var users = from u in db.Alluserdata
                        select u;

            // If searchString is not empty, filter the results
            if (!String.IsNullOrEmpty(searchString))
            {
                users = users.Where(s => s.Firstname.Contains(searchString) || s.Lastname.Contains(searchString));
            }

            ViewBag.SearchString = searchString; // Maintain the search string in the view
            return View(users.ToList()); // Return all users if searchString is empty or null
        }


        //Post job ka controller

        public IActionResult postjob()
        {
            return View();
        }

        [HttpPost]
        public IActionResult postjob(Job j1)
        {
            db.Jobs.Add(j1);
            db.SaveChanges();

            return RedirectToAction("Alljobs");
        }

        public IActionResult Alljobs(string jobType, string location, string jobCategory, int? page, string email)
        {
            int pageNumber = (page ?? 1);
            int pageSize = 3;

            // Query the jobs and apply filters
            var jobsQuery = db.Jobs.AsQueryable();

            if (!string.IsNullOrEmpty(jobType))
            {
                jobsQuery = jobsQuery.Where(j => j.Jobtype == jobType);
            }
            if (!string.IsNullOrEmpty(jobCategory))
            {
                jobsQuery = jobsQuery.Where(j => j.JobTitle == jobCategory);
            }
            if (!string.IsNullOrEmpty(location))
            {
                jobsQuery = jobsQuery.Where(j => j.Location == location);
            }

            var pagedJobs = jobsQuery.ToPagedList(pageNumber, pageSize);

            return View(pagedJobs);


        }

        public IActionResult JobDetails(int id)
        {
            var job = db.Jobs.FirstOrDefault(u => u.JobId==id);
            return View(job);
        }

        [HttpPost]
        public IActionResult SendApplication(int JobId)
        {
            var job = db.Jobs.FirstOrDefault(j => j.JobId == JobId);
            if (job == null)
            {
                return NotFound();
            }

            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(); // The user is not authenticated
            }

            int userId = int.Parse(userIdClaim);

            Applicant newApplicant = new Applicant
            {
                JobId = job.JobId,
                Userid = userId
            };

            // Add the new applicant to the Applicants table
            db.Applicants.Add(newApplicant);
            db.SaveChanges(); // Save changes to the database

            return RedirectToAction("Alljobs");
        }

        public IActionResult ApplicantView(int jobId)
        {
            var applicantsWithJobs = from applicant in db.Applicants
                                     join job in db.Jobs on applicant.JobId equals job.JobId
                                     join user in db.Alluserdata on applicant.Userid equals user.Userid
                                     where job.JobId == jobId
                                     select new ApplicantJobViewModel
                                     {
                                         Applicant = applicant,
                                         FullName = user.Firstname + " " + user.Lastname, 
                                         Location = job.Location,
                                         DepartmentId = job.DepartmentId
                                     };

            return View(applicantsWithJobs.ToList());
        }

        public IActionResult HRCandidateView(int userId)
        {
            var userData = db.Alluserdata.FirstOrDefault(u => u.Userid == userId);
            var signupInfo = db.Signups.FirstOrDefault(s => s.Userid == userId);

            ViewBag.Fullname = $"{userData.Firstname} {userData.Lastname}";
            ViewBag.Email = signupInfo.Email;
            ViewBag.ClassX = userData.ClassX;
            ViewBag.XMarks = userData.XMarks;
            ViewBag.XYear = userData.XYear;
            ViewBag.ClassXii = userData.ClassXii;
            ViewBag.XiiMarks = userData.XiiMarks;
            ViewBag.XiiYear = userData.XiiYear;
            ViewBag.Bachelors = userData.Bachelors;
            ViewBag.BachelorsMarks = userData.BachelorsMarks;
            ViewBag.BachelorsYear = userData.BachelorsYear;
            ViewBag.Masters = userData.Masters;
            ViewBag.MastersMarks = userData.MastersMarks;
            ViewBag.MastersYear = userData.MastersYear;
            ViewBag.Diploma = userData.Diploma;
            ViewBag.DiplomaMarks = userData.DiplomaMarks;
            ViewBag.DiplomaYear = userData.DiplomaYear;
            ViewBag.Company_1 = userData.Company1;
            ViewBag.Role_1 = userData.Role1;
            ViewBag.Years_1 = userData.Years1;
            ViewBag.Company_2 = userData.Company2;
            ViewBag.Role_2 = userData.Role2;
            ViewBag.Years_2 = userData.Years2;
            ViewBag.Company_3 = userData.Company3;
            ViewBag.Role_3 = userData.Role3;
            ViewBag.Years_3 = userData.Years3;
            ViewBag.Company_4 = userData.Company4;
            ViewBag.Role_4 = userData.Role4;
            ViewBag.Years_4 = userData.Years4;
            ViewBag.Company_5 = userData.Company5;
            ViewBag.Role_5 = userData.Role5;
            ViewBag.Years_5 = userData.Years5;


            return View();
        }


        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).GetAwaiter().GetResult();
            return RedirectToAction("Signup");
        }

        

        
    }
}
