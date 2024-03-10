﻿using AuthorPlace.Models.Entities;
using AuthorPlace.Models.Exceptions.Application;
using AuthorPlace.Models.Extensions;
using AuthorPlace.Models.Services.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Security.Claims;

namespace AuthorPlace.Models.Services.Infrastructure.Implementations;

public class AdoNetUserStore : IUserStore<ApplicationUser>, IUserClaimStore<ApplicationUser>, IUserEmailStore<ApplicationUser>, IUserPasswordStore<ApplicationUser>, IUserPhoneNumberStore<ApplicationUser>, IUserSecurityStampStore<ApplicationUser>, IUserTwoFactorStore<ApplicationUser>, IUserTwoFactorRecoveryCodeStore<ApplicationUser>, IUserAuthenticatorKeyStore<ApplicationUser>, IUserAuthenticationTokenStore<ApplicationUser>, IUserLockoutStore<ApplicationUser>, IUserLoginStore<ApplicationUser>, IUserConfirmation<ApplicationUser>
{
    private readonly IDatabaseAccessor databaseAccessor;

    public AdoNetUserStore(IDatabaseAccessor databaseAccessor)
    {
        this.databaseAccessor = databaseAccessor;
    }

    public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken token)
    {
        int affectedRows = await databaseAccessor.CommandAsync($"INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, FullName) VALUES ({user.Id}, {user.UserName}, {user.NormalizedUserName}, {user.Email}, {user.NormalizedEmail}, {user.EmailConfirmed}, {user.PasswordHash}, {user.SecurityStamp}, {user.ConcurrencyStamp}, {user.PhoneNumber}, {user.PhoneNumberConfirmed}, {user.TwoFactorEnabled}, {user.LockoutEnd}, {user.LockoutEnabled}, {user.AccessFailedCount}, {user.FullName})", token);
        if (affectedRows > 0)
        {
            return IdentityResult.Success;
        }
        IdentityError error = new() { Description = "Unable to insert user" };
        return IdentityResult.Failed(error);
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken token)
    {
        int affectedRows = await databaseAccessor.CommandAsync($"UPDATE AspNetUsers SET UserName={user.UserName}, NormalizedUserName={user.NormalizedUserName}, Email={user.Email}, NormalizedEmail={user.NormalizedEmail}, EmailConfirmed={user.EmailConfirmed}, PasswordHash={user.PasswordHash}, SecurityStamp={user.SecurityStamp}, ConcurrencyStamp={user.ConcurrencyStamp}, PhoneNumber={user.PhoneNumber}, PhoneNumberConfirmed={user.PhoneNumberConfirmed}, TwoFactorEnabled={user.TwoFactorEnabled}, LockoutEnd={user.LockoutEnd}, LockoutEnabled={user.LockoutEnabled}, AccessFailedCount={user.AccessFailedCount}, FullName={user.FullName} WHERE Id={user.Id}", token);
        if (affectedRows > 0)
        {
            return IdentityResult.Success;
        }
        IdentityError error = new() { Description = "Unable to update user" };
        return IdentityResult.Failed(error);
    }

    public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken token)
    {
        int affectedRows = await databaseAccessor.CommandAsync($"DELETE FROM AspNetUsers WHERE Id={user.Id}", token);
        if (affectedRows > 0)
        {
            return IdentityResult.Success;
        }
        IdentityError error = new() { Description = "Unable to delete user" };
        return IdentityResult.Failed(error);
    }

    public async Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken token)
    {
        DataSet dataSet = await databaseAccessor.QueryAsync($"SELECT * FROM AspNetUsers WHERE Id={userId}", token);
        DataTable dataTable = dataSet.Tables[0];
        if (dataTable.Rows.Count == 0)
        {
            throw new UserNotFoundException();
        }
        DataRow dataRow = dataTable.Rows[0];
        return dataRow.ToApplicationUser();
    }

    public async Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken token)
    {
        DataSet dataSet = await databaseAccessor.QueryAsync($"SELECT * FROM AspNetUsers WHERE NormalizedUserName={normalizedUserName}", token);
        DataTable dataTable = dataSet.Tables[0];
        if (dataTable.Rows.Count == 0)
        {
            throw new UserNotFoundException();
        }
        DataRow dataRow = dataTable.Rows[0];
        return dataRow.ToApplicationUser();
    }

    public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken token)
    {
        return Task.FromResult(user.NormalizedUserName);
    }

    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken token)
    {
        return Task.FromResult(user.Id);
    }

    public Task<string> GetUserNameAsync(ApplicationUser user, CancellationToken token)
    {
        return Task.FromResult(user.UserName);
    }

    public Task SetNormalizedUserNameAsync(ApplicationUser user, string normalizedName, CancellationToken token)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(ApplicationUser user, string userName, CancellationToken token)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user, CancellationToken token)
    {
        DataSet dataSet = await databaseAccessor.QueryAsync($"SELECT * FROM AspNetUserClaims WHERE UserId={user.Id}", token);
        List<Claim> claims = dataSet.Tables[0].AsEnumerable().Select(row => new Claim(
            type: Convert.ToString(row["ClaimType"])!,
            value: Convert.ToString(row["ClaimValue"])!
        )).ToList();
        return claims;
    }

    public async Task AddClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken token)
    {
        foreach (Claim claim in claims)
        {
            int affectedRows = await databaseAccessor.CommandAsync($"INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue) VALUES ({user.Id}, {claim.Type}, {claim.Value})", token);
            if (affectedRows == 0)
            {
                throw new InvalidOperationException("Couldn't add the claim");
            }
        }
    }

    public async Task ReplaceClaimAsync(ApplicationUser user, Claim claim, Claim newClaim, CancellationToken token)
    {
        await RemoveClaimsAsync(user, new[] { claim }, token);
        await AddClaimsAsync(user, new[] { newClaim }, token);
    }

    public async Task RemoveClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken token)
    {
        foreach (Claim claim in claims)
        {
            int affectedRows = await databaseAccessor.CommandAsync($"DELETE FROM AspNetUserClaims WHERE UserId={user.Id} AND ClaimType={claim.Type} AND ClaimValue={claim.Value}", token);
            if (affectedRows == 0)
            {
                throw new InvalidOperationException("Couldn't remove the claim");
            }
        }
    }

    public async Task<IList<ApplicationUser>> GetUsersForClaimAsync(Claim claim, CancellationToken token)
    {
        DataSet dataSet = await databaseAccessor.QueryAsync($"SELECT AspNetUsers.* FROM AspNetUserClaims INNER JOIN AspNetUsers ON AspNetUserClaims.UserId = AspNetUsers.Id WHERE AspNetUserClaims.ClaimType={claim.Type} AND AspNetUserClaims.ClaimValue={claim.Value}", token);
        List<ApplicationUser> users = dataSet.Tables[0].AsEnumerable().Select(dataRow => dataRow.ToApplicationUser()).ToList();
        return users;
    }

    public async Task<ApplicationUser> FindByEmailAsync(string normalizedEmail, CancellationToken token)
    {
        DataSet dataSet = await databaseAccessor.QueryAsync($"SELECT * FROM AspNetUsers WHERE NormalizedEmail={normalizedEmail}", token);
        DataTable dataTable = dataSet.Tables[0];
        if (dataTable.Rows.Count == 0)
        {
            throw new UserNotFoundException();
        }
        DataRow dataRow = dataTable.Rows[0];
        return dataRow.ToApplicationUser();
    }

    public Task SetEmailAsync(ApplicationUser user, string email, CancellationToken token)
    {
        user.Email = email;
        return Task.CompletedTask;
    }

    public Task<string> GetEmailAsync(ApplicationUser user, CancellationToken token)
    {
        return Task.FromResult(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken token)
    {
        return Task.FromResult(user.EmailConfirmed);
    }

    public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken token)
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public Task<string> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken token)
    {
        return Task.FromResult(user.NormalizedEmail);
    }

    public Task SetNormalizedEmailAsync(ApplicationUser user, string normalizedEmail, CancellationToken token)
    {
        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask;
    }

    public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash, CancellationToken token)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string> GetPasswordHashAsync(ApplicationUser user, CancellationToken token)
    {
        return Task.FromResult(user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken token)
    {
        bool hasPassword = user.PasswordHash != null;
        return Task.FromResult(hasPassword);
    }

    public Task SetPhoneNumberAsync(ApplicationUser user, string phoneNumber, CancellationToken token)
    {
        user.PhoneNumber = phoneNumber;
        return Task.CompletedTask;
    }

    public Task<string> GetPhoneNumberAsync(ApplicationUser user, CancellationToken token)
    {
        return Task.FromResult(user.PhoneNumber);
    }

    public Task<bool> GetPhoneNumberConfirmedAsync(ApplicationUser user, CancellationToken token)
    {
        return Task.FromResult(user.PhoneNumberConfirmed);
    }

    public Task SetPhoneNumberConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken token)
    {
        user.PhoneNumberConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public Task SetSecurityStampAsync(ApplicationUser user, string stamp, CancellationToken token)
    {
        user.SecurityStamp = stamp;
        return Task.CompletedTask;
    }

    public Task<string> GetSecurityStampAsync(ApplicationUser user, CancellationToken token)
    {
        return Task.FromResult(user.SecurityStamp);
    }

    public Task SetTwoFactorEnabledAsync(ApplicationUser user, bool enabled, CancellationToken token)
    {
        user.TwoFactorEnabled = enabled;
        return Task.CompletedTask;
    }

    public Task<bool> GetTwoFactorEnabledAsync(ApplicationUser user, CancellationToken token)
    {
        return Task.FromResult(user.TwoFactorEnabled);
    }

    const string loginProviderName = "[AspNetUserStore]";
    const string authenticatorKeyTokenName = "AuthenticatorKey";
    const string recoveryCodesTokenName = "RecoveryCodes";

    public Task SetAuthenticatorKeyAsync(ApplicationUser user, string key, CancellationToken token)
    {
        return SetTokenAsync(user, loginProviderName, authenticatorKeyTokenName, key, token);
    }

    public Task<string> GetAuthenticatorKeyAsync(ApplicationUser user, CancellationToken token)
    {
        return GetTokenAsync(user, loginProviderName, authenticatorKeyTokenName, token);
    }

    public Task ReplaceCodesAsync(ApplicationUser user, IEnumerable<string> recoveryCodes, CancellationToken token)
    {
        string codesValue = string.Join(";", recoveryCodes);
        return SetTokenAsync(user, loginProviderName, recoveryCodesTokenName, codesValue, token);
    }

    public async Task<bool> RedeemCodeAsync(ApplicationUser user, string code, CancellationToken token)
    {
        string codesValue = await GetTokenAsync(user, loginProviderName, recoveryCodesTokenName, token);
        if (string.IsNullOrEmpty(codesValue))
        {
            return false;
        }
        List<string> codes = codesValue.Split(';').ToList();
        if (!codes.Remove(code))
        {
            return false;
        }
        await ReplaceCodesAsync(user, codes, token);
        return true;
    }

    public async Task<int> CountCodesAsync(ApplicationUser user, CancellationToken token)
    {
        string codesValue = await GetTokenAsync(user, loginProviderName, recoveryCodesTokenName, token);
        if (string.IsNullOrEmpty(codesValue))
        {
            return 0;
        }
        string[] codes = codesValue.Split(';');
        return codes.Length;
    }

    public async Task SetTokenAsync(ApplicationUser user, string loginProvider, string name, string value, CancellationToken token)
    {
        int affectedRows = await databaseAccessor.CommandAsync($"REPLACE INTO AspNetUserTokens (UserId, LoginProvider, Name, Value) VALUES ({user.Id}, {loginProvider}, {name}, {value})", token);
        if (affectedRows == 0)
        {
            throw new InvalidOperationException($"Couldn't set token '{name}' for login provider '{loginProvider}");
        }
    }

    public async Task RemoveTokenAsync(ApplicationUser user, string loginProvider, string name, CancellationToken token)
    {
        int affectedRows = await databaseAccessor.CommandAsync($"DELETE FROM AspNetUserTokens WHERE UserId={user.Id} AND LoginProvider={loginProvider} AND Name={name}", token);
        if (affectedRows == 0)
        {
            throw new InvalidOperationException($"Couldn't remove token '{name}' for login provider '{loginProvider}'");
        }
    }

    public Task<string> GetTokenAsync(ApplicationUser user, string loginProvider, string name, CancellationToken token)
    {
        return databaseAccessor.ScalarAsync<string>($"SELECT Value FROM AspNetUserTokens WHERE UserId={user.Id} AND LoginProvider={loginProvider} AND Name={name}", token);
    }

    public async Task AddLoginAsync(ApplicationUser user, UserLoginInfo login, CancellationToken token)
    {
        int affectedRows = await databaseAccessor.CommandAsync($"INSERT INTO AspNetUserLogins (UserId, LoginProvider, ProviderKey, ProviderDisplayName) VALUES ({user.Id}, {login.LoginProvider}, {login.ProviderKey}, {login.ProviderDisplayName})", token);
        if (affectedRows == 0)
        {
            throw new InvalidOperationException("Couldn't add a login");
        }
    }

    public async Task RemoveLoginAsync(ApplicationUser user, string loginProvider, string providerKey, CancellationToken token)
    {
        int affectedRows = await databaseAccessor.CommandAsync($"DELETE FROM AspNetUserLogins WHERE UserId={user.Id} AND LoginProvider={loginProvider} AND ProviderKey={providerKey}", token);
        if (affectedRows == 0)
        {
            throw new InvalidOperationException("Couldn't remove a login");
        }
    }

    public async Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user, CancellationToken token)
    {
        DataSet dataSet = await databaseAccessor.QueryAsync($"SELECT * FROM AspNetUserLogins WHERE UserId={user.Id}", token);
        List<UserLoginInfo> userLogins = dataSet.Tables[0].AsEnumerable().Select(row => new UserLoginInfo(
            providerKey: Convert.ToString(row["ProviderKey"]),
            loginProvider: Convert.ToString(row["LoginProvider"]),
            displayName: Convert.ToString(row["ProviderDisplayName"])
        )).ToList();
        return userLogins;
    }

    public async Task<ApplicationUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken token)
    {
        DataSet dataSet = await databaseAccessor.QueryAsync($"SELECT AspNetUsers.* FROM AspNetUsers LEFT JOIN AspNetUserLogins ON AspNetUsers.Id=AspNetUserLogins.UserId WHERE LoginProvider={loginProvider} AND ProviderKey={providerKey}", token);
        DataTable dataTable = dataSet.Tables[0];
        if (dataTable.Rows.Count == 0)
        {
            throw new UserNotFoundException();
        }
        DataRow dataRow = dataTable.Rows[0];
        return dataRow.ToApplicationUser();
    }

    public Task<DateTimeOffset?> GetLockoutEndDateAsync(ApplicationUser user, CancellationToken token)
    {
        return Task.FromResult(user.LockoutEnd);
    }

    public Task SetLockoutEndDateAsync(ApplicationUser user, DateTimeOffset? lockoutEnd, CancellationToken token)
    {
        user.LockoutEnd = lockoutEnd;
        return Task.CompletedTask;
    }

    public Task<int> IncrementAccessFailedCountAsync(ApplicationUser user, CancellationToken token)
    {
        user.AccessFailedCount++;
        return Task.FromResult(user.AccessFailedCount);
    }

    public Task ResetAccessFailedCountAsync(ApplicationUser user, CancellationToken token)
    {
        user.AccessFailedCount = 0;
        return Task.CompletedTask;
    }

    public Task<int> GetAccessFailedCountAsync(ApplicationUser user, CancellationToken token)
    {
        return Task.FromResult(user.AccessFailedCount);
    }

    public Task<bool> GetLockoutEnabledAsync(ApplicationUser user, CancellationToken token)
    {
        return Task.FromResult(user.LockoutEnabled);
    }

    public Task SetLockoutEnabledAsync(ApplicationUser user, bool enabled, CancellationToken token)
    {
        user.LockoutEnabled = enabled;
        return Task.CompletedTask;
    }

    public Task<bool> IsConfirmedAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
    {
        return Task.FromResult(user.EmailConfirmed);
    }
}
