using ChatWithEncryption.Models;
using ChatWithEncryption.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ChatWithEncryption.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthServices _authService;
        private readonly SignInManager<User> _signInManager;

        public AuthController(AuthServices authService, SignInManager<User> signInManager)
        {
            _authService = authService;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(); // Страница регистрации
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO registerDto)
        {
            if (!ModelState.IsValid)
                return View(registerDto); // Если данные некорректны, возвращаем страницу с ошибками

            var result = await _authService.RegisterUserAsync(registerDto);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(registerDto);
            }

            return RedirectToAction("Login"); // Перенаправление на страницу логина
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(); // Страница логина
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return View(loginDto);

            var result = await _authService.LoginAsync(loginDto);

            if (result.Succeeded)  // Проверяем, прошёл ли вход успешно
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Неверные данные для входа.");
                return View(loginDto);
            }
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();  // Выход из системы
            return RedirectToAction("Login", "Auth");  // Перенаправление на страницу входа
        }
    }

}
