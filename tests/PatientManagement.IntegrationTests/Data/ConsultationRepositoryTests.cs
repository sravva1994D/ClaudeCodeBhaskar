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
    public async Task FR_HIS_003_single_day_filter_returns_only_visits_on_that_day()
    {
        var anna = await AddPatientAsync("Test Anna Example", "5550101");
        await SeedVisitAsync(anna, Morning.AddDays(-1));
        await SeedVisitAsync(anna, Morning);
        await SeedVisitAsync(anna, Morning.AddDays(1));

        var day = DateOnly.FromDateTime(Morning);
        var visits = await ListFilteredAsync(anna, day, day);

        Assert.Equal([Morning], visits.Select(v => v.VisitDateTime));
    }

    [Fact]
    public async Task FR_HIS_003_range_includes_both_end_days_whole_and_excludes_the_day_after()
    {
        var anna = await AddPatientAsync("Test Anna Example", "5550101");
        await SeedVisitAsync(anna, new DateTime(2026, 11, 30, 23, 59, 59));
        await SeedVisitAsync(anna, new DateTime(2026, 12, 1, 0, 0, 0));
        await SeedVisitAsync(anna, new DateTime(2026, 12, 3, 23, 59, 0));
        await SeedVisitAsync(anna, new DateTime(2026, 12, 4, 0, 0, 0));

        var visits = await ListFilteredAsync(anna, new DateOnly(2026, 12, 1), new DateOnly(2026, 12, 3));

        Assert.Equal([new DateTime(2026, 12, 3, 23, 59, 0), new DateTime(2026, 12, 1, 0, 0, 0)],
            visits.Select(v => v.VisitDateTime));
    }

    [Fact]
    public async Task FR_HIS_003_a_range_ending_on_the_last_day_the_type_holds_does_not_fail()
    {
        // Code review (Module 6): adding a day to DateOnly.MaxValue overflowed. The last day has no upper bound.
        var anna = await AddPatientAsync("Test Anna Example", "5550101");
        await SeedVisitAsync(anna, new DateTime(2026, 12, 1, 9, 0, 0));

        var visits = await ListFilteredAsync(anna, new DateOnly(2026, 12, 1), DateOnly.MaxValue);

        Assert.Equal([new DateTime(2026, 12, 1, 9, 0, 0)], visits.Select(v => v.VisitDateTime));
    }

    [Fact]
    public async Task FR_SRC_002_recent_patients_are_the_five_latest_by_last_visit()
    {
        var patients = new List<int>();
        for (var index = 1; index <= 6; index++)
        {
            patients.Add(await AddPatientAsync($"Test Patient {index}", $"55501{index:D2}"));
            await SeedVisitAsync(patients[^1], Morning.AddDays(index));
        }
        // An older visit for the first patient must not move them up the list.
        await SeedVisitAsync(patients[0], Morning.AddDays(-4));

        var recent = await RecentAsync(5);

        Assert.Equal([patients[5], patients[4], patients[3], patients[2], patients[1]], recent.Select(r => r.PatientId));
        Assert.Equal(Morning.AddDays(6), recent[0].LastVisit);
    }

    [Fact]
    public async Task FR_SRC_002_patient_with_no_visit_is_not_in_the_recent_list()
    {
        var anna = await AddPatientAsync("Test Anna Example", "5550101");
        var ben = await AddPatientAsync("Test Ben Example", "5550102");
        await SeedVisitAsync(ben, Morning);

        var recent = await RecentAsync(5);

        Assert.Equal([ben], recent.Select(r => r.PatientId));
        Assert.DoesNotContain(anna, recent.Select(r => r.PatientId));
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

    [Fact]
    public async Task FR_EXP_001_export_lists_visits_oldest_first_with_vitals_and_medicines_in_order()
    {
        var anna = await AddPatientAsync("Test Anna Example", "5550101");
        var later = await SeedVisitAsync(anna, Morning.AddDays(2));
        var earlier = await SeedVisitWithVitalsAsync(anna, Morning, "Test cough", "Test viral fever");
        await AddPrescriptionAsync(earlier,
            new PrescriptionItem { Name = "Test Medicine B", SortOrder = 2 },
            new PrescriptionItem { Name = "Test Medicine A", Dosage = "500 mg", SortOrder = 1 });

        var rows = await ExportAsync(anna);

        Assert.Equal([earlier, later], rows.Select(r => r.VisitId));
        Assert.Equal(["Test Medicine A", "Test Medicine B"], rows[0].Medicines.Select(m => m.Name));
        Assert.Equal("500 mg", rows[0].Medicines[0].Dosage);
        Assert.Equal((short)120, rows[0].SystolicMmHg);
        Assert.Equal("Test cough", rows[0].Complaints);
        Assert.Equal("Test viral fever", rows[0].Diagnosis);
    }

    [Fact]
    public async Task FR_EXP_001_export_visit_without_vitals_or_prescription_has_blank_parts()
    {
        var anna = await AddPatientAsync("Test Anna Example", "5550101");
        await SeedVisitAsync(anna, Morning);

        var row = Assert.Single(await ExportAsync(anna));

        Assert.Null(row.TemperatureC);
        Assert.Null(row.SystolicMmHg);
        Assert.Null(row.DiastolicMmHg);
        Assert.Null(row.PulseBpm);
        Assert.Empty(row.Medicines);
    }

    [Fact]
    public async Task FR_EXP_001_export_range_uses_the_visit_day_rule_and_only_this_patient()
    {
        var anna = await AddPatientAsync("Test Anna Example", "5550101");
        var ben = await AddPatientAsync("Test Ben Example", "5550102");
        await SeedVisitAsync(anna, new DateTime(2026, 11, 30, 23, 59, 0));
        var first = await SeedVisitAsync(anna, new DateTime(2026, 12, 1, 0, 0, 0));
        var last = await SeedVisitAsync(anna, new DateTime(2026, 12, 3, 23, 59, 0));
        await SeedVisitAsync(anna, new DateTime(2026, 12, 4, 0, 0, 0));
        await SeedVisitAsync(ben, new DateTime(2026, 12, 2, 9, 0, 0));

        var rows = await ExportAsync(anna, new DateOnly(2026, 12, 1), new DateOnly(2026, 12, 3));

        Assert.Equal([first, last], rows.Select(r => r.VisitId));
    }

    [Fact]
    public async Task FR_EXP_001_patient_with_no_visits_exports_no_rows()
    {
        var anna = await AddPatientAsync("Test Anna Example", "5550101");

        Assert.Empty(await ExportAsync(anna));
    }

    private async Task<int> SeedVisitWithVitalsAsync(int patientId, DateTime visitTime, string complaints, string diagnosis)
    {
        await using var context = fixture.CreateContext();
        var visit = new Consultation
        {
            PatientId = patientId,
            VisitDateTime = visitTime,
            Complaints = complaints,
            Diagnosis = diagnosis,
            Vitals = new VitalSign { TemperatureC = 36.8m, SystolicMmHg = 120, DiastolicMmHg = 80, PulseBpm = 72 }
        };
        context.Consultations.Add(visit);
        await context.SaveChangesAsync();
        return visit.Id;
    }

    private async Task AddPrescriptionAsync(int visitId, params PrescriptionItem[] items)
    {
        await using var context = fixture.CreateContext();
        context.Prescriptions.Add(new Prescription { ConsultationId = visitId, Items = items.ToList() });
        await context.SaveChangesAsync();
    }

    private async Task<IReadOnlyList<ExportVisitRow>> ExportAsync(int patientId, DateOnly? from = null, DateOnly? to = null)
    {
        await using var context = fixture.CreateContext();
        return await new ConsultationRepository(context).ListForExportAsync(patientId, from, to);
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

    private async Task<int> SeedVisitAsync(int patientId, DateTime visitTime)
    {
        await using var context = fixture.CreateContext();
        var visit = new Consultation { PatientId = patientId, VisitDateTime = visitTime };
        context.Consultations.Add(visit);
        await context.SaveChangesAsync();
        return visit.Id;
    }

    private async Task<IReadOnlyList<Consultation>> ListAsync(int patientId)
    {
        await using var context = fixture.CreateContext();
        return await new ConsultationRepository(context).ListByPatientAsync(patientId);
    }

    private async Task<IReadOnlyList<Consultation>> ListFilteredAsync(int patientId, DateOnly from, DateOnly to)
    {
        await using var context = fixture.CreateContext();
        return await new ConsultationRepository(context).ListByPatientAsync(patientId, from, to);
    }

    private async Task<IReadOnlyList<PatientLastVisit>> RecentAsync(int count)
    {
        await using var context = fixture.CreateContext();
        return await new ConsultationRepository(context).GetRecentPatientVisitsAsync(count);
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
