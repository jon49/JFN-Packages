﻿using System;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using JFN.User.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using JFN.UserAuthenticationWeb.Settings;

namespace JFN.UserAuthenticationWeb.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly byte[] _salt;
        private readonly User.User _user;

        public RegisterModel(IOptions<UserSettings> userSettings, User.User user)
        {
            _salt = Encoding.UTF8.GetBytes(userSettings.Value.Salt);
            _user = user ?? throw new ArgumentNullException(nameof(user));
        }

        [BindProperty]
        public UserRegister UserRegister { get; set; }

        public string ErrorMessage { get; set; } = null;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _user.RegisterUser(UserRegister.ToRegisterUser(_salt));
            if (user?.UserId > 0)
            {
                await HttpUtil.Login(user, HttpContext);

                return Redirect("/app");
            }

            ErrorMessage = "Incorrect email or password, try again!";
            return Page();
        }
    }

    public class UserRegister
    {
        [Required, MinLength(3, ErrorMessage = "Must be at least 3 characters long.")]
        public string Email { get; set; }

        [Required, MinLength(5, ErrorMessage = "Password Required")]
        public string Password { get; set; }

        [Required, MinLength(5), Compare(nameof(Password), ErrorMessage = "Password and Confirmation Password must be the same")]
        public string ConfirmationPassword { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public static class UserRegisterExtensions
    {
        public static RegisterUser ToRegisterUser(this UserRegister user, byte[] salt)
            => new(
                Email: user.Email.Trim(),
                EncryptedPassword: SecurePasswordHasher.Hash(user.Password, salt),
                FirstName: user.FirstName?.Trim(),
                LastName: user.LastName?.Trim());
    }
}
