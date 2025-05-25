using APBDTut11.Data;
using APBDTut11.DTOs;
using APBDTut11.Models;
using Microsoft.EntityFrameworkCore;

namespace APBDTut11.Services;

public class ApiService : IApiService
{
    private readonly AppDbContext _context;

    public ApiService(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddPrescriptionMedicamentAsync(AddPrescriptionDTO request)
    {
        if(request.Medicaments.Count > 10) throw new Exception("Cannot include more than 10 medicaments");
        
        if(request.DueDate < request.Date) throw new Exception("DueDate must be greater than or equal to Date");
        
        var medicamentIds = request.Medicaments.Select(m => m.IdMedicament).ToList();
        var existingMedicaments = await _context.Medicaments
            .Where(m => medicamentIds.Contains(m.IdMedicament))
            .Select(m => m.IdMedicament).ToListAsync();
        
        var missingMedicaments = medicamentIds.Except(existingMedicaments).ToList();
        if (missingMedicaments.Any())
            throw new Exception($"These medicament IDs do not exist: {string.Join(", ", missingMedicaments)}");
        
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => 
                p.FirstName == request.Patient.FirstName && 
                p.LastName == request.Patient.LastName &&
                p.Birthdate == request.Patient.Birthdate);

        if (patient == null)
        {
            patient = new Patient()
            {
                FirstName = request.Patient.FirstName,
                LastName = request.Patient.LastName,
                Birthdate = request.Patient.Birthdate,
            };
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
        }
        
        var doctorExists = await _context.Doctors.AnyAsync(d => d.IdDoctor == request.IdDoctor);
        if (!doctorExists)
            throw new Exception($"Doctor with ID {request.IdDoctor} does not exist.");

        var prescription = new Prescription
        {
            Date = request.Date,
            DueDate = request.DueDate,
            IdDoctor = request.IdDoctor,
            IdPatient = patient.IdPatient,
            PrescriptionMedicaments = request.Medicaments.Select(m => new PrescriptionMedicament
            {
                IdMedicament = m.IdMedicament,
                Dose = m.Dose,
                Details = m.Description
            }).ToList()
        };
        
        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync();
    }

    public async Task<GetPatientDTO> GetPatientAsync(int patientId)
    {
        var patient = await _context.Patients
            .Include(p => p.Prescriptions)
            .ThenInclude(p => p.Doctor)
            .Include(p => p.Prescriptions)
            .ThenInclude(p => p.PrescriptionMedicaments).ThenInclude(m => m.Medicament)
            .FirstOrDefaultAsync(p => p.IdPatient == patientId);
        
        if(patient == null) throw new Exception("Patient not found");

        var response = new GetPatientDTO
        {
            IdPatient = patient.IdPatient,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Birthdate = patient.Birthdate,
            Prescriptions = patient.Prescriptions.OrderBy(p => p.DueDate).Select(p => new PrescriptionDTO
            {
                IdPrescription = p.IdPrescription,
                Date = p.Date,
                DueDate = p.DueDate,
                Doctor = new DoctorDTO
                {
                    IdDoctor = p.IdDoctor,
                    FirstName = p.Doctor.FirstName,
                    LastName = p.Doctor.LastName
                },
                Medicaments = p.PrescriptionMedicaments.Select(pm => new MedicamentDTO
                {
                    IdMedicament = pm.IdMedicament,
                    Name = pm.Medicament.Name,
                    Description = pm.Medicament.Description,
                    Dose = pm.Dose
                }).ToList()
            }).ToList()
        };
        
        return response;
    }
}