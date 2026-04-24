using System.Collections.Generic;
using Application.Features.DliProducts.Rules;
using Application.Services.Repositories;
using NArchitecture.Core.Persistence.Paging;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Application.Services.DliProducts;

public class DliProductManager : IDliProductService
{
    private readonly IDliProductRepository _dliProductRepository;
    private readonly DliProductBusinessRules _dliProductBusinessRules;

    public DliProductManager(IDliProductRepository dliProductRepository, DliProductBusinessRules dliProductBusinessRules)
    {
        _dliProductRepository = dliProductRepository;
        _dliProductBusinessRules = dliProductBusinessRules;
    }

    public async Task<DliProduct?> GetAsync(
        Expression<Func<DliProduct, bool>> predicate,
        Func<IQueryable<DliProduct>, IIncludableQueryable<DliProduct, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        DliProduct? dliProduct = await _dliProductRepository.GetAsync(predicate, include, withDeleted, enableTracking, cancellationToken);
        return dliProduct;
    }

    public async Task<IPaginate<DliProduct>?> GetListAsync(
        Expression<Func<DliProduct, bool>>? predicate = null,
        Func<IQueryable<DliProduct>, IOrderedQueryable<DliProduct>>? orderBy = null,
        Func<IQueryable<DliProduct>, IIncludableQueryable<DliProduct, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        IPaginate<DliProduct> dliProductList = await _dliProductRepository.GetListAsync(
            predicate,
            orderBy,
            include,
            index,
            size,
            withDeleted,
            enableTracking,
            cancellationToken
        );
        return dliProductList;
    }

    public async Task<DliProduct> AddAsync(DliProduct dliProduct)
    {
        DliProduct addedDliProduct = await _dliProductRepository.AddAsync(dliProduct);

        return addedDliProduct;
    }

    public async Task<ICollection<DliProduct>> AddRangeAsync(ICollection<DliProduct> dliProducts)
    {
        ICollection<DliProduct> addedDliProducts = await _dliProductRepository.AddRangeAsync(dliProducts);

        return addedDliProducts;
    }

    public async Task<DliProduct> UpdateAsync(DliProduct dliProduct)
    {
        DliProduct updatedDliProduct = await _dliProductRepository.UpdateAsync(dliProduct);

        return updatedDliProduct;
    }

    public async Task<ICollection<DliProduct>> UpdateRangeAsync(ICollection<DliProduct> dliProducts)
    {
        ICollection<DliProduct> updatedDliProducts = await _dliProductRepository.UpdateRangeAsync(dliProducts);

        return updatedDliProducts;
    }

    public async Task<DliProduct> DeleteAsync(DliProduct dliProduct, bool permanent = false)
    {
        DliProduct deletedDliProduct = await _dliProductRepository.DeleteAsync(dliProduct);

        return deletedDliProduct;
    }

    public async Task<ICollection<DliProduct>> DeleteRangeAsync(ICollection<DliProduct> dliProducts, bool permanent = false)
    {
        ICollection<DliProduct> deletedDliProducts = await _dliProductRepository.DeleteRangeAsync(dliProducts, permanent);

        return deletedDliProducts;
    }
}
