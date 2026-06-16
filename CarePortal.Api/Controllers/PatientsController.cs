using CarePortal.Application.Patients;
using Microsoft.AspNetCore.Mvc;

namespace CarePortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PatientsController : ControllerBase
{
    private readonly PatientsService _patients;

    public PatientsController(PatientsService patients)
    {
        _patients = patients;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PatientResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var patients = await _patients.ListAsync(cancellationToken);
        return Ok(patients);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var patient = await _patients.GetByIdAsync(id, cancellationToken);
        return patient is null ? NotFound() : Ok(patient);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreatePatientRequest request, CancellationToken cancellationToken)
    {
        var patient = await _patients.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient);
    }
}
