using System.Text.Json;
using JtechnApi.Exams.Models;
using JtechnApi.Exams.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.DependencyInjection;

var folder = Path.Combine(Path.GetTempPath(), "jtechn-exam-check-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(folder);
var dataFolder = Path.Combine(folder, "Modules", "Exams", "Data");
Directory.CreateDirectory(dataFolder);
File.Copy(Path.Combine(AppContext.BaseDirectory, "Modules", "Exams", "Data", "stamping-new.json"), Path.Combine(dataFolder, "stamping-new.json"));
var environment = new TestEnvironment { ContentRootPath = folder, WebRootPath = Path.Combine(folder, "wwwroot") };
var service = new OnlineExamRepository(environment);
var definition = service.Definition();
var questions = definition.Groups.SelectMany(g => g.Questions).ToList();
var checks = 0;
void Check(bool condition, string name) { if (!condition) throw new Exception("FAILED: " + name); checks++; Console.WriteLine("PASS: " + name); }
Check(questions.Count == 53 && questions.Select(q => q.Id).Distinct().Count() == 53, "Imported all unique questions");
Check(questions.Sum(q => q.Points) == 100, "Total points = 100");
Check(questions.Count(q => q.Kind == "matching") == 2, "Two numeric matching questions");
var perfect = questions.ToDictionary(q => q.Id, q => q.Correct.ToArray());
Check(OnlineExamRepository.Grade(definition, perfect) == 100, "All correct answers score 100");
Check(OnlineExamRepository.Grade(definition, new()) == 0, "Unanswered exam scores zero");
var multiple = questions.First(q => q.Kind == "multiple");
Check(OnlineExamRepository.Grade(definition, new() { [multiple.Id] = multiple.Correct.Take(1).ToArray() }) == 0, "Partial multiple choice receives no credit");
var matching = questions.First(q => q.Kind == "matching");
Check(OnlineExamRepository.Grade(definition, new() { [matching.Id] = matching.Correct.Reverse().ToArray() }) == 0, "Matching must preserve numeric positions");
Check(OnlineExamRepository.Classification(80) == "Đạt" && OnlineExamRepository.Classification(79.5m) == "Kiểm tra lại" && OnlineExamRepository.Classification(60) == "Đào tạo lại" && OnlineExamRepository.Classification(49.5m) == "Không đạt", "Classification boundaries");
var colorDefinition = JsonSerializer.Deserialize<OnlineExamDefinition>("{\"scores\":[96,90]}", new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
Check(OnlineExamRepository.Classification(96, colorDefinition) == "Đạt" &&
    OnlineExamRepository.Classification(95.999m, colorDefinition) == "Kiểm tra lại" &&
    OnlineExamRepository.Classification(90, colorDefinition) == "Kiểm tra lại" &&
    OnlineExamRepository.Classification(89.999m, colorDefinition) == "Không đạt (Đào tạo lại)", "Configured threshold boundaries include decimal scores");
Check(OnlineExamRepository.Classification(60, new OnlineExamDefinition()) == "Đào tạo lại", "Legacy definitions preserve classification");
foreach (var invalidScores in new[] { "[90]", "[90,96]", "[90,90]", "[101,90]", "[90,-1]" })
{
    var invalidDefinition = JsonSerializer.Serialize(definition);
    using var document = JsonDocument.Parse(invalidDefinition);
    var values = document.RootElement.EnumerateObject().ToDictionary(p => p.Name, p => p.Value.Clone());
    values["Scores"] = JsonDocument.Parse(invalidScores).RootElement.Clone();
    try { ExamDefinitionValidator.Parse(JsonSerializer.Serialize(values)); throw new Exception("Invalid thresholds accepted"); }
    catch (ArgumentException) { Check(true, "Invalid thresholds rejected: " + invalidScores); }
}
try { await service.Start("employee-a", "Test"); throw new Exception("Missing images allowed"); }
catch (InvalidOperationException) { Check(true, "Missing assets prevent opening incomplete exam"); }
// Placeholder files exist only in the temporary test environment, never in the real question bank.
foreach (var image in service.MissingImages())
{
    var path = Path.Combine(environment.WebRootPath, image.TrimStart('/'));
    Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllBytes(path, new byte[] { 0 });
}
var starts = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => service.Start("employee-a", "Test")));
Check(starts.Select(a => a.Id).Distinct().Count() == 1, "Concurrent starts resume one attempt");
var attempt = starts[0];
Check(await service.Get(attempt.Id, "employee-b") == null, "Other employee cannot read attempt");
Check(await service.Update(attempt.Id, "employee-b", perfect, true) == null, "Other employee cannot submit attempt");
try { await service.Update(attempt.Id, "employee-a", new() { [9999] = new[] { "0" } }, false); throw new Exception("Invalid question accepted"); }
catch (ArgumentException) { Check(true, "Unknown question rejected"); }
await service.Update(attempt.Id, "employee-a", perfect, false);
var restarted = new OnlineExamRepository(environment);
Check((await restarted.Get(attempt.Id, "employee-a")).Answers.Count == 53, "Progress survives service restart");
Check(!JsonSerializer.Serialize(OnlineExamRepository.CandidateView(attempt)).Contains("Correct"), "Answer key excluded from candidate response");
var submitted = await service.Update(attempt.Id, "employee-a", perfect, true);
Check(submitted.Score == 100 && submitted.SubmittedAt != null, "Submission grades and persists result");
var again = await service.Update(attempt.Id, "employee-a", new(), true);
Check(again.Score == 100 && again.SubmittedAt == submitted.SubmittedAt, "Repeated submission is idempotent");
var expired = await service.Start("employee-a", "Test");
expired.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(-1);
await File.WriteAllTextAsync(Path.Combine(folder, "App_Data", "OnlineExams", expired.Id.ToString("N") + ".json"), JsonSerializer.Serialize(expired));
var late = await service.Update(expired.Id, "employee-a", perfect, true);
Check(late.Score == 0 && late.SubmittedAt != null, "Late answers are not accepted");
Check(await service.GetCandidate(attempt.Id, "employee-b") == null, "Candidate lookup preserves ownership restriction");
var catalog = JsonSerializer.Serialize(await service.Catalog("employee-a"));
Check(!catalog.Contains("Correct") && !catalog.Contains("Password"), "Catalog excludes answer keys and credentials");
var expireOnRead = await service.Start("employee-b", "Test");
expireOnRead.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(-1);
await File.WriteAllTextAsync(Path.Combine(folder, "App_Data", "OnlineExams", expireOnRead.Id.ToString("N") + ".json"), JsonSerializer.Serialize(expireOnRead));
await service.GetCandidate(expireOnRead.Id, "employee-b");
Check((await service.Get(expireOnRead.Id, "employee-b")).SubmittedAt != null, "Repository finalizes expired attempt when opened");
Check(ExamDefinitionValidator.Parse(JsonSerializer.Serialize(definition)).Groups.Count == definition.Groups.Count, "Stored JSON accepts the complete supplied question bank");
void RejectDefinition(Action<OnlineExamDefinition> change, string name)
{
    var copy = JsonSerializer.Deserialize<OnlineExamDefinition>(JsonSerializer.Serialize(definition));
    change(copy);
    try { ExamDefinitionValidator.Parse(JsonSerializer.Serialize(copy)); throw new Exception("Accepted invalid definition: " + name); }
    catch (ArgumentException) { Check(true, name); }
}
RejectDefinition(d => d.Groups[0].Questions[0].Points += 1, "Database exam rejects totals other than 100");
var roundedDefinition = JsonSerializer.Deserialize<OnlineExamDefinition>(JsonSerializer.Serialize(definition));
roundedDefinition.Groups[0].Questions[0].Points += 0.001m;
Check(ExamDefinitionValidator.Parse(JsonSerializer.Serialize(roundedDefinition)).Groups.Count == definition.Groups.Count,
    "Database exam rounds total points to two decimal places");
RejectDefinition(d => d.Groups[1].Questions[0].Id = d.Groups[0].Questions[0].Id, "Database exam rejects duplicate question IDs");
RejectDefinition(d => d.Groups[0].Questions[0].Correct = new[] { "invalid-option" }, "Database exam rejects answer keys outside the choices");
RejectDefinition(d => d.Groups[0].Questions[0].Image = "/img/../secret.png", "Database exam rejects image path traversal");
RejectDefinition(d => d.Groups[0].Questions[0].Kind = "unknown", "Database exam rejects unknown question types");

using var services = new ServiceCollection().AddScoped<IDepartmentExamRepository>(_ => new TestExamCatalog(id =>
{
    if (id != 101 && id != 102) throw new InvalidOperationException("Exam unavailable");
    var copy = JsonSerializer.Deserialize<OnlineExamDefinition>(JsonSerializer.Serialize(definition));
    copy.Id = id; copy.Title = "Database exam " + id;
    return copy;
})).BuildServiceProvider();
IOnlineExamRepository databaseExams = new OnlineExamRepository(environment, services.GetRequiredService<IServiceScopeFactory>());
var firstExam = await databaseExams.Start("12345", "Test", 101);
var secondExam = await databaseExams.Start("12345", "Test", 102);
Check(firstExam.Id != secondExam.Id && firstExam.CatalogExamId == 101 && secondExam.CatalogExamId == 102, "Different database exams have separate active attempts");
Check((await databaseExams.Start("12345", "Test", 101)).Id == firstExam.Id, "Resume the selected database exam only");
var dbCatalog = JsonDocument.Parse(JsonSerializer.Serialize(await databaseExams.Catalog("12345", 101)));
Check(dbCatalog.RootElement.GetProperty("Attempts").GetArrayLength() == 1, "Catalog history is isolated per database exam");
Check((await databaseExams.Get(firstExam.Id, "12345")).Definition.Title == "Database exam 101", "Attempt preserves a snapshot of the selected database definition");
var selection = JsonDocument.Parse(JsonSerializer.Serialize(await databaseExams.Catalog("12345")));
Check(selection.RootElement.GetProperty("selectExam").GetBoolean(), "Landing page requests department-specific exam selection");
try { await databaseExams.Catalog("12345", 103); throw new Exception("Unassigned exam allowed"); }
catch (UnauthorizedAccessException) { Check(true, "Direct exam ID cannot bypass department authorization"); }
try { await databaseExams.StartCandidate("12346", "Test", 101); throw new Exception("Missing date allowed"); }
catch (ArgumentException) { Check(true, "Start requires an exam date"); }
var metadataCandidate = JsonDocument.Parse(JsonSerializer.Serialize(await databaseExams.StartCandidate("12346", "Test", 101,
    new OnlineExamStartInput { ExamDate = new DateTime(2020, 1, 2), IsNewEmployee = true })));
var metadataId = metadataCandidate.RootElement.GetProperty("Id").GetGuid();
var storedMetadata = await databaseExams.Get(metadataId, "12346");
Check(storedMetadata.ExamDate == new DateTime(2020, 1, 2) && storedMetadata.ExamDate != storedMetadata.StartedAt.Date,
    "Exam date is stored separately from attempt creation time");
Check(storedMetadata.IsNewEmployee == true, "New employee selection is persisted");
await databaseExams.StartCandidate("12346", "Test", 101, new OnlineExamStartInput { ExamDate = new DateTime(2021, 1, 1), IsNewEmployee = false });
var resumedMetadata = await databaseExams.Get(metadataId, "12346");
Check(resumedMetadata.ExamDate == storedMetadata.ExamDate && resumedMetadata.IsNewEmployee == true, "Resuming does not overwrite exam date or employee category");
var attemptStore = new TestAttemptStore();
using var persistentServices = new ServiceCollection()
    .AddScoped<IDepartmentExamRepository>(_ => new TestExamCatalog(id =>
    {
        var copy = JsonSerializer.Deserialize<OnlineExamDefinition>(JsonSerializer.Serialize(definition));
        copy.Id = id; copy.Title = "Stored exam";
        return copy;
    }))
    .AddScoped<IOnlineExamAttemptStore>(_ => attemptStore)
    .BuildServiceProvider();
var databaseStoredExams = new OnlineExamRepository(environment, persistentServices.GetRequiredService<IServiceScopeFactory>());
var databaseStored = JsonDocument.Parse(JsonSerializer.Serialize(await databaseStoredExams.StartCandidate("12345", "Test", 101,
    new OnlineExamStartInput { ExamDate = DateTime.UtcNow.Date })));
var databaseStoredId = databaseStored.RootElement.GetProperty("Id").GetGuid();
await databaseStoredExams.SaveCandidate(databaseStoredId, "12345", perfect, true);
Check((await new OnlineExamRepository(environment, persistentServices.GetRequiredService<IServiceScopeFactory>())
    .Get(databaseStoredId, "12345")).Score == 100, "Registered attempt store persists submitted results across repository instances");
Check((await databaseStoredExams.History("12345")).Any(item => item.Id == databaseStoredId), "Registered attempt store provides history without file storage");
Console.WriteLine($"{checks} checks passed. Temporary test data: {folder}");

sealed class TestEnvironment : IWebHostEnvironment
{
    public string ApplicationName { get; set; } = "ExamChecks";
    public string EnvironmentName { get; set; } = "Development";
    public string ContentRootPath { get; set; }
    public string WebRootPath { get; set; }
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
}

sealed class TestExamCatalog : IDepartmentExamRepository
{
    private readonly Func<int, OnlineExamDefinition> load;
    public TestExamCatalog(Func<int, OnlineExamDefinition> load) => this.load = load;
    public OnlineExamDefinition LoadDefinition(int id) => load(id);
    public bool CanTake(int employeeId, int examId) => (employeeId == 12345 || employeeId == 12346) && (examId == 101 || examId == 102);
    public Task<object> Available(int employeeId) => Task.FromResult<object>(Array.Empty<object>());
    public Task<object> List(int? departmentId) => throw new NotSupportedException();
    public Task<JtechnApi.Shares.RepositoryResult> GetById(int id) => throw new NotSupportedException();
    public Task<JtechnApi.Shares.RepositoryResult> Save(int? id, JtechnApi.Exams.Dtos.DepartmentExamInput input) => throw new NotSupportedException();
    public Task<JtechnApi.Shares.RepositoryResult> Delete(int id) => throw new NotSupportedException();
}

sealed class TestAttemptStore : IOnlineExamAttemptStore
{
    private readonly Dictionary<Guid, string> attempts = new();

    public Task<OnlineExamAttempt> Get(Guid id, string employeeId)
    {
        if (!attempts.TryGetValue(id, out var value)) return Task.FromResult<OnlineExamAttempt>(null);
        var attempt = JsonSerializer.Deserialize<OnlineExamAttempt>(value);
        return Task.FromResult(attempt.EmployeeId == employeeId ? attempt : null);
    }

    public Task<List<OnlineExamAttempt>> History(string employeeId) => Task.FromResult(attempts.Values
        .Select(value => JsonSerializer.Deserialize<OnlineExamAttempt>(value))
        .Where(attempt => attempt.EmployeeId == employeeId)
        .OrderByDescending(attempt => attempt.StartedAt)
        .ToList());

    public Task Save(OnlineExamAttempt attempt)
    {
        attempts[attempt.Id] = JsonSerializer.Serialize(attempt);
        return Task.CompletedTask;
    }
}
