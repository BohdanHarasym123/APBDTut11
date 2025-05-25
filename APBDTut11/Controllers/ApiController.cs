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
    public async Task<IActionResult> AddPrescription([FromBody]AddPrescriptionDTO prescription)
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

    [HttpGet("api/patients/{patientId}")]
    public async Task<IActionResult> GetPatientAsync([FromRoute]int patientId)
    {
        try
        {
            var result = await _apiService.GetPatientAsync(patientId);
            return Ok(result);
        }
        catch (Exception e)
        {
            if(e.Message.Contains("not found")) return NotFound(e.Message);
            return BadRequest(e.Message);
        }
    }
}