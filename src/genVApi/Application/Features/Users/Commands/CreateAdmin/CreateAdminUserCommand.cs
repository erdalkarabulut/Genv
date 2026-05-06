using Application.Features.Users.Constants;
using Application.Features.Users.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Persistence.Paging;
using NArchitecture.Core.Security.Hashing;
using static Application.Features.Users.Constants.UsersOperationClaims;

namespace Application.Features.Users.Commands.CreateAdmin;

public class CreateAdminUserCommand : IRequest<CreatedAdminUserResponse>, ISecuredRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public List<int> OperationClaimIds { get; set; } = new();

    public CreateAdminUserCommand()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
        Password = string.Empty;
    }

    public string[] Roles => new[] { Admin, Write, UsersOperationClaims.Create };

    public class CreateAdminUserCommandHandler : IRequestHandler<CreateAdminUserCommand, CreatedAdminUserResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserOperationClaimRepository _userOperationClaimRepository;
        private readonly IOperationClaimRepository _operationClaimRepository;
        private readonly IMapper _mapper;
        private readonly UserBusinessRules _userBusinessRules;

        public CreateAdminUserCommandHandler(
            IUserRepository userRepository,
            IUserOperationClaimRepository userOperationClaimRepository,
            IOperationClaimRepository operationClaimRepository,
            IMapper mapper,
            UserBusinessRules userBusinessRules)
        {
            _userRepository = userRepository;
            _userOperationClaimRepository = userOperationClaimRepository;
            _operationClaimRepository = operationClaimRepository;
            _mapper = mapper;
            _userBusinessRules = userBusinessRules;
        }

        public async Task<CreatedAdminUserResponse> Handle(CreateAdminUserCommand request, CancellationToken cancellationToken)
        {
            await _userBusinessRules.UserEmailShouldNotExistsWhenInsert(request.Email);

            User user = _mapper.Map<User>(request);

            HashingHelper.CreatePasswordHash(
                request.Password,
                passwordHash: out byte[] passwordHash,
                passwordSalt: out byte[] passwordSalt
            );

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            User createdUser = await _userRepository.AddAsync(user);

            // Assign operation claims
            var operationClaims = await _operationClaimRepository.GetListAsync(
                predicate: c => request.OperationClaimIds.Contains(c.Id),
                enableTracking: false,
                cancellationToken: cancellationToken
            );

            var claimNames = new List<string>();
            
            foreach (var claim in operationClaims.Items)
            {
                await _userOperationClaimRepository.AddAsync(new UserOperationClaim
                {
                    Id = Guid.NewGuid(),
                    UserId = createdUser.Id,
                    OperationClaimId = claim.Id
                });
                claimNames.Add(claim.Name);
            }

            // If no claims specified, assign default "Read" claim
            if (operationClaims.Count == 0)
            {
                var defaultClaim = await _operationClaimRepository.GetAsync(c => c.Name == "Users.Read");
                if (defaultClaim != null)
                {
                    await _userOperationClaimRepository.AddAsync(new UserOperationClaim
                    {
                        Id = Guid.NewGuid(),
                        UserId = createdUser.Id,
                        OperationClaimId = defaultClaim.Id
                    });
                    claimNames.Add(defaultClaim.Name);
                }
            }

            CreatedAdminUserResponse response = _mapper.Map<CreatedAdminUserResponse>(createdUser);
            response.OperationClaimNames = claimNames;

            return response;
        }
    }
}
