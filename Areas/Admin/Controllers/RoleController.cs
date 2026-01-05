using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebBanMayTinh.Areas.Admin.Models.Views;
using WebBanMayTinh.Authorization;
using WebBanMayTinh.Models;

namespace WebBanMayTinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = $"permission:{Permissions.RoleAccess}")]
    public class RoleController : Controller
    {
        private DataContext context;
        private RoleManager<IdentityRole> roleManager;
        public RoleController(DataContext context, RoleManager<IdentityRole> roleManager)
        {
            this.context = context;
            this.roleManager = roleManager;
        }
        // GET: RoleController
        public ActionResult Index()
        {
            var roles = context.Roles.ToList();
            return View(roles);
        }

        // GET: RoleController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: RoleController/Create
        public ActionResult Create()
        {
            var permissions = typeof(Permissions)
                .GetFields()
                .Select(f => new PermissionVM
                {
                    Role = new IdentityRole(f.GetValue(null)!.ToString()!)
                })
                .ToList();

            var vm = new RoleCreateVM
            {
                Permissions = permissions
            };

            return View(vm);
        }

        // POST: RoleController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RoleCreateVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var role = new IdentityRole(model.Name);
            var result = await roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Tạo role thất bại");
                return View(model);
            }

            var selectedPermissions = model.Permissions
                .Where(p => p.Checked)
                .Select(p => p.Role);

            foreach (var permission in selectedPermissions)
            {
                await roleManager.AddClaimAsync(
                    role,
                    new Claim(CustomClaimTypes.Permission.ToString(), permission.ToString())
                );
            }

            return RedirectToAction("Index");
        }

        // GET: RoleController/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();
            var roleClaims = await roleManager.GetClaimsAsync(role);
            var rolePermissions = roleClaims
                .Where(c => c.Type == CustomClaimTypes.Permission)
                .Select(c => c.Value)
                .ToHashSet();
            
            var permissions = Permissions.GetAll() // method tao sẽ cho bên dưới
                .Select(p => new PermissionVM
                {
                    Role = new IdentityRole(p),
                    Checked = rolePermissions.Contains(p)
                })
                .ToList();

            var vm = new RoleUpdateVM
            {
                RoleId = role.Id,
                Name = role.Name!,
                Permissions = permissions
            };

            return View(vm);
        }

        // POST: RoleController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, RoleUpdateVM vm)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            role.Name = vm.Name;
            role.NormalizedName = vm.Name.ToUpper(); 

            var result = await roleManager.UpdateAsync(role);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(role);
            }
            // Xóa các claim cũ
            var oldClaims = await roleManager.GetClaimsAsync(role);
            foreach (var claim in oldClaims.Where(c => c.Type == "permission"))
            {
                await roleManager.RemoveClaimAsync(role, claim);
            }

            var selectedPermissions = vm.Permissions
                .Where(p => p.Checked)
                .Select(p => p.Role);

            // Gán lại quyền
            foreach (var permission in selectedPermissions)
            {
                await roleManager.AddClaimAsync(role,
                    new Claim(CustomClaimTypes.Permission.ToString(), permission.ToString()));
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: RoleController/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        // POST: RoleController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id, IFormCollection collection)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            try
            {
                await roleManager.DeleteAsync(role);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
