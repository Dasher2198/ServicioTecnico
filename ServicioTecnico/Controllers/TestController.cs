using Microsoft.AspNetCore.Mvc;

namespace ServicioTecnico.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public ActionResult<object> Get()
        {
            return Ok(new
            {
                message = "API funcionando correctamente",
                timestamp = DateTime.Now,
                status = "OK"
            });
        }

        [HttpGet("ping")]
        public ActionResult<string> Ping()
        {
            return Ok("pong");
        }
    }
}