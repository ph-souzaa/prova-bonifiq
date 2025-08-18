using Microsoft.AspNetCore.Mvc;
using ProvaPub.Models;
using ProvaPub.Models.Responses;
using ProvaPub.Repository;
using ProvaPub.Services;
using ProvaPub.Services.Interfaces;

namespace ProvaPub.Controllers
{
	
	/// <summary>
	/// O Código abaixo faz uma chmada para a regra de negócio que valida se um consumidor pode fazer uma compra.
	/// Crie o teste unitário para esse Service. Se necessário, faça as alterações no código para que seja possível realizar os testes.
	/// Tente criar a maior cobertura possível nos testes.
	/// 
	/// Utilize o framework de testes que desejar. 
	/// Crie o teste na pasta "Tests" da solution
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	public class Parte4Controller :  ControllerBase
	{
        ICustomerService _customerService;

        public Parte4Controller(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet("CanPurchase")]
        public async Task<ActionResult<Response<bool>>> CanPurchase(int customerId,decimal purchaseValue)
        {
            try
            {
                var ok = await _customerService.CanPurchase(customerId, purchaseValue);
                return Ok(Response<bool>.Ok(ok, "Validação processada."));
            }
            catch (Exception ex)
            {
                return BadRequest(Response<bool>.Fail(ex.Message));
            }
        }
	}
}
