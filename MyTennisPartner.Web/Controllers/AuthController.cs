using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyTennisPartner.Data.Managers;
using MyTennisPartner.Data.Utilities;
using MyTennisPartner.Models.Users;
using MyTennisPartner.Web.Exceptions;
using MyTennisPartner.Web.Models;
using MyTennisPartner.Web.Models.AccountViewModels;
using MyTennisPartner.Web.Services;
using MyTennisPartner.Web.Utilities;

namespace MyTennisPartner.Web.Controllers
{
    public class AuthController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        //private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly JwtOptions _jwtOptions;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHostingEnvironment _environment;
        private readonly EmailSenderOptions _emailOptions;
        private readonly MemberManager _memberManager;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            IOptions<JwtOptions> jwtOptions,
            IEmailSender emailSender,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AuthController> logger,
            IHostingEnvironment environment,
            IOptions<EmailSenderOptions> emailOptions, 
            NotificationService notificationService,
            MemberManager memberManager
            ) : base(logger, notificationService)

        {
            _userManager = userManager;
            _jwtOptions = jwtOptions?.Value;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _environment = environment;
            _emailOptions = emailOptions?.Value;
            _memberManager = memberManager;
        }

        /// <summary>
        /// if user is logged in, returns info about user account.  If not, returns 404 not found
        /// </summary>
        /// <returns></returns>
        [HttpGet("~/api/auth/getuserinfo")]
        [Produces("application/json")]
        public async Task<IActionResult> GetUserInfo()
        {
            var name = _userManager.GetUserName(User);
            if (name == null)
            {
                //return ApiNotFound("User not found");
                // we don't want a 404 here, because those show up as "failed requests" in metrics, so just return an empty Ok
                return ApiOk();
            }
            bool isAuthenticated = User.Identity.IsAuthenticated;
            var appUser = await _userManager.FindByNameAsync(name);

            //var isAdmin = User.IsInRole("Admin");  // for some reason, this doesn't work, but the next line does
            var isAdmin = await _userManager.IsInRoleAsync(appUser, "Admin");

            return ApiOk(new
            {
                userName = name,
                isLoggedIn = isAuthenticated,
                firstName = appUser.FirstName,
                lastName = appUser.LastName,
                userId = appUser.Id,
                isAdmin
            });
        }

        [AllowAnonymous]
        [Produces("application/json")]
        [HttpPost("~/api/auth/checkpassword")]
        public async Task<IActionResult> CheckPassword([FromBody]CheckPasswordViewModel model)
        {
            if (model is null)
            {
                return ApiBad("", "The checkPassword viewmodel is null.");
            }

            // Ensure the username and password is valid.
            var user = await _userManager.FindByNameAsync(model.Email);

            // Ensure the email is confirmed.
            //if (!await _userManager.IsEmailConfirmedAsync(user))
            //{
            //    return ApiBad("email_not_confirmed", "You must have a confirmed email to log in.");
            //}

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return ApiBad("", "The username or password is invalid.");
            }
            return ApiOk(model);
        }

        [AllowAnonymous]
        [HttpPost("~/api/auth/login")]
        [Produces("application/json")]
        public async Task<IActionResult> Login([FromBody]LoginViewModel model)
        {
            if (model is null)
            {
                return ApiBad("", "The login viewmodel is null");
            }

            // Ensure the username and password is valid.
            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return ApiBad("","The username or password is invalid");
            }

            // Ensure the email is confirmed.
            //if (!user.EmailConfirmed)
            //{
            //    return ApiBad("email_not_confirmed", "You must have a confirmed email to log in.");
            //}

            Logger.LogInformation($"User logged in (id: {user.Id})");

            var token = AuthenticationHelper.CreateToken(user, _jwtOptions, model.KeepMeLoggedIn);

            return ApiOk(new { token });
        }

        [ApiValidationFilter]
        [HttpPut("~/api/auth/update")]
        public async Task<IActionResult> Update([FromBody]UpdateAccountViewModel model)
        {
            if (model is null)
            {
                return ApiBad("", "The UpdateAccount viewmodel is null");
            }
            var name = _userManager.GetUserName(User);
            var appUser = await _userManager.FindByNameAsync(name);

            if (appUser == null)
            {
                return ApiNotFound($"User not found: {name}");
            }

            var emailHasChanged = appUser.Email != model.Email;
            appUser.Email = model.Email;
            appUser.UserName = model.Email;
            appUser.FirstName = model.FirstName;
            appUser.LastName = model.LastName;

            var result = await _userManager.UpdateAsync(appUser);
            if (result.Succeeded)
            {
                // now update member if email has changed
                if (emailHasChanged)
                {
                    var member = await _memberManager.GetMemberByUserAsync(appUser.Id);
                    if (member != null)
                    {
                        member.Email = appUser.Email;
                        await _memberManager.UpdateMemberAsync(member);
                    }
                }
                // we succeeded
                return ApiOk(appUser);
            }
            // we failed
            return ApiFailed(result.Errors.First().Description);
        }

        [AllowAnonymous]
        [ApiValidationFilter]
        [HttpPost("~/api/auth/register")]
        public async Task<IActionResult> Register([FromBody]NewUser model)
        {
            if (model is null)
            {
                return ApiBad("", "The newUser model is null");
            }
            var user = new ApplicationUser { UserName = model.username, Email = model.username, FirstName = model.firstName, LastName = model.lastName };
            var result = await _userManager.CreateAsync(user, model.password);
            if (result.Succeeded)
            {
                Logger.LogInformation($"New user registered (id: {user.Id})");

                if (!user.EmailConfirmed)
                {
                    await SendEmailConfirmationRequest(user, model.username);
                }
                Logger.LogInformation($"New user registered (id: {user.Id}, username: {model.username})");

                await SendEmailNoticeNewRegistration(user, _emailOptions.SiteAdminEmail);

                return ApiOk(user);
            }
            else
            {
                var message = string.Join(" - ", result.Errors.Select(x => x.Description).ToList());
                return ApiFailed(message);
            }
        }

        [ApiValidationFilter]
        [AllowAnonymous]
        [HttpPost("~/api/auth/sendConfirmEmail")]
        public async Task<IActionResult> SendConfirmEmail([FromBody]ForgotPasswordViewModel model)
        {
            if (model is null)
            {
                return ApiBad("ForgotPasswordViewModel", "cannot be null");
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return ApiBad("email", "unable to find user with this e-mail address");
            }
            await SendEmailConfirmationRequest(user, model.Email);
            return ApiOk(model);
        }

        [NonAction]
        private async Task SendEmailConfirmationRequest(ApplicationUser user, string emailAddress)
        {
            // Send email confirmation email
            var confirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var emailConfirmUrl = Url.RouteUrl("ConfirmEmail", new { uid = user.Id, token = confirmToken }, Request.Scheme);
            await _emailSender.SendEmailAsync(emailAddress, "Please confirm your account",
                $"Please confirm your account by clicking this <a clicktracking=off href=\"{emailConfirmUrl}\">link</a>."
            );

            Logger.LogInformation($"Sent email confirmation email (id: {user.Id})");
        }

        [NonAction]
        private async Task SendEmailNoticeNewRegistration(ApplicationUser user, string emailAddress)
        {
            // Send email notification to site admin that a new user has registered
            await _emailSender.SendEmailAsync(emailAddress, $"New user registered in {_environment.EnvironmentName}",
                $"A new user has registered on the website.  Name = {user.FirstName} {user.LastName}, Email = {user.Email}"
            );

            Logger.LogInformation($"Sent new user registered email (user id: {user.Id})");
        }

        [AllowAnonymous]
        [HttpGet("~/api/auth/confirm", Name = "ConfirmEmail")]
        public async Task<IActionResult> Confirm(string uid, string token)
        {
            var user = await _userManager.FindByIdAsync(uid);
            var confirmResult = await _userManager.ConfirmEmailAsync(user, token);
            if (confirmResult.Succeeded)
            {
                return Redirect("/?confirmed=1");
            }
            else
            {
                return Redirect("/error/email-confirm");
            }
        }

        /// <summary>
        /// method to handle forgotten password.  creates security token and sends e-mail with link to password reset
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ApiValidationFilter]
        [AllowAnonymous]
        [HttpPost("~/api/auth/forgot", Name = "ForgotPassword")]
        public async Task<IActionResult> Forgot([FromBody]ForgotPasswordViewModel model)
        {
            if (model is null)
            {
                return ApiBad("ForgotPasswordViewModel", "cannot be null");
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var emailUrl = $"{Request.Scheme}://{Request.Host}/reset/?uid={user.Id}&token={Uri.EscapeDataString(resetToken)}&email={user.Email}";
                await _emailSender.SendEmailAsync(user.UserName, "Password reset request",
                    $"<p>Please reset your password by clicking on this <a clicktracking=off href=\"{emailUrl}\">link</a> or paste this into your browser:</p><p>{emailUrl}</p>");
                Logger.LogInformation($"Sent password reset email (id: {user.Id})");
                return ApiOk(model);
            }
            return ApiBad("email", "Unable to find user with this e-mail address");
        }

        [ApiValidationFilter]
        [AllowAnonymous]
        [HttpPost("~/api/auth/reset")]
        public async Task<IActionResult> Reset([FromBody]ResetPasswordViewModel model)
        {
            if (model is null) return ApiBad("model", "cannot be null");

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return ApiBad("email", "Unable to find user with this e-mail address");
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return Redirect("/");  // just go to the normal sign in page after we're done resetting password
            }
            var message = result.Errors.FirstOrDefault()?.Description ?? "Unkown error from Auth Controller";

            return ApiFailed(message);
        }

        [HttpPost("~/api/auth/externallogin")]
        [AllowAnonymous]
#pragma warning disable CA1054 // Uri parameters should not be strings
        public IActionResult ExternalLogin(string provider, string returnUrl = "/home", bool keepMeLoggedIn=false)
#pragma warning restore CA1054 // Uri parameters should not be strings
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth", new { returnUrl, keepMeLoggedIn });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet("~/api/auth/externallogincallback")]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null, bool keepMeLoggedIn = false)
        {
            if (remoteError != null)
            {
                return ApiBad("", remoteError);
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return ApiBad("", "Unable to find external login info");
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                Logger.LogInformation($"User {user.UserName} logged in with {info.LoginProvider} provider.");
                var token = AuthenticationHelper.CreateToken(user, _jwtOptions, keepMeLoggedIn);
                var nextUrl = $"/setToken?token={token}&returnUrl={returnUrl}";
                return RedirectToLocal(nextUrl);
            }
            if (result.IsLockedOut)
            {
                return ApiBad("", "User account is locked out");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ViewData["ReturnUrl"] = returnUrl;
                var provider = info.LoginProvider;
                var email = AuthenticationHelper.GetEmailFromPrincipal(info.Principal);
                if (string.IsNullOrEmpty(email))
                {
                    // for cases where external provider does not provide e-mail, like twitter, use name instead
                    email = AuthenticationHelper.GetNameFromPrincipal(info.Principal);
                }
                return RedirectToLocal($"/externalsignin?email={email}&provider={provider}");
            }
        }

        [ApiValidationFilter]
        [HttpPost("~/api/auth/externalloginconfirmation")]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string returnUrl = null)
        {
            if (model is null)
            {
                return ApiBad("model", "cannot be null");
            }

            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return BadRequest(new { general = $"Error loading external login information during confirmation." });
                //throw new ApplicationException("Error loading external login information during confirmation.");
            }
            var firstName = AuthenticationHelper.GetFirstNameFromPrincipal(info.Principal);
            var lastName = AuthenticationHelper.GetLastNameFromPrincipal(info.Principal);
            var newUser = new ApplicationUser { UserName = model.Email, Email = model.Email, FirstName = firstName, LastName = lastName };
            var result = await _userManager.CreateAsync(newUser);

            if (result.Succeeded)
            {
                // we successfully created a new user linked to this external provider.  Now create a login for them and sign them in
                result = await _userManager.AddLoginAsync(newUser, info);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(newUser, isPersistent: false);
                    Logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                    var token = AuthenticationHelper.CreateToken(newUser, _jwtOptions);
                    var nextUrl = $"/setToken?token={token}&returnUrl={returnUrl}";
                    return RedirectToLocal(nextUrl);
                }
            }
            else
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    // This e-mail already exists, so we can link this existing user to this new external provider
                    result = await _userManager.AddLoginAsync(existingUser, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(existingUser, isPersistent: false);
                        var token = AuthenticationHelper.CreateToken(existingUser, _jwtOptions);
                        var nextUrl = $"/setToken?token={token}&returnUrl={returnUrl}";
                        return RedirectToLocal(nextUrl);
                    }
                }
            }

            var errorMessage = result.Errors.FirstOrDefault()?.Description;
            return ApiFailed(errorMessage);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

    }
}