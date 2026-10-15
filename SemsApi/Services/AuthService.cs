using Azure;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SemsApi.Data;
using SemsApi.DTO.Student;
using SemsApi.DTO.Teacher;
using SemsApi.DTO.User;
using SemsApi.Exceptions;
using SemsApi.Interfaces;
using SemsApi.Models;
using SemsApi.Utilities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace SemsApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        public AuthService(ITokenService tokenService, UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _dbContext = dbContext;
        }
    }
}
