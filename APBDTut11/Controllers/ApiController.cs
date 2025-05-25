using APBDTut11.DTOs;
using APBDTut11.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBDTut11.Controllers;

[ApiController]
public class ApiController : ControllerBase
{
    private readonly IApiService _apiService;

    public ApiController(IApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpPost("api/prescriptions")]
    public async Task<IActionResult> AddPrescription(AddPrescriptionDTO prescription)
    {
        try
        {
            await _apiService.AddPrescriptionMedicamentAsync(prescription);
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}