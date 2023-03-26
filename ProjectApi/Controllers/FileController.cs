using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectApi.Models;

namespace ProjectApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly ProjectApiContext _dbContext;
        public FileController(ProjectApiContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 2147483648)]
        [RequestSizeLimit(2147483648)]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> Upload([FromForm] FFile file)
        {
            try
            {
                var accessToken = await HttpContext.GetTokenAsync("access_token");
                var valid = ValidateToken.ValidToken(accessToken);
                var acId = valid.Claims.FirstOrDefault(x => x.Type == "Id").Value;
                var folder = await _dbContext.Folders.Where(x => x.Id == file.id).SingleOrDefaultAsync();

                if (folder == null || folder.AccountId != Convert.ToInt32(acId))
                {
                    return Ok(new ApiResponse
                    {
                        Success = false,
                        Message = "Course ko hop le",
                    });
                }

                string FileName = file.fileUpload.FileName;

                // combining GUID to create unique name before saving in wwwroot
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + FileName;

                // getting full path inside wwwroot/images
                string fileUrl = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Files/", uniqueFileName);
                using (FileStream fileStream = System.IO.File.Create(fileUrl))
                {
                    file.fileUpload.CopyTo(fileStream);
                    fileStream.Flush();
                }
                var f = new Models.File();
                f.Name = FileName;    
                f.Url = fileUrl;
                f.CreateAt = DateTime.Now;
                f.FolderId = folder.Id;
                f.AccountId = folder.AccountId;

                await _dbContext.Files.AddAsync(f);
                await _dbContext.SaveChangesAsync();    
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Upload file successfully",
                    Data = new
                    {
                        file = f
                    }
                });   
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Upload file not successfully",
                    Data = ex.ToString()
                }); ;
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> DownLoadFile(int id)
        {
            var file = _dbContext.Files.Where(x=>x.Id == id).SingleOrDefault();
            //var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Files", "Test1.exe");
            var path = file.Url;
            var stream = new FileStream(path, FileMode.Open);
            return File(stream, "application/octet-stream", path);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var valid = ValidateToken.ValidToken(accessToken);
            var acId = valid.Claims.FirstOrDefault(x => x.Type == "Id").Value;
            var file = await _dbContext.Files.Where(x => x.Id == id).SingleOrDefaultAsync();
            if (file == null || file.AccountId != Convert.ToInt32(acId))
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "File ko hop le",
                });
            }
            _dbContext.Files.Remove(file);
            await _dbContext.SaveChangesAsync();
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "",
                Data = new
                {
                    file = file
                }

            });
        }

    }
}
