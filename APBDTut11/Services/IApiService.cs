using APBDTut11.DTOs;

namespace APBDTut11.Services;

public interface IApiService
{
    Task AddPrescriptionMedicamentAsync(AddPrescriptionDTO request);
    
    Task<GetPatientDTO> GetPatientAsync(int patientId);
}