using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using BookListMVC.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BookListMVC.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _db;

        public UsersController(ApplicationDbContext db)
        {
            _db = db;
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var user = GetUser(username, password);

            if (user == null)
            {
                TempData["Error"] = "Error logging in. Check your credentials";
                return View("Login");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim("First Name", user.FirstName),
                new Claim(ClaimTypes.Name, user.FirstName),
                new Claim("Last Name", user.LastName),
                new Claim(ClaimTypes.Role, user.IsAdmin?"Admin":"User")
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(claimsPrincipal);

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect("/");
        }

        private User GetUser(string username, string password)
        {
            return _db.Users.FromSqlRaw($"SELECT * FROM Users WHERE Username='{username.ToLower()}' AND Password='{password}'").FirstOrDefault();
        }

        private User GetUserSecure(string username, string password)
        {
            return _db.Users.FirstOrDefault(u =>
                u.Username.Equals(username.ToLower()) && u.Password.Equals(password));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }

        [HttpGet]
        [Route("/Account/AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data =  Json(new { data = await _db.Users.ToListAsync() });
            return data;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Error creating user";
                return View("Create");
            }

            if(_db.Users.Any(u => u.Username.ToLower().Equals(user.Username)))
            {
                TempData["Error"] = "Username already exists";
                return View("Create");
            }

            // Create in DB
            user.Username = user.Username.ToLower();
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id)
        {
            var dbUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (dbUser == null)
            {
                return Json(new { success = false, message = "Failed to update user" });
            }

            return View(dbUser);
        }

        [HttpPost]
        public async Task<IActionResult> Update(User user)
        {
            var dbUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            if (dbUser == null)
            {
                return Json(new { success = false, message = "Failed to update user" });
            }

            if (!string.IsNullOrEmpty(user.FirstName))
            {
                dbUser.FirstName = user.FirstName;
            }

            if (!string.IsNullOrEmpty(user.LastName))
            {
                dbUser.LastName = user.LastName;
            }

            if (!string.IsNullOrEmpty(user.Password))
            {
                dbUser.Password = user.Password;
            }

            _db.Users.Update(dbUser);
            await _db.SaveChangesAsync();

            return Redirect("Index");
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return Json(new {success=false, message="Failed to delete user"});
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return Json(new {success = true, message = "Successfully deleted user"});
        }

        public async Task<IActionResult> Display(int id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return Json(new { success = false, message = "Failed to fetch user" });
            }

            return View(user);
        }
    }
}
