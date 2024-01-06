using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using BulkyWeb.DataAcces.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(IUnitOfWork unitOfWork, ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
           return View();
        }

        public IActionResult RoleManagement(string userId)
        {
            string RoleId = _context.UserRoles.FirstOrDefault(x => x.UserId == userId).RoleId;

            RoleManagementVM RoleVM = new RoleManagementVM()
            {
                ApplicationUser = _context.ApplicationUsers.Include(x => x.Company).FirstOrDefault(x => x.Id == userId),
                Roles = _context.Roles.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Name
                }),
                Companies = _context.Companies.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
            };

            RoleVM.ApplicationUser.Role = _context.Roles.FirstOrDefault(x => x.Id == RoleId).Name;           

            return View(RoleVM);
        }

        [HttpPost]
        public IActionResult RoleManagement(RoleManagementVM roleManagementVM)
        {
            string RoleId = _context.UserRoles.FirstOrDefault(x => x.UserId == roleManagementVM.ApplicationUser.Id).RoleId;
            string oldRole = _context.Roles.FirstOrDefault(x => x.Id == RoleId).Name;

            if (!(roleManagementVM.ApplicationUser.Role == oldRole))
            {
                //a role has been updated
                ApplicationUser applicationUser = _context.ApplicationUsers.FirstOrDefault(x => x.Id == roleManagementVM.ApplicationUser.Id);
                if (roleManagementVM.ApplicationUser.Role == SD.Role_Company)
                {
                    applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                }
                if (oldRole == SD.Role_Company)
                {
                    applicationUser.CompanyId = null;
                }
                _context.SaveChanges();

                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, roleManagementVM.ApplicationUser.Role).GetAwaiter().GetResult();
            }
            
            return RedirectToAction(nameof(Index));
        }

        #region Api Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            //if you write it this way, be careful, because ef can exlude any user that does not have a company -> make fk nullable in applicationdbcontext
            List<ApplicationUser> objUserList = _context.ApplicationUsers.Include(x => x.Company).ToList();

            //do this outside the loop^for less queries
            var userRoles = _context.UserRoles.ToList();
            var roles = _context.Roles.ToList();

            foreach (var objUser in objUserList)
            {
                var roleId = userRoles.FirstOrDefault(x => x.UserId == objUser.Id)?.RoleId;
                objUser.Role = roles.FirstOrDefault(x => x.Id == roleId)?.Name;

                if (objUser.Company == null) 
                {
                    objUser.Company = new Company() { Name = ""};
                }
            }

            return Json(new { data = objUserList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id)
        {
            var objDb = _context.ApplicationUsers.FirstOrDefault(x => x.Id == id);
            if (objDb == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }

            if (objDb.LockoutEnd != null && objDb.LockoutEnd > DateTime.Now)
            {
                //locked user -> unlock
                objDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objDb.LockoutEnd = DateTimeOffset.Now.AddYears(1000);
            }
            _context.SaveChanges();

            return Json(new { success = true, message = "Operation successfull" });
        }
        #endregion

    }
}
