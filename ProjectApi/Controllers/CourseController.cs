using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectApi.Models;
using ProjectApi.Apis;
using System.Security.Claims;

namespace ProjectApi.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ProjectApiContext _dbContext;
        private int numItem = 6;
        public CourseController(ProjectApiContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetCourseById(int id)
        {
            //var accessToken = await HttpContext.GetTokenAsync("access_token");
            //var valid = ValidateToken.ValidToken(accessToken);
            //var acId = valid.Claims.FirstOrDefault(x => x.Type == "Id").Value;
            //var role = valid.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value;
            //Console.WriteLine(role);
            //Console.WriteLine("da chay vao day");
            //if (Convert.ToInt32(role) != 1)
            //{
            //    var ac_co = _dbContext.AccountCourses.Where(x => x.AccountId == Convert.ToInt32(acId) && x.CourseId == id).SingleOrDefault();
            //    if (ac_co == null || ac_co.Status == 0)
            //    {
            //        return Ok(new ApiResponse
            //        {
            //            Success = false,
            //            Message = "No Permission",
            //        });
            //    }
            //}
            var course = _dbContext.Courses.Where(c => c.Id == id).FirstOrDefault();
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Done",
                Data = course
            });
            //return Ok(course);
        }

        [HttpGet("ListStudent/{id}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> ListStudent(int id)
        {
            var listStudent = _dbContext.AccountCourses.Where(c => c.CourseId == id).Include(x=>x.Account).ToList();
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Done",
                Data = listStudent
            });
        }

        [HttpGet("Enroll/{id}")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> EnrollCourse(int id)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var valid = ValidateToken.ValidToken(accessToken);
            var acId = valid.Claims.FirstOrDefault(x => x.Type == "Id").Value;
            var ac_course = _dbContext.AccountCourses.Where(x => x.AccountId == Convert.ToInt32(acId) && x.CourseId == id).FirstOrDefault();
            if (ac_course != null)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Waiting"
                });
            }
            var ac_co = new AccountCourse();
            ac_co.AccountId = Convert.ToInt32(acId);
            ac_co.CourseId = id;

            await _dbContext.AccountCourses.AddAsync(ac_co);
            await _dbContext.SaveChangesAsync();
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Done",
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCourse(int page, string? name)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var valid = ValidateToken.ValidToken(accessToken);
            var acId = valid.Claims.FirstOrDefault(x => x.Type == "Id").Value;
            List<Course> courseList = new List<Course>();
            var total = 0;
            if (name != null)
            {
                courseList = _dbContext.Courses.Include(x=>x.Account).Where(x=>x.CourseName.Contains(name)).Skip(page*numItem).Take(numItem).ToList();
                total = _dbContext.Courses.Where(x => x.CourseName.Contains(name)).Count(); 
            } else
            {
                courseList = _dbContext.Courses.Include(x => x.Account).Skip(page*numItem).Take(numItem).ToList();
                total = _dbContext.Courses.Count();
            }
            List<object> data = new List<object>();
            foreach (var course in courseList)
            {
                var acc_course = _dbContext.AccountCourses.Where(x => x.AccountId == Convert.ToInt32(acId) && x.CourseId == course.Id).ToList();
                data.Add(new {
                    course = course,    
                    acc_course = acc_course
                });
            }

            return Ok(new ApiResponse
            {
                Success = true, 
                Message = "Successfully",
                Data = new
                {
                    courseList = data,
                    total = total,
                    numItem = numItem,
                    page = page
                }
            });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "1")]
        public IActionResult GetCourseAccountId(int id, int page, string? name)
        {
            List<Course> courseList = new List<Course>();
            var total = 0;
            if (name != null)
            {
                courseList = _dbContext.Courses.Where(x=>x.AccountId == id && x.CourseName.Contains(name)).Include(x => x.Account).Skip(page*numItem).Take(numItem).ToList();
                total = _dbContext.Courses.Where(x => x.CourseName.Contains(name)).Count();
            }
            else
            {
                courseList = _dbContext.Courses.Where(x=>x.AccountId == id).Include(x => x.Account).Skip(page*numItem).Take(numItem).ToList();
                total = _dbContext.Courses.Count();
            }

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Successfully",
                Data = new
                {
                    courseList = courseList,
                    total = total,
                    numItem = numItem,
                    page =  page
                }
            });
        }

        [HttpPost]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> AddCourse(CourseApi course)
        {
            Console.WriteLine(course.CourseName);
            try
            {
                var accessToken = await HttpContext.GetTokenAsync("access_token");
                var valid = ValidateToken.ValidToken(accessToken);
                var acId = valid.Claims.FirstOrDefault(x => x.Type == "Id").Value;

                var co = new Course();
                co.CourseName = course.CourseName;
                co.CourseDescription = course.CourseDescription;    
                co.AccountId = Convert.ToInt32(acId);
                co.CreateAt = DateTime.Now; 
                _dbContext.Courses.Add(co);
                _dbContext.SaveChanges();
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Add course successfully",
                });
            } catch(Exception ex)
            {
                return Ok(new ApiResponse
                {
                    Success = false,    
                    Message = ex.Message,   
                });
            }
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> UpdateCourse(int id,CourseApi course)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var valid = ValidateToken.ValidToken(accessToken);
            var co = _dbContext.Courses.SingleOrDefault(c => c.Id == id);

            if (co == null)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Not Found",
                });
            }

            var acId = valid.Claims.FirstOrDefault(x => x.Type == "Id").Value;
            if (co.AccountId != Convert.ToInt32(acId))
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid authorization",
                });
            }
      
            co.CourseName = course.CourseName;
            co.CourseDescription = course.CourseDescription;
            _dbContext.SaveChanges();
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Update successfully",
            }); ;
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var valid = ValidateToken.ValidToken(accessToken);
            var course = _dbContext.Courses.SingleOrDefault(c => c.Id == id);

            if (course == null)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Not Found",
                });
            }

            var acId = valid.Claims.FirstOrDefault(x => x.Type == "Id").Value;
            if (course.AccountId != Convert.ToInt32(acId))
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid authorization",
                });
            }
            try
            {
                _dbContext.Courses.Remove(course);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "File and Foler exist Course",
                });
            }
            
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Delete Successfully",
            });
        }

        [HttpPost("AcceptStudent/{is}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> AcceptStudent(RejectAccept rejectAccept)
        {
            
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var valid = ValidateToken.ValidToken(accessToken);
            var acId = valid.Claims.FirstOrDefault(x => x.Type == "Id").Value;

            var course = _dbContext.Courses.Where(x => x.Id == rejectAccept.courseId).SingleOrDefault();
            if (course == null || course.AccountId != Convert.ToInt32(acId))
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Error to accept",
                });
            }
            foreach(var item in rejectAccept.studentList)
            {
                var ac = _dbContext.AccountCourses.Where(x => x.CourseId == rejectAccept.courseId && x.AccountId == item).SingleOrDefault();
                ac.Status = 1;
                await _dbContext.SaveChangesAsync();
            }
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Accept successfully",
            });
            
        }

        [HttpPost("RejectStudent/{is}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> RejectStudent(RejectAccept rejectAccept)
        {

            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var valid = ValidateToken.ValidToken(accessToken);
            var acId = valid.Claims.FirstOrDefault(x => x.Type == "Id").Value;

            var course = _dbContext.Courses.Where(x => x.Id == rejectAccept.courseId).SingleOrDefault();
            if (course == null || course.AccountId != Convert.ToInt32(acId))
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Reject to accept",
                });
            }
            foreach (var item in rejectAccept.studentList)
            {
                var ac = _dbContext.AccountCourses.Where(x => x.CourseId == rejectAccept.courseId && x.AccountId == item).SingleOrDefault();
                ac.Status = 0;
                await _dbContext.SaveChangesAsync();
            }
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Reject successfully",
            });

        }

    }
}
