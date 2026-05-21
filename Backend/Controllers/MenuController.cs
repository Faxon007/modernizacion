using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Backend.Repositories;
using Backend.Common;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MenuController : ControllerBase
    {
        private readonly IMenuRepository _menuRepository;
        private readonly ILogger<MenuController> _logger;

        public MenuController(IMenuRepository menuRepository, ILogger<MenuController> logger)
        {
            _menuRepository = menuRepository;
            _logger = logger;
        }

        [HttpGet("items")]
        public async Task<IActionResult> GetMenuItems([FromQuery] string usuario, [FromQuery] string sistema)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(sistema))
                {
                    return BadRequest(ApiResponse<object>.Fail("MENU.INVALID_PARAMS", "El usuario y el sistema son parámetros obligatorios."));
                }

                string cleanedUser = usuario.Trim().ToUpper().Replace("PROMERICA\\", "");

                var items = await _menuRepository.GetMenuItemsAsync(cleanedUser, sistema);
                var menuItems = items.Select(m => new
                {
                    id = m.CodMenuItem,
                    nombre = m.Nombre,
                    path = m.Path,
                    descripcion = m.Descripcion,
                    padreId = m.CodItemPadre,
                    visible = m.Visible == "S" || m.Visible == "1" || m.Visible == "true"
                }).ToList();

                return Ok(ApiResponse<object>.Ok(menuItems));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener items de menú para usuario {Usuario}", usuario);
                return StatusCode(500, ApiResponse<object>.Fail("MENU.INTERNAL_ERROR", "Error interno al recuperar los privilegios del menú."));
            }
        }
    }
}
