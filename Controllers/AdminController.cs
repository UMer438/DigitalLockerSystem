using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using System;
using DigitalLockerSystem.Models;  

namespace DigitalLockerSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin/Index
        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            var model = users.Select(u => new AdminUserViewModel
            {
                Id = u.Id,
                Email = u.Email,
                Roles = _userManager.GetRolesAsync(u).Result,
                IsBlocked = u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow
            }).ToList();

            return View(model);
        }

        // GET: Admin/EditRoles/userId
        public async Task<IActionResult> EditRoles(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();

            var model = new EditRolesViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                Roles = allRoles.Select(role => new RoleSelection
                {
                    RoleName = role,
                    Selected = userRoles.Contains(role)
                }).ToList()
            };

            return View(model);
        }

        // POST: Admin/EditRoles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoles(EditRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var selectedRoles = model.Roles.Where(r => r.Selected).Select(r => r.RoleName);

            var resultRemove = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            if (!resultRemove.Succeeded)
            {
                ModelState.AddModelError("", "Failed to remove roles");
                return View(model);
            }

            var resultAdd = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
            if (!resultAdd.Succeeded)
            {
                ModelState.AddModelError("", "Failed to add roles");
                return View(model);
            }

            TempData["Message"] = "User roles updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/BlockUser/userId
        public async Task<IActionResult> BlockUser(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var model = new BlockUserViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                LockoutEnd = user.LockoutEnd
            };

            return View(model);
        }

        // POST: Admin/BlockUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockUser(BlockUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound();

            if (model.BlockPermanently)
            {
                user.LockoutEnd = DateTimeOffset.MaxValue;
            }
            else if (model.BlockTemporarilyUntil.HasValue)
            {
                user.LockoutEnd = model.BlockTemporarilyUntil;
            }
            else
            {
                user.LockoutEnd = null; // Unblock user
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["Message"] = "User block status updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Failed to update user block status.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnblockUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return NotFound();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var result = await _userManager.SetLockoutEndDateAsync(user, null);

            if (result.Succeeded)
            {
                TempData["Message"] = "User has been unblocked.";
            }
            else
            {
                TempData["Message"] = "Failed to unblock user.";
            }

            return RedirectToAction(nameof(Index));
        }
    }

    // ViewModels (no changes needed)

    public class AdminUserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public System.Collections.Generic.IList<string> Roles { get; set; }
        public bool IsBlocked { get; set; }
    }

    public class EditRolesViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public System.Collections.Generic.List<RoleSelection> Roles { get; set; }
    }

    public class RoleSelection
    {
        public string RoleName { get; set; }
        public bool Selected { get; set; }
    }

    public class BlockUserViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }

        public DateTimeOffset? LockoutEnd { get; set; }

        public bool BlockPermanently { get; set; }
        public DateTimeOffset? BlockTemporarilyUntil { get; set; }
    }
}
