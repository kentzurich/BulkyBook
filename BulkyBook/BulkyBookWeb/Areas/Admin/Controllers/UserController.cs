using BulkyBook.DataAccess.Repository.UnitOfWork;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.ROLE_ADMIN)]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(IUnitOfWork unitOfWork, 
                              UserManager<IdentityUser> userManager,
                              RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RoleManagement(string userId)
        {
            RoleManagementViewModel RoleVM = new()
            {
                ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == userId, includeProperties: "Company"),
                RoleList = _roleManager.Roles.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Name
                }),
                CompanyList = _unitOfWork.Company.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };
            RoleVM.ApplicationUser.Role = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser
                    .GetFirstOrDefault(x => x.Id == userId))
                    .GetAwaiter()
                    .GetResult()
                    .FirstOrDefault();
            return View(RoleVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RoleManagement(RoleManagementViewModel roleVM)
        {
            string oldRole = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser
                    .GetFirstOrDefault(x => x.Id == roleVM.ApplicationUser.Id))
                    .GetAwaiter()
                    .GetResult()
                    .FirstOrDefault();

            ApplicationUserModel applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == roleVM.ApplicationUser.Id);

            if (!roleVM.ApplicationUser.Role.Equals(oldRole))
            {
                //a role was updated
                if (roleVM.ApplicationUser.Role.Equals(StaticDetails.ROLE_USER_COMPANY))
                    applicationUser.CompanyId = roleVM.ApplicationUser.CompanyId;
                else
                {
                    if (oldRole.Equals(StaticDetails.ROLE_USER_COMPANY))
                        applicationUser.CompanyId = null;
                }
                _unitOfWork.ApplicationUser.Update(applicationUser);
                _unitOfWork.Save();

                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, roleVM.ApplicationUser.Role).GetAwaiter().GetResult();
            }
            else
            {
                if(oldRole.Equals(StaticDetails.ROLE_USER_COMPANY) && !applicationUser.CompanyId.Equals(roleVM.ApplicationUser.CompanyId))
                {
                    applicationUser.CompanyId = roleVM.ApplicationUser.CompanyId;
                    _unitOfWork.ApplicationUser.Update(applicationUser);
                    _unitOfWork.Save();
                }
            }

            TempData["success"] = "Role updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult LockUnlockAcc([FromBody] string id)
        {
            var objFromDb = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id.Equals(id));
            if (objFromDb is null)
                return Json(new { success = false, message = "Error while locking/unlocking." });

            if (objFromDb.LockoutEnd is not null && objFromDb.LockoutEnd > DateTime.Now)
                //user currently lock and we need to unlock them
                objFromDb.LockoutEnd = DateTime.Now;
            else
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);

            _unitOfWork.ApplicationUser.Update(objFromDb);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Operation successful." });
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var userList = _unitOfWork.ApplicationUser.GetAll(includeProperties:"Company").ToList();

            foreach(var user in userList)
            {
                user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();

                if (user.Company is null)
                    user.Company = new() { Name = string.Empty };
            }

            return Json(new { data = userList });
        }
        #endregion
    }
}
