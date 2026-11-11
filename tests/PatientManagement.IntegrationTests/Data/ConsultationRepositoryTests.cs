using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Abstractions;
using PatientManagement.Data.Entities;
using PatientManagement.Data.Repositories;
using PatientManagement.IntegrationTests.Fixtures;

namespace PatientManagement.IntegrationTests.Data;

// Repository tests for visits against SQL Server. Synthetic values only. Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public class ConsultationRepositoryTests(TestDatabaseFixture fixture) : IAsyncLifetime
{
    private static readonly DateTime Morning = new(2026, 12, 1, 9, 0, 0);

    public async Task InitializeAsync() => await Clear();

    public async Task DisposeAsync() => await Clear();

    [Fact]
    public async Task FR_HIS_001_visits_are_listed_newest_first_for_one_patient_only()
    {
        var anna = await AddPatientAsync("Test Anna Example", "5550101");
        var ben = await AddPatientAsync("Test Ben Example", "5550102");
        await SeedVisitAsync(anna, Morning);
        await SeedVisitAsync(anna, Morning.AddDays(3));
        await SeedVisitAsync(ben, Morning.AddDays(5));

        var visits = await ListAsync(anna);

        Assert.Equal([Morning.AddDays(3), Morning], visits.Select(v => v.VisitDateTime));
    }

    [Fact]
    public async Task FR_HIS_001_latest_visit_time_comes_from_visits_only()
    {
        var anna = await AddPatientAsync("Test Anna Example", "5550101");
        var ben = await AddPatientAsync("Test Ben Example", "5550102");
        await SeedVisitAsync(anna, Morning);
        await SeedVisitAsync(anna, Morning.AddDays(2));
        await SeedVisitAsync(ben, Morning.AddDays(9));

        Assert.Equal(Morning.AddDays(2), await LatestVisitAsync(anna));
    }

    [Fact]
    public async Task FR_HIS_001_patient_with_no_visit_has_no_latest_visit()
    {
        var anna = await AddPatientAsync("Test Anna Example", "5550101");

        Assert.Null(await LatestVisitAsync(anna));
    }

    [Fact]
    public async Task FR_HIS_001_get_by_id_returns_null_for_an_unknown_visit()
    {
        await using var context = fixture.CreateContext();

        Assert.Null(await new ConsultationRepository(context).GetByIdAsync(999999));
    }

    [Fact]
    public async Task FR_HIS_001_get_by_appointment_returns_the_linked_visit()
    {
        var anna = await AddPatientAsync("Test Anna Example", "5550101");
        var appointmentId = await AddAppointmentAsync(anna, "Checked-In");
        await using (var context = fixture.CreateContext())
        {
            await new ConsultationRepository(context).AddAsync(NewVisit(anna, appointmentId));
        }

        await using var reader = fixture.CreateContext();
        var visit = await new ConsultationRepository(reader).GetByAppointmentIdAsync(appointmentId);

        Assert.NotNull(visit);
        Assert.Equal(appointmentId, visit!.AppointmentId);
    }

    [Fact]
    public async Task FR_APT_003_start_from_an_appointment_saves_the_visit_and_the_status_together()
    {
        var anna = await AddPatientAsync("Test Anna Example", "5550101");
        var appointmentId = await AddAppointmentAsync(anna, "Checked-In");

        await using (var context = fixture.CreateContext())
        {
            var appointment = await context.Appointments.SingleAsync(a => a.Id == appointmentId);
            appointment.Status = "In Consultation";
            await new ConsultationRepository(context).AddAsync(NewVisit(anna, appointmentId), appointment);
        }

        await using var reader = fixture.CreateContext();
        Assert.Equal("In Consultation", (await reader.Appointments.SingleAsync()).Status);
        Assert.Equal(1, await reader.Consultations.CountAsync());
    }

    [Fact]
    public async Task FR_HIS_001_second_visit_for_the_same_appointment_raises_already_visited()
    {
        var anna = await AddPatientAsync("Test Anna Example", "5550101");
        var appointmentId = await AddAppointmentAsync(anna, "Checked-In");
        await using (var context = fixture.CreateContext())
        {
            await new ConsultationRepository(context).AddAsync(NewVisit(anna, appointmentId));
        }

        await using var second = fixture.CreateContext();

        await Assert.ThrowsAsync<AppointmentAlreadyVisitedException>(() =>
            new ConsultationRepository(second).AddAsync(NewVisit(anna, appointmentId)));
    }

    [Fact]
    public async Task FR_HIS_001_walk_ins_can_be_added_without_an_appointment()
    {
        var anna = await AddPatientAsync("Test Anna Example", "5550101");

        await using (var context = fixture.CreateContext())
        {
            var repository = new ConsultationRepository(context);
            await repository.AddAsync(NewVisit(anna, appointmentId: null));
            await repository.AddAsync(NewVisit(anna, appointmentId: null));
        }

        Assert.Equal(2, (await ListAsync(anna)).Count);
    }

    [Fact]
    public async Task FR_APT_003_finish_saves_the_finished_time_and_the_completed_status_together()
    {
        var anna = await AddPatientAsync("Test Anna Example", "5550101");
        var appointmentId = await AddAppointmentAsync(anna, "In Consultation");
        int visitId;
        await using (var context = fixture.CreateContext())
        {
            var visit = NewVisit(anna, appointmentId);
            context.Consultations.Add(visit);
            await context.SaveChangesAsync();
            visitId = visit.Id;
        }

        await using (var context = fixture.CreateContext())
        {
            var repository = new ConsultationRepository(context);
            var appointment = await context.Appointments.SingleAsync(a => a.Id == appointmentId);
            appointment.Status = "Completed";
            Assert.True(await repository.MarkFinishedAsync(visitId, Morning.AddMinutes(25), appointment));
        }

        await using var reader = fixture.CreateContext();
        var saved = await reader.Consultations.SingleAsync();
        Assert.Equal(Morning.AddMinutes(25), saved.FinishedAt);
        Assert.Equal("Completed", (await reader.Appointments.SingleAsync()).Status);
    }

    [Fact]
    public async Task FR_APT_003_a_second_finish_changes_nothing()
    {
        var anna = await AddPatientAsync("Test Anna Example", "5550101");
        await using (var context = fixture.CreateContext())
        {
            await new ConsultationRepository(context).AddAsync(NewVisit(anna, appointmentId: null));
        }

        int visitId;
        await using (var context = fixture.CreateContext())
        {
            visitId = (await context.Consultations.SingleAsync()).Id;
            Assert.True(await new ConsultationRepository(context).MarkFinishedAsync(visitId, Morning.AddMinutes(25)));
        }

        await using (var context = fixture.CreateContext())
        {
            Assert.False(await new ConsultationRepository(context).MarkFinishedAsync(visitId, Morning.AddMinutes(40)));
        }

        await using var reader = fixture.CreateContext();
        Assert.Equal(Morning.AddMinutes(25), (await reader.Consultations.SingleAsync()).FinishedAt);
    }

    [Fact]
    public async Task FR_HIS_002_get_by_id_with_vitals_loads_them()
    {
        var anna = await AddPatientAsync("Test Anna Example", "5550101");
        int visitId;
        await using (var context = fixture.CreateContext())
        {
            var visit = NewVisit(anna, appointmentId: null);
            visit.Vitals = new VitalSign
            {
                TemperatureC = 37.0m,
                SystolicMmHg = 120,
                DiastolicMmHg = 80,
                PulseBpm = 70
            };
            context.Consultations.Add(visit);
            await context.SaveChangesAsync();
            visitId = visit.Id;
        }

        await using var reader = fixture.CreateContext();
        var loaded = await new ConsultationRepository(reader).GetByIdAsync(visitId, includeVitals: true);

        Assert.NotNull(loaded!.Vitals);
        Assert.Equal(120, loaded.Vitals!.SystolicMmHg);
    }

    [Fact]
    public async Task FR_RX_002_a_visit_is_not_saved_once_its_prescription_is_printed()
    {
        var anna = await AddPatientAsync("Test Anna Example", "5550101");
        var visitId = await SeedVisitWithPrintedPrescriptionAsync(anna);

        await using (var context = fixture.CreateContext())
        {
            var visit = await context.Consultations.SingleAsync(consultation => consultation.Id == visitId);
            visit.Complaints = "Test late note";

            Assert.False(await new ConsultationRepository(context).UpdateAsync(visit));
        }

        await using var reader = fixture.CreateContext();
        Assert.Null((await reader.Consultations.SingleAsync()).Complaints);
        Assert.Equal(0, await reader.VitalSigns.CountAsync());
    }

    [Fact]
    public async Task FR_VIT_001_visit_text_and_new_vitals_are_saved_in_one_save()
    {
        // D-M5-8: the visit text and the vitals are written together, or not at all.
        var anna = await AddPatientAsync("Test Anna Example", "5550101");
        int visitId;
        await using (var context = fixture.CreateContext())
        {
            var visit = NewVisit(anna, appointmentId: null);
            context.Consultations.Add(visit);
            await context.SaveChangesAsync();
            visitId = visit.Id;
        }

        await using (var context = fixture.CreateContext())
        {
            var visit = await context.Consultations.Include(consultation => consultation.Vitals)
                .SingleAsync(consultation => consultation.Id == visitId);
            visit.Complaints = "Test cough";
            visit.Vitals = new VitalSign { ConsultationId = visitId, TemperatureC = 36.8m, SystolicMmHg = 120, DiastolicMmHg = 80, PulseBpm = 72 };

            Assert.True(await new ConsultationRepository(context).UpdateAsync(visit));
        }

        await using var reader = fixture.CreateContext();
        var saved = await reader.Consultations.Include(consultation => consultation.Vitals).SingleAsync();
        Assert.Equal("Test cough", saved.Complaints);
        Assert.Equal(120, saved.Vitals!.SystolicMmHg);
    }

    private async Task<int> SeedVisitWithPrintedPrescriptionAsync(int patientId)
    {
        await using var context = fixture.CreateContext();
        var visit = NewVisit(patientId, appointmentId: null);
        context.Consultations.Add(visit);
        await context.SaveChangesAsync();
        context.Prescriptions.Add(new Prescription
        {
            ConsultationId = visit.Id,
            IsPrinted = true,
            PrintedAt = Morning.AddMinutes(30),
            Items = [new PrescriptionItem { Name = "Test Medicine", SortOrder = 1 }]
        });
        await context.SaveChangesAsync();
        return visit.Id;
    }

    private async Task<int> AddPatientAsync(string fullName, string phone)
    {
        await using var context = fixture.CreateContext();
        var patient = new Patient
        {
            FullName = fullName,
            DateOfBirth = new DateOnly(1980, 1, 1),
            Gender = "Female",
            Phone = phone,
            Email = "patient@example.test",
            Address = "Test address"
        };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        return patient.Id;
    }

    private async Task<int> AddAppointmentAsync(int patientId, string status)
    {
        await using var context = fixture.CreateContext();
        var appointment = new Appointment
        {
            PatientId = patientId,
            StartTime = Morning,
            DurationMinutes = 30,
            Status = status,
            IsArchived = false
        };
        context.Appointments.Add(appointment);
        await context.SaveChangesAsync();
        return appointment.Id;
    }

    private async Task SeedVisitAsync(int patientId, DateTime visitTime)
    {
        await using var context = fixture.CreateContext();
        context.Consultations.Add(new Consultation { PatientId = patientId, VisitDateTime = visitTime });
        await context.SaveChangesAsync();
    }

    private async Task<IReadOnlyList<Consultation>> ListAsync(int patientId)
    {
        await using var context = fixture.CreateContext();
        return await new ConsultationRepository(context).ListByPatientAsync(patientId);
    }

    private async Task<DateTime?> LatestVisitAsync(int patientId)
    {
        await using var context = fixture.CreateContext();
        return await new ConsultationRepository(context).GetLatestVisitTimeAsync(patientId);
    }

    private static Consultation NewVisit(int patientId, int? appointmentId) => new()
    {
        PatientId = patientId,
        AppointmentId = appointmentId,
        VisitDateTime = Morning
    };

    private async Task Clear() =>
        await fixture.ClearTablesAsync("PrescriptionItems", "Prescriptions", "VitalSigns", "Consultations", "Appointments", "Patients");
}
