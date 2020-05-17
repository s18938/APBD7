using AuthenticationSampleWebApp.DTOs;
using AuthenticationSampleWebApp.DTOs.Responses;
using AuthenticationSampleWebApp.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace AuthenticationSampleWebApp.Services
{
    public class SqlServerDbService : StudentDbService
    {
       


        public EnrollStudentRes EnrollStudent(EnrollStudentReq request)
        {
            using (var con = new SqlConnection(@"Data Source=DESKTOP-5G2FL6J\SQLEXPRESS;Initial Catalog=APBD_4;Integrated Security=True"))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();
                var tran = con.BeginTransaction();
                com.Transaction = tran;
                try
                {                    
                    com.CommandText = "SELECT IdStudy FROM studies WHERE name=@name";
                    com.Parameters.AddWithValue("name", request.Studies);
                    int idStudies;
                    using (var reader = com.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            tran.Rollback();
                            throw new ArgumentException("Podane studia nie istnieją");

                        }
                        idStudies = (int)reader["IdStudy"];
                    }
                   
                    com.CommandText = "SELECT * FROM ENROLLMENT WHERE IdStudy=@id AND Semester=1 ORDER BY StartDate desc";
                    com.Parameters.AddWithValue("id", idStudies);

                    var response = new EnrollStudentRes();
                    int enrollmentId;
                    response.LastName = request.LastName;
                    var dr = com.ExecuteReader();
                    if (dr.Read())
                    {
                        response.Semester = (int)dr["Semester"];
                        response.StartDate = DateTime.Parse(dr["StartDate"].ToString());
                        enrollmentId = (int)dr["IdEnrollment"];
                        dr.Close();
                        dr.Dispose();
                    }

                    else
                    {
                        dr.Close();
                        dr.Dispose();
                       
                        com.CommandText = "INSERT into Enrollment(IdEnrollment, Semester, StartDate, IdStudy) values ((Select ISNULL(MAX(IdEnrollment),1)+1 From Enrollment),1, SYSDATETIME(),@id)";

                        com.ExecuteNonQuery();


                        response.Semester = 1;
                        com.CommandText = "SELECT SYSDATETIME() \"StartDate\", Max(IdEnrollment) \"IdEnrollment\" From Enrollment";

                        dr = com.ExecuteReader();
                        dr.Read();
                        response.StartDate = DateTime.Parse(dr["StartDate"].ToString());
                        enrollmentId = (int)dr["IdEnrollment"];
                    }


                    
                    com.CommandText = "SELECT 1 From Student where IndexNumber=@IndexNumber ";
                    com.Parameters.AddWithValue("IndexNumber", request.IndexNumber);
                    dr.Close();
                    dr.Dispose();

                    dr = com.ExecuteReader();

                    if (dr.Read())
                    {
                        tran.Rollback();
                        throw new ArgumentException("Student o podanym indeksie już znajduje się w bazie danych");

                    }
                    dr.Close();
                    dr.Dispose();

                    com.CommandText = "INSERT INTO Student(IndexNumber, FirstName, LastName, BirthDate, IdEnrollment) VALUES(@Index, @Fname, @Lname,@Bdate,@IdEnrollment)";
                    com.Parameters.AddWithValue("Index", request.IndexNumber);
                    com.Parameters.AddWithValue("Fname", request.FirstName);
                    com.Parameters.AddWithValue("Lname", request.LastName);
                    com.Parameters.AddWithValue("IdEnrollment", enrollmentId);


                    com.ExecuteNonQuery();

                    tran.Commit();
                   
                    return response;
                }
                catch (SqlException ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        public PromoteStudentRes PromoteStudents(int Semester, string Studies)
        {
            using (var con = new SqlConnection(@"Data Source=DESKTOP-5G2FL6J\SQLEXPRESS;Initial Catalog=APBD_4;Integrated Security=True"))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();

                var Transaction = con.BeginTransaction();
                com.Transaction = Transaction;

                try
                {
                    com.CommandText = "SELECT * FROM Enrollment join Studies on Studies.IdStudy = Enrollment.IdStudy where Name=@Name AND semester=@SemesterPar ";
                    com.Parameters.AddWithValue("Name", Studies);
                    com.Parameters.AddWithValue("SemesterPar", Semester);

                    using (var dr = com.ExecuteReader())
                    {
                        if (!dr.Read())
                            throw new ArgumentException();
                    }

                    com.CommandText = "EXEC promoteStudents @Studies=@Name, @semester=@SemesterPar";
                    com.ExecuteNonQuery();
                    Transaction.Commit();

                    return new PromoteStudentRes
                    {
                        Semester = Semester + 1,
                        Studies = Studies

                    };
                }
                catch (SqlException ex)
                {
                    Transaction.Rollback();
                    throw new ArgumentException(ex.Message);
                }

            }



        }
        public bool CheckPassword(LoginRequestDto request)
        {
            using (var con = new SqlConnection(@"Data Source=DESKTOP-5G2FL6J\SQLEXPRESS;Initial Catalog=APBD_4;Integrated Security=True"))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();

              
                com.CommandText = "SELECT Password,Salt FROM Student WHERE IndexNumber=@number";
                com.Parameters.AddWithValue("number", request.Login);

                using var dr = com.ExecuteReader();

                if (dr.Read())
                {
                    var valueBytes = KeyDerivation.Pbkdf2(
                                password: request.Haslo,
                                salt: Encoding.UTF8.GetBytes(dr["Salt"].ToString()),
                                prf: KeyDerivationPrf.HMACSHA512,
                                iterationCount: 10000,
                                numBytesRequested: 256 / 8);
                    return Convert.ToBase64String(valueBytes) == dr["Password"].ToString();
                    
                }
                return false;

            }
        }

        public Claim[] GetClaims(string IndexNumber)
        {
            using (var con = new SqlConnection(@"Data Source=DESKTOP-5G2FL6J\SQLEXPRESS;Initial Catalog=APBD_4;Integrated Security=True"))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();

                com.CommandText = "select Student.IndexNumber,FirstName,LastName,Role" +
                    " from Student_Roles Join Roles on Student_Roles.IdRole = Roles.IdRole join Student on Student.IndexNumber = Student_Roles.IndexNumber" +
                    " where Student.IndexNumber=@Index;";
                com.Parameters.AddWithValue("Index", IndexNumber);

                var dr = com.ExecuteReader();

                if (dr.Read())
                {
                    
                    var claimList = new List<Claim>();
                    claimList.Add(new Claim(ClaimTypes.NameIdentifier, dr["IndexNumber"].ToString()));
                    claimList.Add(new Claim(ClaimTypes.Name, dr["FirstName"].ToString() + " " + dr["LastName"].ToString())); 
                    claimList.Add(new Claim(ClaimTypes.Role, dr["Role"].ToString()));

                    while (dr.Read())
                    {
                        claimList.Add(new Claim(ClaimTypes.Role, dr["Role"].ToString()));
                    }
                    return claimList.ToArray<Claim>();
                }
                else return null;



            }
        }

        public void SetRefreshToken(string refToken, string IndexNumber)
        {
            using (var con = new SqlConnection(@"Data Source=DESKTOP-5G2FL6J\SQLEXPRESS;Initial Catalog=APBD_4;Integrated Security=True"))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();

                com.CommandText = "UPDATE Student SET RefreshToken=@token, TokenExpirationDate=@expires WHERE IndexNumber=@IndexNumber";
                com.Parameters.AddWithValue("token", refToken);
                com.Parameters.AddWithValue("IndexNumber", IndexNumber);

                var dr = com.ExecuteNonQuery();
            }
        }

        public string CheckRefreshToken(string refToken)
        {
            using (var con = new SqlConnection(@"Data Source=DESKTOP-5G2FL6J\SQLEXPRESS;Initial Catalog=APBD_4;Integrated Security=True"))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();

                com.CommandText = "SELECT IndexNumber FROM STUDENT WHERE RefreshToken=@token AND TokenExpirationDate > @expires";
                com.Parameters.AddWithValue("token", refToken);              

                using var dr = com.ExecuteReader();

                if (dr.Read())
                    return dr["IndexNumber"].ToString();
                else
                    return null;
            }
        }
        public void UpdateStudent(Student UpdateStudent)
        {
            var DbContext = new StudentDbContext();
            var Student = DbContext.Students.Where(st => st.Id == UpdateStudent.Id).FirstOrDefault();
            if (Student == null)
                throw new Exception("Nie ma takiego studenta");
            Student.FirstName = UpdateStudent.FirstName != null ? UpdateStudent.FirstName : Student.FirstName;
            Student.LastName = UpdateStudent.LastName != null ? UpdateStudent.LastName : Student.LastName;
            Student.BirthDate = UpdateStudent.BirthDate.Equals(null) ? UpdateStudent.BirthDate : Student.BirthDate;
            DbContext.SaveChanges();
        }

        public void DeleteStudent(int Id)
        {
            var DbContext = new StudentDbContext();         
            var delStudent = new Student()
            {
                Id = Id
            };         
            DbContext.Remove(delStudent);
            DbContext.SaveChanges();
        }

        public IEnumerable<Student> GetStudents()
        {            
            return new StudentDbContext().Students.ToList();
        }
    }
}