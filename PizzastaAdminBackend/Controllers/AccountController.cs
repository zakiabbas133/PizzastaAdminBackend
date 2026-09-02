using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PizzastaAdminBackend.Data;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PizzastaAdminBackend.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
        }


        // =========================================================
        // LOGIN
        // =========================================================

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid request.",
                    errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value!.Errors
                                .Select(e => e.ErrorMessage)
                                .ToArray()
                        )
                });
            }

            try
            {
                // Find user by username
                var user = await _userManager.FindByNameAsync(
                    request.Username
                );

                if (user == null)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Invalid username or password."
                    });
                }


                // Check password
                var passwordValid = await _userManager.CheckPasswordAsync(
                    user,
                    request.Password
                );

                if (!passwordValid)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Invalid username or password."
                    });
                }


                // Check if account is locked
                if (await _userManager.IsLockedOutAsync(user))
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Account is locked out."
                    });
                }


                // Generate JWT
                var token = await GenerateJwtToken(user);


                return Ok(new
                {
                    success = true,
                    message = "Login successful.",

                    token = token.Token,

                    expiresAt = token.ExpiresAt,

                    user = new
                    {
                        id = user.Id,
                        username = user.UserName,
                        email = user.Email
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while processing the login."
                });
            }
        }


        // =========================================================
        // SIGNUP
        // =========================================================

        [HttpPost]
        public async Task<IActionResult> Signup(SignupRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid request.",
                    errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value!.Errors
                                .Select(e => e.ErrorMessage)
                                .ToArray()
                        )
                });
            }

            try
            {
                var existingUser =
                    await _userManager.FindByNameAsync(request.Username);

                if (existingUser != null)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "Username already exists."
                    });
                }


                var existingEmail =
                    await _userManager.FindByEmailAsync(request.Email);

                if (existingEmail != null)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "Email already exists."
                    });
                }


                var user = new ApplicationUser
                {
                    UserName = request.Username,
                    Email = request.Email,
                    EmailConfirmed = true
                };


                var result = await _userManager.CreateAsync(
                    user,
                    request.Password
                );


                if (!result.Succeeded)
                {
                    var errors = result.Errors
                        .Select(e => e.Description)
                        .ToArray();

                    return BadRequest(new
                    {
                        success = false,
                        message = "Unable to create account.",
                        errors
                    });
                }


                return Ok(new
                {
                    success = true,
                    message = "Account created successfully.",
                    user = new
                    {
                        id = user.Id,
                        username = user.UserName,
                        email = user.Email
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while creating the account."
                });
            }
        }
        
        [Authorize]
        public IActionResult Logout()
        {
            return Ok(new
            {
                success = true,
                message = "Logout successful."
            });
        }


        // =========================================================
        // GENERATE JWT
        // =========================================================

        private async Task<JwtTokenResult> GenerateJwtToken(ApplicationUser user)
        {
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "JWT Key is not configured."
                );

            var jwtIssuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "JWT Issuer is not configured."
                );

            var jwtAudience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "JWT Audience is not configured."
                );

            var expiryMinutes =
                int.TryParse(
                    _configuration["Jwt:ExpiryMinutes"],
                    out var minutes
                )
                    ? minutes
                    : 60;


            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);


            // Claims
            var claims = new List<Claim>
            {
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    user.Id
                ),

                new Claim(
                    JwtRegisteredClaimNames.UniqueName,
                    user.UserName ?? string.Empty
                ),

                new Claim(
                    JwtRegisteredClaimNames.Email,
                    user.Email ?? string.Empty
                ),

                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id
                ),

                new Claim(
                    ClaimTypes.Name,
                    user.UserName ?? string.Empty
                )
            };


            // Add roles to JWT
            foreach (var role in roles)
            {
                claims.Add(
                    new Claim(
                        ClaimTypes.Role,
                        role
                    )
                );
            }


            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            );

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );


            var expiresAt =
                DateTime.UtcNow.AddMinutes(expiryMinutes);


            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );


            return new JwtTokenResult
            {
                Token = new JwtSecurityTokenHandler()
                    .WriteToken(token),

                ExpiresAt = expiresAt
            };
        }
    }


    // =============================================================
    // JWT RESULT
    // =============================================================

    public class JwtTokenResult
    {
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }
    }


    // =============================================================
    // LOGIN REQUEST
    // =============================================================

    public class LoginRequest
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = string.Empty;


        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;


        public bool RememberMe { get; set; }
    }


    // =============================================================
    // SIGNUP REQUEST
    // =============================================================

    public class SignupRequest
    {
        [Required(ErrorMessage = "Username is required.")]
        [MinLength(
            3,
            ErrorMessage = "Username must be at least 3 characters."
        )]
        [MaxLength(
            50,
            ErrorMessage = "Username cannot exceed 50 characters."
        )]
        public string Username { get; set; } = string.Empty;


        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "Password is required.")]
        [MinLength(
            8,
            ErrorMessage = "Password must be at least 8 characters."
        )]
        public string Password { get; set; } = string.Empty;


        [Required(ErrorMessage = "Confirm password is required.")]
        [Compare(
            "Password",
            ErrorMessage = "Password and confirm password do not match."
        )]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}