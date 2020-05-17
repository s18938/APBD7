using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using AuthenticationSampleWebApp.DTOs;
using AuthenticationSampleWebApp.Models;
using AuthenticationSampleWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationSampleWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
       // public IConfiguration Configuration { get; set; }

        //private readonly StudentDbContext _context;

        //public StudentsController(StudentDbContext context)
        //{
        //    _context = context;
        //}
        //public StudentsController(IConfiguration configuration)
        //{
        //    Configuration = configuration;
        //}
        private readonly StudentDbService _dbService;

        public StudentsController(StudentDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet]
        public IActionResult GetStudents()
        {
            return Ok(_dbService.GetStudents());
        }
       

        [HttpPut("update")]
        public IActionResult updateStudent(Student student)
        {
            try
            {
                _dbService.UpdateStudent(student);
                return Ok("Student został zmodyfikowany");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpDelete("{id}")]
        public IActionResult deleteStudent(int id)
        {
            _dbService.DeleteStudent(id);
            return Ok($"Student {id} usunięty pomyślnie");
        }
        //[HttpGet]
        //[Authorize(Roles ="admin2")]
        //public IActionResult GetStudents()
        //{
        //    var list = new List<Student>();
        //    list.Add(new Student
        //    {
        //        IdStudent=1,
        //        FirstName="Jan",
        //        LastName="Kowalski"
        //    });
        //    list.Add(new Student
        //    {
        //        IdStudent = 2,
        //        FirstName = "Andrzej",
        //        LastName = "Malewski"
        //    });


        //    return Ok(list);
        //}

        //        [HttpPost]
        //        public IActionResult Login(LoginRequestDto request)
        //        {
        //            var claims = new[]
        //{
        //                new Claim(ClaimTypes.NameIdentifier, "1"),
        //                new Claim(ClaimTypes.Name, "jan123"),
        //                new Claim(ClaimTypes.Role, "admin"),
        //                new Claim(ClaimTypes.Role, "student")
        //            };

        //            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
        //            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //            var token = new JwtSecurityToken
        //            (
        //                issuer: "Gakko",
        //                audience: "Students",
        //                claims: claims,
        //                expires: DateTime.Now.AddMinutes(10),
        //                signingCredentials: creds
        //            );

        //            return Ok(new
        //            {
        //                token = new JwtSecurityTokenHandler().WriteToken(token),
        //                refreshToken=Guid.NewGuid()
        //            });
        //        }

    }
}