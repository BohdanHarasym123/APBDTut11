namespace APBDTut11.DTOs;

public class GetPatientDTO
{
    public int IdPatient { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime Birthdate { get; set; }

    public List<PrescriptionDTO> Prescriptions { get; set; }
}