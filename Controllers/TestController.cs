using comercializadora_api.Data;
using Microsoft.AspNetCore.Mvc;

namespace comercializadora_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        public TestController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        [HttpGet]
        public ActionResult ObtennerUsuariosTest() {
            var usuarios = this.dbContext.Usuarios.ToList();
            return Ok(usuarios);
        }
    }
}
