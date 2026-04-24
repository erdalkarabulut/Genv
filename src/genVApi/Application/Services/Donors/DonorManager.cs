using System.Collections.Generic;
using Application.Features.Donors.Rules;
using Application.Services.Repositories;
using NArchitecture.Core.Persistence.Paging;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Application.Services.Donors;

public class DonorManager : IDonorService
{
    private readonly IDonorRepository _donorRepository;
    private readonly DonorBusinessRules _donorBusinessRules;

    public DonorManager(IDonorRepository donorRepository, DonorBusinessRules donorBusinessRules)
    {
        _donorRepository = donorRepository;
        _donorBusinessRules = donorBusinessRules;
    }

    public async Task<Donor?> GetAsync(
        Expression<Func<Donor, bool>> predicate,
        Func<IQueryable<Donor>, IIncludableQueryable<Donor, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        Donor? donor = await _donorRepository.GetAsync(predicate, include, withDeleted, enableTracking, cancellationToken);
        return donor;
    }

    public async Task<IPaginate<Donor>?> GetListAsync(
        Expression<Func<Donor, bool>>? predicate = null,
        Func<IQueryable<Donor>, IOrderedQueryable<Donor>>? orderBy = null,
        Func<IQueryable<Donor>, IIncludableQueryable<Donor, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        IPaginate<Donor> donorList = await _donorRepository.GetListAsync(
            predicate,
            orderBy,
            include,
            index,
            size,
            withDeleted,
            enableTracking,
            cancellationToken
        );
        return donorList;
    }

    public async Task<Donor> AddAsync(Donor donor)
    {
        Donor addedDonor = await _donorRepository.AddAsync(donor);

        return addedDonor;
    }

    public async Task<ICollection<Donor>> AddRangeAsync(ICollection<Donor> donors)
    {
        ICollection<Donor> addedDonors = await _donorRepository.AddRangeAsync(donors);

        return addedDonors;
    }

    public async Task<Donor> UpdateAsync(Donor donor)
    {
        Donor updatedDonor = await _donorRepository.UpdateAsync(donor);

        return updatedDonor;
    }

    public async Task<ICollection<Donor>> UpdateRangeAsync(ICollection<Donor> donors)
    {
        ICollection<Donor> updatedDonors = await _donorRepository.UpdateRangeAsync(donors);

        return updatedDonors;
    }

    public async Task<Donor> DeleteAsync(Donor donor, bool permanent = false)
    {
        Donor deletedDonor = await _donorRepository.DeleteAsync(donor);

        return deletedDonor;
    }

    public async Task<ICollection<Donor>> DeleteRangeAsync(ICollection<Donor> donors, bool permanent = false)
    {
        ICollection<Donor> deletedDonors = await _donorRepository.DeleteRangeAsync(donors, permanent);

        return deletedDonors;
    }
}
