using B_Riley.BankingApp.Models.Entities;
using B_Riley.BankingApp.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_Riley.BankingApp.Data.Repositories
{
    public class AccountRepository: BaseRepository<Account>
    {
        private const string CACHEKEY_ALLACCOUNTS = "AllAccounts";
        private const string CACHEKEY_ACCOUNT = "Account_{0}";

        private readonly IAppCache appCache;


        public AccountRepository(BankingAppContext context, IAppCache appCache)
            : base(context)
        {
            this.appCache = appCache;
        }

        public override async Task<IEnumerable<Account>> GetAllAsync()
        {
            // try to get data from cache. if not, call database
            var accounts = await appCache.GetOrCreateAsync(CACHEKEY_ALLACCOUNTS, () =>
            {
                return GetQueryable().Where(account => !account.IsDeleted).ToListAsync();
            });
            return accounts;
        }

        public override async Task<Account?> FindAsync(int id)
        {
            if (id < 0) throw new ArgumentOutOfRangeException(nameof(id));

            var cacheKey = string.Format(CACHEKEY_ACCOUNT, id);
            Account? account = null;

            // try to get it from all accounts in cache
            var allAccounts = appCache.Get<IEnumerable<Account>>(CACHEKEY_ALLACCOUNTS);
            if (allAccounts != null)
            {
                account = allAccounts.FirstOrDefault(account => account.Id == id);
            }

            // store it in cache then return
            if (account != null)
            {
                appCache.Set(cacheKey, account);
            }
            else
            {
                account = await appCache.GetOrCreateAsync(cacheKey, () =>
                {
                    return Context.Accounts.FirstOrDefaultAsync(account => !account.IsDeleted && account.Id == id);
                });
            }
            return account;
        }


        public override async Task<Account?> InsertOrUpdateAsync(Account account)
        {
            if (account== null) throw new ArgumentNullException(nameof(account));

            var now = DateTime.Now;
            account.DateModified = now;
            account.IsDeleted = false;

            if (account.Id == 0) // new
            {
                account.DateCreated = now;
                Context.Add(account);                
            }
            else // update
            {
                SetAsUpdate(account);
            }

            await SaveChangesAsync();

            var cacheKey = string.Format(CACHEKEY_ACCOUNT, account.Id);
            appCache.Remove(cacheKey);
            appCache.Remove(CACHEKEY_ALLACCOUNTS);  // remove cache to refresh

            return account;
        }

        public async Task MarkAsDeletedAsync(Account account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));
            if (account.Id < 0) throw new ArgumentOutOfRangeException(nameof(account.Id));

            account.IsDeleted = true;
            SetAsUpdate(account);
            await SaveChangesAsync();

            var cacheKey = string.Format(CACHEKEY_ACCOUNT, account.Id);
            appCache.Remove(cacheKey);
            appCache.Remove(CACHEKEY_ALLACCOUNTS);  // remove cache to refresh
        }
    }
}
