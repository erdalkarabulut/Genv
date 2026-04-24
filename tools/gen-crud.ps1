# PowerShell script to (re)generate feature CRUD command/response DTOs for the
# stem-cell / cryo management domain. Each entity defines the fields the generator
# should place inside Create / Update / Range / Response / GetById / ListItem DTOs.
#
# Running:  pwsh tools/gen-crud.ps1
#
# After regeneration the generator ALSO updates:
#   - Persistence/EntityConfigurations/*Configuration.cs
#   - Application/Features/<X>/Profiles/MappingProfiles.cs (left untouched - AutoMapper convention based)
#   - Application/Features/<X>/Commands/*/*Validator.cs (writable fields)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$src  = Join-Path $root "src/genVApi"

# ------------------------------------------------------------------
# Domain schema
# ------------------------------------------------------------------
# Each entity:
#   Name           : entity class name (singular)
#   PluralName     : feature folder name (plural)
#   Fields         : ordered list of fields (type, name)
#   Required       : fields to add NotEmpty validator for
#   NeedsRequired  : fields nullable in request (optional)
# ------------------------------------------------------------------
$schema = @(
    @{
        Name        = "Patient"
        Plural      = "Patients"
        Fields = @(
            @{ Type = "string";           Name = "FullName" }
            @{ Type = "double";           Name = "WeightKg" }
            @{ Type = "string?";          Name = "BloodGroup" }
            @{ Type = "Domain.Enums.TransplantType"; Name = "TransplantType" }
            @{ Type = "string?";          Name = "Diagnosis" }
            @{ Type = "string?";          Name = "ProtocolNo" }
            @{ Type = "DateTime?";        Name = "BirthDate" }
            @{ Type = "Guid?";            Name = "DonorId" }
        )
        Required = @("FullName")
    }
    @{
        Name        = "Donor"
        Plural      = "Donors"
        Fields = @(
            @{ Type = "string";           Name = "FullName" }
            @{ Type = "double";           Name = "WeightKg" }
            @{ Type = "string?";          Name = "BloodGroup" }
            @{ Type = "string?";          Name = "Relation" }
        )
        Required = @("FullName")
    }
    @{
        Name        = "CollectionSession"
        Plural      = "CollectionSessions"
        Fields = @(
            @{ Type = "Guid";             Name = "PatientId" }
            @{ Type = "int";              Name = "Day" }
            @{ Type = "DateTime";         Name = "Date" }
            @{ Type = "double?";          Name = "WbcPre" }
            @{ Type = "double?";          Name = "Hgb" }
            @{ Type = "double?";          Name = "Hct" }
            @{ Type = "double?";          Name = "Plt" }
            @{ Type = "double";           Name = "VolumeMl" }
            @{ Type = "double";           Name = "WBC" }
            @{ Type = "double";           Name = "Cd34Percent" }
            @{ Type = "double";           Name = "Cd45Percent" }
            @{ Type = "double";           Name = "Cd3Percent" }
            @{ Type = "double?";          Name = "LymphocytePercent" }
            @{ Type = "double?";          Name = "Mhs" }
            @{ Type = "double";           Name = "Cd34PerKg" }
            @{ Type = "double";           Name = "Cd3PerKg" }
        )
        Required = @("PatientId")
    }
    @{
        Name        = "Bag"
        Plural      = "Bags"
        Fields = @(
            @{ Type = "Guid";             Name = "SessionId" }
            @{ Type = "int";              Name = "BagNumber" }
            @{ Type = "double";           Name = "VolumeMl" }
            @{ Type = "double";           Name = "SourceVolumeMl" }
            @{ Type = "double";           Name = "Cd34PerKg" }
            @{ Type = "double";           Name = "Cd3PerKg" }
            @{ Type = "Domain.Enums.BagStatus"; Name = "Status" }
            @{ Type = "Guid?";            Name = "SlotId" }
        )
        Required = @("SessionId")
    }
    @{
        Name        = "BagMovement"
        Plural      = "BagMovements"
        Fields = @(
            @{ Type = "Guid";             Name = "BagId" }
            @{ Type = "Guid?";            Name = "FromSlotId" }
            @{ Type = "Guid?";            Name = "ToSlotId" }
            @{ Type = "string";           Name = "Action" }
        )
        Required = @("BagId","Action")
    }
    @{
        Name        = "Box"
        Plural      = "Boxes"
        Fields = @(
            @{ Type = "Guid";             Name = "RackId" }
            @{ Type = "string";           Name = "Name" }
        )
        Required = @("RackId","Name")
    }
    @{
        Name        = "DliProduct"
        Plural      = "DliProducts"
        Fields = @(
            @{ Type = "Guid";             Name = "PatientId" }
            @{ Type = "double";           Name = "Cd3PerKg" }
            @{ Type = "double";           Name = "VolumeMl" }
            @{ Type = "string?";          Name = "Notes" }
        )
        Required = @("PatientId")
    }
    @{
        Name        = "Product"
        Plural      = "Products"
        Fields = @(
            @{ Type = "Guid";             Name = "SessionId" }
            @{ Type = "double";           Name = "TotalVolume" }
            @{ Type = "double";           Name = "TotalWbc" }
            @{ Type = "double";           Name = "Cd34Percent" }
            @{ Type = "double";           Name = "Cd45Percent" }
            @{ Type = "double";           Name = "Cd3Percent" }
            @{ Type = "double";           Name = "TotalCd34PerKg" }
        )
        Required = @("SessionId")
    }
    @{
        Name        = "Rack"
        Plural      = "Racks"
        Fields = @(
            @{ Type = "Guid";             Name = "TankId" }
            @{ Type = "string";           Name = "Name" }
        )
        Required = @("TankId","Name")
    }
    @{
        Name        = "Slot"
        Plural      = "Slots"
        Fields = @(
            @{ Type = "Guid";             Name = "BoxId" }
            @{ Type = "string";           Name = "Position" }
            @{ Type = "bool";             Name = "IsOccupied" }
            @{ Type = "int";              Name = "Version" }
        )
        Required = @("BoxId","Position")
    }
    @{
        Name        = "Tank"
        Plural      = "Tanks"
        Fields = @(
            @{ Type = "string";           Name = "Name" }
        )
        Required = @("Name")
    }
)

function Format-Props([object[]]$Fields,[string]$Indent = "    ") {
    ($Fields | ForEach-Object { "$Indent" + "public $($_.Type) $($_.Name) { get; set; }" }) -join "`n"
}

function Write-File([string]$Path,[string]$Content) {
    $dir = Split-Path -Parent $Path
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
    [System.IO.File]::WriteAllText($Path, $Content, [System.Text.UTF8Encoding]::new($false))
    Write-Host "  wrote $Path"
}

foreach ($e in $schema) {
    $n   = $e.Name
    $p   = $e.Plural
    $fld = $e.Fields
    $req = $e.Required
    $lc  = $n.Substring(0,1).ToLowerInvariant() + $n.Substring(1)

    $featureDir = Join-Path $src "Application/Features/$p"
    $propsBlock = Format-Props $fld
    $propsResp  = Format-Props $fld

    Write-Host "== $p ==" -ForegroundColor Cyan

    # ---- Create command ----
    $createCmd = @"
using Application.Features.$p.Constants;
using Application.Features.$p.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using MediatR;
using static Application.Features.$p.Constants.${p}OperationClaims;

namespace Application.Features.$p.Commands.Create;

public class Create${n}Command : IRequest<Created${n}Response>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
$propsBlock

    public string[] Roles => [Admin, Write, ${p}OperationClaims.Create];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["Get$p"];

    public class Create${n}CommandHandler : IRequestHandler<Create${n}Command, Created${n}Response>
    {
        private readonly IMapper _mapper;
        private readonly I${n}Repository _${lc}Repository;
        private readonly ${n}BusinessRules _${lc}BusinessRules;

        public Create${n}CommandHandler(IMapper mapper, I${n}Repository ${lc}Repository,
                                         ${n}BusinessRules ${lc}BusinessRules)
        {
            _mapper = mapper;
            _${lc}Repository = ${lc}Repository;
            _${lc}BusinessRules = ${lc}BusinessRules;
        }

        public async Task<Created${n}Response> Handle(Create${n}Command request, CancellationToken cancellationToken)
        {
            $n $lc = _mapper.Map<$n>(request);

            await _${lc}Repository.AddAsync($lc);

            Created${n}Response response = _mapper.Map<Created${n}Response>($lc);
            return response;
        }
    }
}
"@
    Write-File "$featureDir/Commands/Create/Create${n}Command.cs" $createCmd

    # ---- Created response ----
    $createdResp = @"
using NArchitecture.Core.Application.Responses;

namespace Application.Features.$p.Commands.Create;

public class Created${n}Response : IResponse
{
    public Guid Id { get; set; }
$propsResp
}
"@
    Write-File "$featureDir/Commands/Create/Created${n}Response.cs" $createdResp

    # ---- Create validator ----
    $reqRules = ($req | ForEach-Object { "        RuleFor(c => c.$_).NotEmpty();" }) -join "`n"
    if (-not $reqRules) { $reqRules = "        // no required fields" }
    $createVal = @"
using FluentValidation;

namespace Application.Features.$p.Commands.Create;

public class Create${n}CommandValidator : AbstractValidator<Create${n}Command>
{
    public Create${n}CommandValidator()
    {
$reqRules
    }
}
"@
    Write-File "$featureDir/Commands/Create/Create${n}CommandValidator.cs" $createVal

    # ---- CreateRange command ----
    $createRange = @"
using Application.Features.$p.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using static Application.Features.$p.Constants.${p}OperationClaims;

namespace Application.Features.$p.Commands.CreateRange;

public class Create${n}RangeCommand : IRequest<Created${n}RangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<Create${n}RangeItem> Items { get; set; } = new List<Create${n}RangeItem>();

    public string[] Roles => [Admin, Write, ${p}OperationClaims.CreateRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["Get$p"];

    public class Create${n}RangeItem
    {
$(Format-Props $fld "        ")
    }

    public class Create${n}RangeCommandHandler : IRequestHandler<Create${n}RangeCommand, Created${n}RangeResponse>
    {
        private readonly IMapper _mapper;
        private readonly I${n}Repository _${lc}Repository;

        public Create${n}RangeCommandHandler(IMapper mapper, I${n}Repository ${lc}Repository)
        {
            _mapper = mapper;
            _${lc}Repository = ${lc}Repository;
        }

        public async Task<Created${n}RangeResponse> Handle(Create${n}RangeCommand request, CancellationToken cancellationToken)
        {
            List<$n> ${lc}s = request.Items.Select(item => _mapper.Map<$n>(item)).ToList();

            ICollection<$n> added${n}s = await _${lc}Repository.AddRangeAsync(${lc}s);

            return new Created${n}RangeResponse { Ids = added${n}s.Select(e => e.Id).ToList() };
        }
    }
}
"@
    Write-File "$featureDir/Commands/CreateRange/Create${n}RangeCommand.cs" $createRange

    # ---- Update command ----
    $updateCmd = @"
using Application.Features.$p.Constants;
using Application.Features.$p.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using MediatR;
using static Application.Features.$p.Constants.${p}OperationClaims;

namespace Application.Features.$p.Commands.Update;

public class Update${n}Command : IRequest<Updated${n}Response>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid Id { get; set; }
$propsBlock

    public string[] Roles => [Admin, Write, ${p}OperationClaims.Update];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["Get$p"];

    public class Update${n}CommandHandler : IRequestHandler<Update${n}Command, Updated${n}Response>
    {
        private readonly IMapper _mapper;
        private readonly I${n}Repository _${lc}Repository;
        private readonly ${n}BusinessRules _${lc}BusinessRules;

        public Update${n}CommandHandler(IMapper mapper, I${n}Repository ${lc}Repository,
                                         ${n}BusinessRules ${lc}BusinessRules)
        {
            _mapper = mapper;
            _${lc}Repository = ${lc}Repository;
            _${lc}BusinessRules = ${lc}BusinessRules;
        }

        public async Task<Updated${n}Response> Handle(Update${n}Command request, CancellationToken cancellationToken)
        {
            ${n}? $lc = await _${lc}Repository.GetAsync(predicate: x => x.Id == request.Id, cancellationToken: cancellationToken);
            await _${lc}BusinessRules.${n}ShouldExistWhenSelected($lc);
            $lc = _mapper.Map(request, $lc);

            await _${lc}Repository.UpdateAsync($lc!);

            Updated${n}Response response = _mapper.Map<Updated${n}Response>($lc);
            return response;
        }
    }
}
"@
    Write-File "$featureDir/Commands/Update/Update${n}Command.cs" $updateCmd

    # ---- Updated response ----
    $updatedResp = @"
using NArchitecture.Core.Application.Responses;

namespace Application.Features.$p.Commands.Update;

public class Updated${n}Response : IResponse
{
    public Guid Id { get; set; }
$propsResp
}
"@
    Write-File "$featureDir/Commands/Update/Updated${n}Response.cs" $updatedResp

    # ---- Update validator ----
    $updateVal = @"
using FluentValidation;

namespace Application.Features.$p.Commands.Update;

public class Update${n}CommandValidator : AbstractValidator<Update${n}Command>
{
    public Update${n}CommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
$reqRules
    }
}
"@
    Write-File "$featureDir/Commands/Update/Update${n}CommandValidator.cs" $updateVal

    # ---- UpdateRange command ----
    $updateRange = @"
using Application.Features.$p.Constants;
using Application.Features.$p.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using static Application.Features.$p.Constants.${p}OperationClaims;

namespace Application.Features.$p.Commands.UpdateRange;

public class Update${n}RangeCommand : IRequest<Updated${n}RangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<Update${n}RangeItem> Items { get; set; } = new List<Update${n}RangeItem>();

    public string[] Roles => [Admin, Write, ${p}OperationClaims.UpdateRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["Get$p"];

    public class Update${n}RangeItem
    {
        public Guid Id { get; set; }
$(Format-Props $fld "        ")
    }

    public class Update${n}RangeCommandHandler : IRequestHandler<Update${n}RangeCommand, Updated${n}RangeResponse>
    {
        private readonly IMapper _mapper;
        private readonly I${n}Repository _${lc}Repository;
        private readonly ${n}BusinessRules _${lc}BusinessRules;

        public Update${n}RangeCommandHandler(IMapper mapper, I${n}Repository ${lc}Repository, ${n}BusinessRules ${lc}BusinessRules)
        {
            _mapper = mapper;
            _${lc}Repository = ${lc}Repository;
            _${lc}BusinessRules = ${lc}BusinessRules;
        }

        public async Task<Updated${n}RangeResponse> Handle(Update${n}RangeCommand request, CancellationToken cancellationToken)
        {
            List<$n> items = new List<$n>();

            foreach (Update${n}RangeItem item in request.Items)
            {
                ${n}? entity = await _${lc}Repository.GetAsync(
                    predicate: x => x.Id == item.Id,
                    cancellationToken: cancellationToken
                );
                await _${lc}BusinessRules.${n}ShouldExistWhenSelected(entity);
                _mapper.Map(source: item, destination: entity!);
                items.Add(entity!);
            }

            ICollection<$n> updated = await _${lc}Repository.UpdateRangeAsync(items);

            return new Updated${n}RangeResponse { Ids = updated.Select(e => e.Id).ToList() };
        }
    }
}
"@
    Write-File "$featureDir/Commands/UpdateRange/Update${n}RangeCommand.cs" $updateRange

    # ---- GetById response ----
    $getByIdResp = @"
using NArchitecture.Core.Application.Responses;

namespace Application.Features.$p.Queries.GetById;

public class GetById${n}Response : IResponse
{
    public Guid Id { get; set; }
$propsResp
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
"@
    Write-File "$featureDir/Queries/GetById/GetById${n}Response.cs" $getByIdResp

    # ---- GetList list item dto ----
    $listDto = @"
using NArchitecture.Core.Application.Dtos;

namespace Application.Features.$p.Queries.GetList;

public class GetList${n}ListItemDto : IDto
{
    public Guid Id { get; set; }
$propsResp
    public DateTime CreatedDate { get; set; }
}
"@
    Write-File "$featureDir/Queries/GetList/GetList${n}ListItemDto.cs" $listDto
}

Write-Host "`nAll feature DTOs regenerated." -ForegroundColor Green
