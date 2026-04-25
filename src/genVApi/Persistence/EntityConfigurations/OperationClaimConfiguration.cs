using Application.Features.Auth.Constants;
using Application.Features.OperationClaims.Constants;
using Application.Features.UserOperationClaims.Constants;
using Application.Features.Users.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Application.Features.EmailAuthenticators.Constants;
using Application.Features.OtpAuthenticators.Constants;
using Application.Features.RefreshTokens.Constants;
using NArchitecture.Core.Security.Constants;
using Application.Features.Bags.Constants;
using Application.Features.BagMovements.Constants;
using Application.Features.Boxes.Constants;
using Application.Features.CollectionSessions.Constants;
using Application.Features.DliProducts.Constants;
using Application.Features.Donors.Constants;
using Application.Features.Patients.Constants;
using Application.Features.Products.Constants;
using Application.Features.Racks.Constants;
using Application.Features.Slots.Constants;
using Application.Features.Tanks.Constants;












namespace Persistence.EntityConfigurations;

public class OperationClaimConfiguration : IEntityTypeConfiguration<OperationClaim>
{
    public void Configure(EntityTypeBuilder<OperationClaim> builder)
    {
        builder.ToTable("OperationClaims").HasKey(oc => oc.Id);

        builder.Property(oc => oc.Id).HasColumnName("Id").IsRequired();
        builder.Property(oc => oc.Name).HasColumnName("Name").IsRequired();
        builder.Property(oc => oc.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(oc => oc.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(oc => oc.DeletedDate).HasColumnName("DeletedDate");

        builder.HasData(_seeds);

        builder.HasBaseType((string)null!);
    }

    public static int AdminId => 1;
    private IEnumerable<OperationClaim> _seeds
    {
        get
        {
            yield return new() { Id = AdminId, Name = GeneralOperationClaims.Admin };

            IEnumerable<OperationClaim> featureOperationClaims = getFeatureOperationClaims(AdminId);
            foreach (OperationClaim claim in featureOperationClaims)
                yield return claim;
        }
    }

#pragma warning disable S1854 // Unused assignments should be removed
    private IEnumerable<OperationClaim> getFeatureOperationClaims(int initialId)
    {
        int lastId = initialId;
        List<OperationClaim> featureOperationClaims = new();

        #region Auth
        featureOperationClaims.AddRange(
            [
                new() { Id = ++lastId, Name = AuthOperationClaims.Admin },
                new() { Id = ++lastId, Name = AuthOperationClaims.Read },
                new() { Id = ++lastId, Name = AuthOperationClaims.Write },
                new() { Id = ++lastId, Name = AuthOperationClaims.RevokeToken },
            ]
        );
        #endregion

        #region OperationClaims
        featureOperationClaims.AddRange(
            [
                new() { Id = ++lastId, Name = OperationClaimsOperationClaims.Admin },
                new() { Id = ++lastId, Name = OperationClaimsOperationClaims.Read },
                new() { Id = ++lastId, Name = OperationClaimsOperationClaims.Write },
                new() { Id = ++lastId, Name = OperationClaimsOperationClaims.Create },
                new() { Id = ++lastId, Name = OperationClaimsOperationClaims.Update },
                new() { Id = ++lastId, Name = OperationClaimsOperationClaims.Delete },
            ]
        );
        #endregion

        #region UserOperationClaims
        featureOperationClaims.AddRange(
            [
                new() { Id = ++lastId, Name = UserOperationClaimsOperationClaims.Admin },
                new() { Id = ++lastId, Name = UserOperationClaimsOperationClaims.Read },
                new() { Id = ++lastId, Name = UserOperationClaimsOperationClaims.Write },
                new() { Id = ++lastId, Name = UserOperationClaimsOperationClaims.Create },
                new() { Id = ++lastId, Name = UserOperationClaimsOperationClaims.Update },
                new() { Id = ++lastId, Name = UserOperationClaimsOperationClaims.Delete },
            ]
        );
        #endregion

        #region Users
        featureOperationClaims.AddRange(
            [
                new() { Id = ++lastId, Name = UsersOperationClaims.Admin },
                new() { Id = ++lastId, Name = UsersOperationClaims.Read },
                new() { Id = ++lastId, Name = UsersOperationClaims.Write },
                new() { Id = ++lastId, Name = UsersOperationClaims.Create },
                new() { Id = ++lastId, Name = UsersOperationClaims.Update },
                new() { Id = ++lastId, Name = UsersOperationClaims.Delete },
            ]
        );
        #endregion

                #region EmailAuthenticators
        featureOperationClaims.AddRange(
            [
                new() { Id = ++lastId, Name = EmailAuthenticatorsOperationClaims.Admin },
                new() { Id = ++lastId, Name = EmailAuthenticatorsOperationClaims.Read },
                new() { Id = ++lastId, Name = EmailAuthenticatorsOperationClaims.Write },
                new() { Id = ++lastId, Name = EmailAuthenticatorsOperationClaims.Create },
                new() { Id = ++lastId, Name = EmailAuthenticatorsOperationClaims.Update },
                new() { Id = ++lastId, Name = EmailAuthenticatorsOperationClaims.Delete },
            ]
        );
        #endregion

        #region OtpAuthenticators
        featureOperationClaims.AddRange(
            [
                new() { Id = ++lastId, Name = OtpAuthenticatorsOperationClaims.Admin },
                new() { Id = ++lastId, Name = OtpAuthenticatorsOperationClaims.Read },
                new() { Id = ++lastId, Name = OtpAuthenticatorsOperationClaims.Write },
                new() { Id = ++lastId, Name = OtpAuthenticatorsOperationClaims.Create },
                new() { Id = ++lastId, Name = OtpAuthenticatorsOperationClaims.Update },
                new() { Id = ++lastId, Name = OtpAuthenticatorsOperationClaims.Delete },
            ]
        );
        #endregion

        #region RefreshTokens
        featureOperationClaims.AddRange(
            [
                new() { Id = ++lastId, Name = RefreshTokensOperationClaims.Admin },
                new() { Id = ++lastId, Name = RefreshTokensOperationClaims.Read },
                new() { Id = ++lastId, Name = RefreshTokensOperationClaims.Write },
                new() { Id = ++lastId, Name = RefreshTokensOperationClaims.Create },
                new() { Id = ++lastId, Name = RefreshTokensOperationClaims.Update },
                new() { Id = ++lastId, Name = RefreshTokensOperationClaims.Delete },
            ]
        );
        #endregion

return featureOperationClaims;

#region Bags
featureOperationClaims.AddRange(
    [
        new() { Id = ++lastId, Name = BagsOperationClaims.Admin },
        new() { Id = ++lastId, Name = BagsOperationClaims.Read },
        new() { Id = ++lastId, Name = BagsOperationClaims.Write },
        new() { Id = ++lastId, Name = BagsOperationClaims.Create },
        new() { Id = ++lastId, Name = BagsOperationClaims.Update },
        new() { Id = ++lastId, Name = BagsOperationClaims.Delete },
        new() { Id = ++lastId, Name = BagsOperationClaims.CreateRange },
        new() { Id = ++lastId, Name = BagsOperationClaims.UpdateRange },
        new() { Id = ++lastId, Name = BagsOperationClaims.DeleteRange },
    ]
);
#endregion


#region BagMovements
featureOperationClaims.AddRange(
    [
        new() { Id = ++lastId, Name = BagMovementsOperationClaims.Admin },
        new() { Id = ++lastId, Name = BagMovementsOperationClaims.Read },
        new() { Id = ++lastId, Name = BagMovementsOperationClaims.Write },
        new() { Id = ++lastId, Name = BagMovementsOperationClaims.Create },
        new() { Id = ++lastId, Name = BagMovementsOperationClaims.Update },
        new() { Id = ++lastId, Name = BagMovementsOperationClaims.Delete },
        new() { Id = ++lastId, Name = BagMovementsOperationClaims.CreateRange },
        new() { Id = ++lastId, Name = BagMovementsOperationClaims.UpdateRange },
        new() { Id = ++lastId, Name = BagMovementsOperationClaims.DeleteRange },
    ]
);
#endregion


#region Boxes
featureOperationClaims.AddRange(
    [
        new() { Id = ++lastId, Name = BoxesOperationClaims.Admin },
        new() { Id = ++lastId, Name = BoxesOperationClaims.Read },
        new() { Id = ++lastId, Name = BoxesOperationClaims.Write },
        new() { Id = ++lastId, Name = BoxesOperationClaims.Create },
        new() { Id = ++lastId, Name = BoxesOperationClaims.Update },
        new() { Id = ++lastId, Name = BoxesOperationClaims.Delete },
        new() { Id = ++lastId, Name = BoxesOperationClaims.CreateRange },
        new() { Id = ++lastId, Name = BoxesOperationClaims.UpdateRange },
        new() { Id = ++lastId, Name = BoxesOperationClaims.DeleteRange },
    ]
);
#endregion


#region CollectionSessions
featureOperationClaims.AddRange(
    [
        new() { Id = ++lastId, Name = CollectionSessionsOperationClaims.Admin },
        new() { Id = ++lastId, Name = CollectionSessionsOperationClaims.Read },
        new() { Id = ++lastId, Name = CollectionSessionsOperationClaims.Write },
        new() { Id = ++lastId, Name = CollectionSessionsOperationClaims.Create },
        new() { Id = ++lastId, Name = CollectionSessionsOperationClaims.Update },
        new() { Id = ++lastId, Name = CollectionSessionsOperationClaims.Delete },
        new() { Id = ++lastId, Name = CollectionSessionsOperationClaims.CreateRange },
        new() { Id = ++lastId, Name = CollectionSessionsOperationClaims.UpdateRange },
        new() { Id = ++lastId, Name = CollectionSessionsOperationClaims.DeleteRange },
    ]
);
#endregion


#region DliProducts
featureOperationClaims.AddRange(
    [
        new() { Id = ++lastId, Name = DliProductsOperationClaims.Admin },
        new() { Id = ++lastId, Name = DliProductsOperationClaims.Read },
        new() { Id = ++lastId, Name = DliProductsOperationClaims.Write },
        new() { Id = ++lastId, Name = DliProductsOperationClaims.Create },
        new() { Id = ++lastId, Name = DliProductsOperationClaims.Update },
        new() { Id = ++lastId, Name = DliProductsOperationClaims.Delete },
        new() { Id = ++lastId, Name = DliProductsOperationClaims.CreateRange },
        new() { Id = ++lastId, Name = DliProductsOperationClaims.UpdateRange },
        new() { Id = ++lastId, Name = DliProductsOperationClaims.DeleteRange },
    ]
);
#endregion


#region Donors
featureOperationClaims.AddRange(
    [
        new() { Id = ++lastId, Name = DonorsOperationClaims.Admin },
        new() { Id = ++lastId, Name = DonorsOperationClaims.Read },
        new() { Id = ++lastId, Name = DonorsOperationClaims.Write },
        new() { Id = ++lastId, Name = DonorsOperationClaims.Create },
        new() { Id = ++lastId, Name = DonorsOperationClaims.Update },
        new() { Id = ++lastId, Name = DonorsOperationClaims.Delete },
        new() { Id = ++lastId, Name = DonorsOperationClaims.CreateRange },
        new() { Id = ++lastId, Name = DonorsOperationClaims.UpdateRange },
        new() { Id = ++lastId, Name = DonorsOperationClaims.DeleteRange },
    ]
);
#endregion


#region Patients
featureOperationClaims.AddRange(
    [
        new() { Id = ++lastId, Name = PatientsOperationClaims.Admin },
        new() { Id = ++lastId, Name = PatientsOperationClaims.Read },
        new() { Id = ++lastId, Name = PatientsOperationClaims.Write },
        new() { Id = ++lastId, Name = PatientsOperationClaims.Create },
        new() { Id = ++lastId, Name = PatientsOperationClaims.Update },
        new() { Id = ++lastId, Name = PatientsOperationClaims.Delete },
        new() { Id = ++lastId, Name = PatientsOperationClaims.CreateRange },
        new() { Id = ++lastId, Name = PatientsOperationClaims.UpdateRange },
        new() { Id = ++lastId, Name = PatientsOperationClaims.DeleteRange },
    ]
);
#endregion


#region Products
featureOperationClaims.AddRange(
    [
        new() { Id = ++lastId, Name = ProductsOperationClaims.Admin },
        new() { Id = ++lastId, Name = ProductsOperationClaims.Read },
        new() { Id = ++lastId, Name = ProductsOperationClaims.Write },
        new() { Id = ++lastId, Name = ProductsOperationClaims.Create },
        new() { Id = ++lastId, Name = ProductsOperationClaims.Update },
        new() { Id = ++lastId, Name = ProductsOperationClaims.Delete },
        new() { Id = ++lastId, Name = ProductsOperationClaims.CreateRange },
        new() { Id = ++lastId, Name = ProductsOperationClaims.UpdateRange },
        new() { Id = ++lastId, Name = ProductsOperationClaims.DeleteRange },
    ]
);
#endregion


#region Racks
featureOperationClaims.AddRange(
    [
        new() { Id = ++lastId, Name = RacksOperationClaims.Admin },
        new() { Id = ++lastId, Name = RacksOperationClaims.Read },
        new() { Id = ++lastId, Name = RacksOperationClaims.Write },
        new() { Id = ++lastId, Name = RacksOperationClaims.Create },
        new() { Id = ++lastId, Name = RacksOperationClaims.Update },
        new() { Id = ++lastId, Name = RacksOperationClaims.Delete },
        new() { Id = ++lastId, Name = RacksOperationClaims.CreateRange },
        new() { Id = ++lastId, Name = RacksOperationClaims.UpdateRange },
        new() { Id = ++lastId, Name = RacksOperationClaims.DeleteRange },
    ]
);
#endregion


#region Slots
featureOperationClaims.AddRange(
    [
        new() { Id = ++lastId, Name = SlotsOperationClaims.Admin },
        new() { Id = ++lastId, Name = SlotsOperationClaims.Read },
        new() { Id = ++lastId, Name = SlotsOperationClaims.Write },
        new() { Id = ++lastId, Name = SlotsOperationClaims.Create },
        new() { Id = ++lastId, Name = SlotsOperationClaims.Update },
        new() { Id = ++lastId, Name = SlotsOperationClaims.Delete },
        new() { Id = ++lastId, Name = SlotsOperationClaims.CreateRange },
        new() { Id = ++lastId, Name = SlotsOperationClaims.UpdateRange },
        new() { Id = ++lastId, Name = SlotsOperationClaims.DeleteRange },
    ]
);
#endregion


#region Tanks
featureOperationClaims.AddRange(
    [
        new() { Id = ++lastId, Name = TanksOperationClaims.Admin },
        new() { Id = ++lastId, Name = TanksOperationClaims.Read },
        new() { Id = ++lastId, Name = TanksOperationClaims.Write },
        new() { Id = ++lastId, Name = TanksOperationClaims.Create },
        new() { Id = ++lastId, Name = TanksOperationClaims.Update },
        new() { Id = ++lastId, Name = TanksOperationClaims.Delete },
        new() { Id = ++lastId, Name = TanksOperationClaims.CreateRange },
        new() { Id = ++lastId, Name = TanksOperationClaims.UpdateRange },
        new() { Id = ++lastId, Name = TanksOperationClaims.DeleteRange },
    ]
);
#endregion

    }
#pragma warning restore S1854 // Unused assignments should be removed
}
