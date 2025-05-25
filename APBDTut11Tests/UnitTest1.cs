using APBDTut11.Data;
using APBDTut11.DTOs;
using APBDTut11.Models;
using APBDTut11.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

public class UnitTest1
{
    private readonly AppDbContext _context;
    private readonly ApiService _service;

    public UnitTest1()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        _context = new AppDbContext(options);
        _service = new ApiService(_context);

        _context.Doctors.Add(new Doctor { IdDoctor = 1, FirstName = "Greg", LastName = "House", Email = "house@hospital.com" });
        _context.Medicaments.Add(new Medicament { IdMedicament = 1, Name = "Ibuprofen", Description = "Painkiller", Type = "Tablet" });
        
        _context.SaveChanges();
    }

    [Fact]
    public async Task AddPrescription_ValidData()
    {
        var request = new AddPrescriptionDTO
        {
            IdDoctor = 1,
            Date = DateTime.Today,
            DueDate = DateTime.Today.AddDays(3),
            Patient = new PatientDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Birthdate = new DateTime(1990, 1, 1)
            },
            Medicaments = new List<PrescriptionMedicamentDTO>
            {
                new PrescriptionMedicamentDTO { IdMedicament = 1, Dose = 2, Description = "Morning" }
            }
        };

        await _service.AddPrescriptionMedicamentAsync(request);
        _context.Prescriptions.Count().Should().Be(1);
    }

    [Fact]
    public async Task AddPrescription_MoreThan10Medicaments()
    {
        var request = new AddPrescriptionDTO
        {
            IdDoctor = 1,
            Date = DateTime.Today,
            DueDate = DateTime.Today.AddDays(3),
            Patient = new PatientDTO { FirstName = "A", LastName = "B", Birthdate = DateTime.Today },
            Medicaments = Enumerable.Range(0, 11).Select(i => new PrescriptionMedicamentDTO
            {
                IdMedicament = 1,
                Dose = 1,
                Description = "test"
            }).ToList()
        };

        Func<Task> act = () => _service.AddPrescriptionMedicamentAsync(request);
        await act.Should().ThrowAsync<Exception>().WithMessage("*more than 10*");
    }

    [Fact]
    public async Task AddPrescription_InvalidDates()
    {
        var request = new AddPrescriptionDTO
        {
            IdDoctor = 1,
            Date = DateTime.Today,
            DueDate = DateTime.Today.AddDays(-1),
            Patient = new PatientDTO { FirstName = "A", LastName = "B", Birthdate = DateTime.Today },
            Medicaments = new List<PrescriptionMedicamentDTO>
            {
                new PrescriptionMedicamentDTO { IdMedicament = 2, Dose = 1, Description = "night" }
            }
        };

        Func<Task> act = () => _service.AddPrescriptionMedicamentAsync(request);
        await act.Should().ThrowAsync<Exception>().WithMessage("*DueDate must be*");
    }

    [Fact]
    public async Task AddPrescription_NonExistingMedicament()
    {
        var request = new AddPrescriptionDTO
        {
            IdDoctor = 1,
            Date = DateTime.Today,
            DueDate = DateTime.Today.AddDays(3),
            Patient = new PatientDTO { FirstName = "A", LastName = "B", Birthdate = DateTime.Today },
            Medicaments = new List<PrescriptionMedicamentDTO>
            {
                new PrescriptionMedicamentDTO { IdMedicament = 99, Dose = 1, Description = "fail" }
            }
        };

        Func<Task> act = () => _service.AddPrescriptionMedicamentAsync(request);
        await act.Should().ThrowAsync<Exception>().WithMessage("*do not exist*");
    }

    [Fact]
    public async Task GetPatientDetails()
    {
        var patient = new Patient
        {
            FirstName = "T",
            LastName = "User",
            Birthdate = new DateTime(2000, 1, 1)
        };
        _context.Patients.Add(patient);
        _context.SaveChanges();

        _context.Prescriptions.Add(new Prescription
        {
            Date = DateTime.Today,
            DueDate = DateTime.Today.AddDays(1),
            IdDoctor = 1,
            IdPatient = patient.IdPatient,
            PrescriptionMedicaments = new List<PrescriptionMedicament>
            {
                new PrescriptionMedicament
                {
                    IdMedicament = 1,
                    Dose = 1,
                    Details = "after meal"
                }
            }
        });
        _context.SaveChanges();
        
        var result = await _service.GetPatientAsync(patient.IdPatient);
        
        result.FirstName.Should().Be("T");
        result.Prescriptions.Should().HaveCount(1);
        result.Prescriptions[0].Medicaments.Should().HaveCount(1);
    }
}
