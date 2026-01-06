using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Areas.Admin.Models.Views;
using WebBanMayTinh.Authorization;
using WebBanMayTinh.Models;
using WebBanMayTinh.Services;
using WebBanMayTinh.Utils;

namespace WebBanMayTinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [HasPermission(CustomClaimTypes.Permission, Permissions.UserAccess)]
    public class UserController : Controller
    {
        private readonly DataContext _context;
        private IUserService userService;
        private UserManager<AppUser> _userManager;
        private SignInManager<AppUser> signInManager;
        private RoleManager<IdentityRole> roleManager;

        public UserController(
            DataContext context, 
            IUserService userService, 
            UserManager<AppUser> userManager, 
            SignInManager<AppUser> signInManager, 
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            this.userService = userService;
            this._userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
        }

        [HasPermission(CustomClaimTypes.Permission, Permissions.UserRead)]
        public async Task<IActionResult> Index(
            string? role,
            string? keyword,
            int? page = 1,
            int? pageSize = 4)
        {
            var query =
                    from u in _context.Users
                    join ur in _context.UserRoles on u.Id equals ur.UserId into urj
                    from ur in urj.DefaultIfEmpty()
                    join r in _context.Roles on ur.RoleId equals r.Id into rj
                    from r in rj.DefaultIfEmpty()
                    where
                        (string.IsNullOrEmpty(keyword) ||
                            u.UserName.Contains(keyword) ||
                            u.FirstName.Contains(keyword) ||
                            u.LastName.Contains(keyword) ||
                            u.Email.Contains(keyword) ||
                            u.Address.Contains(keyword) ||
                            u.PhoneNumber.Contains(keyword))
                        &&
                        (string.IsNullOrEmpty(role) || r.Name == role)
                    select u;

            var pagedUsers = await query
                .Distinct()
                .OrderBy(u => u.LastName)
                .ToPagedResultAsync(page.Value, pageSize.Value);

            var items = new List<UserVM>();

            ViewBag.Roles = _context.Roles.ToList();
            ViewBag.Role = role ?? string.Empty;
            ViewBag.Keyword = keyword ?? string.Empty;

            foreach (var user in pagedUsers.Items)
            {
                var roles = await _userManager.GetRolesAsync(user);

                items.Add(new UserVM
                {
                    User = user,
                    Roles = roles.ToList()
                });
            }

            var result = new PaginationResult<UserVM>(
                items,
                pagedUsers.TotalItems,
                pagedUsers.CurrentPage,
                pagedUsers.PageSize,
                pagedUsers.TotalPages
            );

            return View(result);
        }

        [HasPermission(CustomClaimTypes.Permission, Permissions.UserRead)]
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);

            var roles = await _userManager.GetRolesAsync(user);

            if (user == null)
            {
                return NotFound();
            }

            return View(new UserDetailVM
            {
                Id = id,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles,
            });
        }

        [HasPermission(CustomClaimTypes.Permission, Permissions.UserCreate)]
        public async Task<IActionResult> Create()
        {
            return View(new UserCreateVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(CustomClaimTypes.Permission, Permissions.UserCreate)]
        public async Task<IActionResult> Create(UserCreateVM userVM)
        {

            if (ModelState.IsValid)
            {
                var user = new AppUser()
                {
                    UserName = userVM.Username,
                    Email = userVM.Email,
                    FirstName = userVM.FirstName ?? "",
                    LastName = userVM.LastName ?? "",
                    PhoneNumber = userVM.Phone,
                    Address = userVM.Address,
                    EmailConfirmed = true
                };

                var ok = await userService.AddUser(user, userVM.Password);
                if (!ok)
                {
                    TempData["Error"] = "Tạo mới người dùng không thành công";
                    return View(userVM);
                }
                TempData["Error"] = null;
                TempData["Success"] = "Tạo mới người dùng thành công";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["Error"] = "Tạo mới người dùng không thành công";
                return View(userVM);
            }
        }

        [HasPermission(CustomClaimTypes.Permission, Permissions.UserUpdate)]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(new UserEditVM()
            {
                Username = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.PhoneNumber,
                Email = user.Email,
                Address = user.Address
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(CustomClaimTypes.Permission, Permissions.UserUpdate)]
        public async Task<IActionResult> Edit(string id, UserEditVM model)
        {
            if (ModelState.IsValid)
            {
                    var currentUser = await _userManager.FindByIdAsync(id);

                    if (currentUser == null)
                    {
                        return NotFound();
                    }

                    currentUser.UserName = model.Username;
                    currentUser.Email = model.Email;
                    currentUser.PhoneNumber = model.Phone;
                    currentUser.FirstName = model.FirstName;
                    currentUser.LastName = model.LastName;
                    currentUser.Address = model.Address;

                    var result = await _userManager.UpdateAsync(currentUser);

                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }

                        ViewBag.Roles = new SelectList(_context.Roles, "Id", "Name");
                        return View(model);
                    }

                    //var currentRoles = await userManager.GetRolesAsync(currentUser);
                    //await userManager.RemoveFromRolesAsync(currentUser, currentRoles);
                    //var newRoles = await roleManager.FindByIdAsync(model.RoleId);
                    //if (newRoles != null)
                    //{
                    //    await userManager.AddToRoleAsync(currentUser, newRoles.Name);
                    //}
                    return RedirectToAction(nameof(Index));

            }
            else
            {
                //ViewBag.Roles = new SelectList(roles, "Id", "Name");
                return View(model);
            }
        }

        [HasPermission(CustomClaimTypes.Permission, Permissions.UserDelete)]
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                //.Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [HasPermission(CustomClaimTypes.Permission, Permissions.UserDelete)]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        [HasPermission(CustomClaimTypes.Permission, Permissions.UserPermission)]
        public async Task<IActionResult> Permission(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var roles = _context.Roles;
            var userRoles = await _userManager.GetRolesAsync(user);

            ViewBag.UserRoles = userRoles;
            ViewBag.Roles = roles;
            ViewBag.User = user;

            var models = new List<PermissionVM>();

            foreach (var role in roles)
            {
                PermissionVM permissionVM = new PermissionVM();
                if (userRoles.Contains(role.Name ?? ""))
                    permissionVM.Checked = true;
                else 
                    permissionVM.Checked = false;
                permissionVM.Role = role;
                models.Add(permissionVM);
            }


            return View(models);
        }

        [HttpPost]
        [HasPermission(CustomClaimTypes.Permission, Permissions.UserPermission)]
        public async Task<IActionResult> Permission(string id, List<PermissionVM> model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);

            foreach (var item in model)
            {
                if (item.Checked && !userRoles.Contains(item.Role.Name))
                {
                    await _userManager.AddToRoleAsync(user, item.Role.Name);
                }
                else if (!item.Checked && userRoles.Contains(item.Role.Name))
                {
                    await _userManager.RemoveFromRoleAsync(user, item.Role.Name);
                }
            }

            return RedirectToAction("Index");
        }
    }
}
