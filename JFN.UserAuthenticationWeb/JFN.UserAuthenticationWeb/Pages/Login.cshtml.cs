using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Options;
using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using JFN.UserAuthenticationWeb.Settings;
using JFN.User.Dto;

#nullable enable

namespace JFN.UserAuthenticationWeb.Pages
{
    public class LoginModel : PageModel
    {
        private readonly byte[] _salt;
        private readonly User.User _user;
        private readonly string? _overrideReturnUrl;

        public LoginModel(IOptions<UserSettings> userSettings, User.User user)
        {
            _salt = Encoding.UTF8.GetBytes(userSettings.Value.Salt);
            _overrideReturnUrl = userSettings.Value.OverrideReturnUrl;
            _user = user ?? throw new ArgumentNullException(nameof(user));
        }

        [BindProperty]
        public UserLogin? UserLogin { get; set; }
        public string ErrorMessage { get; set; } = "";

        public void OnGet(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
        }

        public async Task<IActionResult> OnPost(string? returnUrl = null)
        {
            var user = await ValidateLogin(UserLogin);
            if (user?.UserId > 0)
            {
                await HttpUtil.Login(user, HttpContext);

                if (_overrideReturnUrl is { } && Url.IsLocalUrl(returnUrl) && !returnUrl!.StartsWith("/app"))
                {
                    return Redirect($"{_overrideReturnUrl}?returnUrl={returnUrl}");
                }
                return
                    Url.IsLocalUrl(returnUrl) && returnUrl != "/"
                        ? Redirect(returnUrl)
                    : Redirect("/app");
            }

            ErrorMessage = "Incorrect email or password, try again!";
            return Page();
        }

        public async Task OnGetLogout(string? returnUrl = null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (returnUrl is null)
            {
                HttpContext.Response.Redirect("/login");
            }
            else
            {
                HttpContext.Response.Redirect(returnUrl);
            }
        }

        private Task<LoggedInUser?> ValidateLogin(UserLogin? user)
        {
            if (user is null) return Task.FromResult<LoggedInUser?>(null);
            var hashedUser = user.ToDBUser(_salt);
            return _user.LoginUser(hashedUser);
        }
    }

    public class UserLogin
    {
        [Required, MinLength(3)]
        public string? Email { get; set; }
        [Required, MinLength(5)]
        public string? Password { get; set; }
    }

    public static class LoginUserExtensions
    {
        public static LoginUser ToDBUser(this UserLogin user, byte[] salt)
            => new
            ( Email: user.Email ?? "",
              EncryptedPassword: SecurePasswordHasher.Hash(user.Password, salt) );
    }
}
