using ChatWithEncryption.Data;
using ChatWithEncryption.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ChatWithEncryption.Services
{
    public class AuthServices
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public AuthServices(ApplicationDbContext context,
                            SignInManager<User> signInManager,
                            UserManager<User> userManager)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // Регистрация пользователя
        public async Task<IdentityResult> RegisterUserAsync(RegisterDTO registerDTO)
        {
            var user = new User
            {
                UserName = registerDTO.Username,
            };

            var result = await _userManager.CreateAsync(user, registerDTO.Password);
            return result;
        }

        public async Task<SignInResult> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.UserName);

            if (user == null)
            {
                // Запись в лог, если пользователь не найден
                Console.WriteLine($"User not found: {loginDto.UserName}");
                return SignInResult.Failed;
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, false, false);
            if (!result.Succeeded)
            {
                // Запись ошибок для дальнейшего анализа
                Console.WriteLine("Login failed.");
            }

            return result;
        }

        // Логика для выхода из системы
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }

}
