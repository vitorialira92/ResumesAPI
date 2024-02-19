using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace ResumesAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ResumeController : ControllerBase
    {
        private readonly IContentTypeProvider _contentTypeProvider;
        public ResumeController(IContentTypeProvider contentTypeProvider) { 
            _contentTypeProvider = contentTypeProvider;
        }

        [HttpGet]
        public IActionResult GetResume(string fileName)
        {
            var path = $"wwwroot/{fileName}";

            if(!System.IO.File.Exists(path))
            {
                return BadRequest(new
                {
                    Titulo = "Erro ao fazer download do arquivo",
                    Detalhes = "Arquivo não encontrado",
                    StatusCode = 400
                });
            }

            var fileBytes = System.IO.File.ReadAllBytes(path);

            var contentType = "application/octet-stream";

            if(_contentTypeProvider.TryGetContentType(path, out var contentTypeProvider))
            {
                contentType = contentTypeProvider;
            }

            return File(fileBytes, contentType);

        }

        [HttpPost]
        public async Task<IActionResult> PostResume(IFormFile file)
        {
            var path = $"wwwroot/{file.FileName}";

            if(ValidateFile(file) != null)
            {
                return ValidateFile(file);
            }

            using (var fileStream = System.IO.File.Create(path))
            {
                await file.CopyToAsync(fileStream);
            }

            return Ok(new { FileName = file.FileName });
        }

        private IActionResult? ValidateFile(IFormFile file)
        {
            if(file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new
                {
                    Titulo = "Erro ao fazer upload do arquivo",
                    Detalhes = "Arquivo é maior do que 5MB",
                    StatusCode = 400
                });
            }

            string[] possibleExtensions = [".pdf", ".doc", ".docx"];
            var extensao = Path.GetExtension(file.FileName);
            
            if(!possibleExtensions.Contains(extensao))
            {
                return BadRequest(new
                {
                    Titulo = "Erro ao fazer upload do arquivo",
                    Detalhes = "Arquivo é de um formato não suportado",
                    StatusCode = 400
                });
            }

            return null;
        }
    }
}
