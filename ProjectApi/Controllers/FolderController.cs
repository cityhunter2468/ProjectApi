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
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FolderController : Controller
    {
        private readonly ProjectApiContext _dbContext;
        public FolderController(ProjectApiContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("CourseId/{id}")]
        public async Task<IActionResult> GetFolderandFileByCorseId(int id)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var valid = ValidateToken.ValidToken(accessToken);
            var acId = valid.Claims.FirstOrDefault(x => x.Type == "Id").Value;
            var role = valid.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value;
            //IEnumerable<Claim> roleClaims = User.FindAll(ClaimTypes.Role);
            Console.WriteLine(role);
            Console.WriteLine("da chay vao day");
            if (Convert.ToInt32(role) != 1)
            {
                var ac_co = _dbContext.AccountCourses.Where(x=>x.AccountId == Convert.ToInt32(acId) && x.CourseId == id).SingleOrDefault(); 
                if (ac_co == null || ac_co.Status == 0)
                {
                    return Ok(new ApiResponse
                    {
                        Success = false,
                        Message = "No Permission",
                    });
                }
            }
            var folders = _dbContext.Folders.Include(d => d.Files).Where(c => c.CourseId == id).ToList();
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Successfully",
                Data = new
                {
                    folders = folders
                }
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFolderFileId(int id)
        {
            //var accessToken = await HttpContext.GetTokenAsync("access_token");
            //var valid = ValidateToken.ValidToken(accessToken);
            //var acId = valid.Claims.FirstOrDefault(x => x.Type == "Id").Value;
            //var role = valid.Claims.FirstOrDefault(x => x.Type == "Role").Value;
            //if (Convert.ToInt32(role) != 1)
            //{
            //    var ac_co = _dbContext.AccountCourses.Where(x => x.AccountId == Convert.ToInt32(acId) && x.CourseId == id).SingleOrDefault();
            //    if (ac_co == null)
            //    {
            //        return Ok(new ApiResponse
            //        {
            //            Success = false,
            //            Message = "No Permission",
            //        });
            //    }
            //}
            List<Folder> folders = new List<Folder>();
            folders = _dbContext.Folders.Where(c => c.CourseId == id).ToList();

            return Ok(folders);
        }

        [HttpPost("{id}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> AddFolder(int id, FolderApi folder)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var valid = ValidateToken.ValidToken(accessToken);
            var acId = valid.Claims.FirstOrDefault(x => x.Type == "Id").Value;
            var course = await _dbContext.Courses.Where(x => x.Id == id).SingleOrDefaultAsync();
            
            if (course == null || course.AccountId != Convert.ToInt32(acId))
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Course ko hop le",
                }) ;
            }

            var fo = new Folder();
            fo.CourseId = course.Id;
            fo.CreateAt = DateTime.Now;
            fo.AccountId = course.AccountId;
            fo.FolderName = folder.FolderName;

            await _dbContext.Folders.AddAsync(fo);
            await _dbContext.SaveChangesAsync();
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Add folder successfully",
                Data = new
                {
                    folder = fo
                }
            });
        }

        [HttpPut]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> UpdateFolder(Folder data)
        {
         
            var folder = _dbContext.Folders.SingleOrDefault(c => c.Id == data.Id);

            if (folder == null)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Not Found",
                });
            }

            folder.FolderName = data.FolderName;
            _dbContext.SaveChanges();
            return Ok(new ApiResponse
            {
                Success = false,
                Message = "Update successfully",
            }); 
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> DeleteFolder(int id)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var valid = ValidateToken.ValidToken(accessToken);
            var acId = valid.Claims.FirstOrDefault(x => x.Type == "Id").Value;
            var folder = _dbContext.Folders.SingleOrDefault(c => c.Id == id);

            if (folder == null || folder.AccountId != Convert.ToInt32(acId))
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Not permistion file",
                });
            }

            try
            {
                _dbContext.Folders.Remove(folder);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "File exist folder",
                });
            }
            
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Delete Successfully",
            });
        }
    }
}
