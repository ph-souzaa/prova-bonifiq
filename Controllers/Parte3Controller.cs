using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProvaPub.Models;
using ProvaPub.Models.Responses;
using ProvaPub.Repository;
using ProvaPub.Services;
using ProvaPub.Services.Interfaces;

namespace ProvaPub.Controllers
{

    /// <summary>
    /// Esse teste simula um pagamento de uma compra.
    /// O método PayOrder aceita diversas formas de pagamento. Dentro desse método é feita uma estrutura de diversos "if" para cada um deles.
    /// Sabemos, no entanto, que esse formato não é adequado, em especial para futuras inclusões de formas de pagamento.
    /// Como você reestruturaria o método PayOrder para que ele ficasse mais aderente com as boas práticas de arquitetura de sistemas?
    /// 
    /// Outra parte importante é em relação à data (OrderDate) do objeto Order. Ela deve ser salva no banco como UTC mas deve retornar para o cliente no fuso horário do Brasil. 
    /// Demonstre como você faria isso.
    /// </summary>
    [ApiController]
	[Route("[controller]")]
	public class Parte3Controller :  ControllerBase
	{

        private readonly IOrderService _orderService;

        public Parte3Controller(IOrderService orderService)
        {
            _orderService = orderService;
        }


        [HttpGet("orders")]
        public async Task<ActionResult<Response<Order>>> PlaceOrder(string paymentMethod,decimal paymentValue,int customerId)
        {
            var result = await _orderService.PayOrder(paymentMethod, paymentValue, customerId);

            if (!result.Success || result.Data is null)
                return BadRequest(Response<Order>.Fail(result.Message));

            var tz = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            result.Data.OrderDate = TimeZoneInfo.ConvertTimeFromUtc(result.Data.OrderDate, tz);

            return Ok(result);
        }
    }
}
