namespace APBDTut11.DTOs;

public class AddPrescriptionDTO
{
    public int IdDoctor { get; set; }
    public PatientDTO Patient { get; set; }
    public List<PrescriptionMedicamentDTO> Medicaments { get; set; }

    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
}