using AuthenticationSampleWebApp.DTOs;
using AuthenticationSampleWebApp.DTOs.Responses;
using AuthenticationSampleWebApp.Models;
using System.Collections.Generic;
using System.Security.Claims;

namespace AuthenticationSampleWebApp.Services
{
    public interface StudentDbService
    {
        EnrollStudentRes EnrollStudent(EnrollStudentReq request);
        PromoteStudentRes PromoteStudents(int semester, string studies);

        public bool CheckPassword(LoginRequestDto request);
        public Claim[] GetClaims(string IndexNumber);
        public void SetRefreshToken(string token, string indexNumber);
        public string CheckRefreshToken(string token);
        public IEnumerable<Student> GetStudents();
        public void DeleteStudent(int Id);
        public void UpdateStudent(Student UpdateStudent);
    }
}
