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


        public AccountRepository(BankingAppContext context, IAppCache appCache): base(context)
        {
            this.appCache = appCache;
        }

        public override async Task<IEnumerable<Account>> GetAllAsync()
        {
            var accounts = await appCache.GetOrCreateAsync(CACHEKEY_ALLACCOUNTS, () =>
            {
                return GetQueryable().ToListAsync();
            });
            return accounts;
        }

        public override async Task<Account?> FindAsync(int id)
        {
            var cacheKey = string.Format(CACHEKEY_ACCOUNT, id);
            Account? account = null;

            // try to get it from all accounts in cache
            var allAccounts = appCache.Get<IEnumerable<Account>>(CACHEKEY_ALLACCOUNTS);
            if (allAccounts != null)
            {
                account = allAccounts.FirstOrDefault(account => account.Id == id);
                appCache.Set(cacheKey, account);
            }

            if (account == null)
            {
                account = await appCache.GetOrCreateAsync(cacheKey, () =>
                {
                    return Context.Accounts.FirstOrDefaultAsync(account => account.Id == id);
                });
            }
            return account;
        }


        public override async Task<Account?> InsertOrUpdateAsync(Account account)
        {
            if (account== null) throw new ArgumentNullException(nameof(account));

            var now = DateTime.Now;
            account.DateModified = now;
            if (account.Id == 0) // new
            {
                account.DateCreated = now;
                Context.Add(account);                
            }
            else // update
            {
                SetAsUpdate(account);
            }

            await Context.SaveChangesAsync();

            var cacheKey = string.Format(CACHEKEY_ACCOUNT, account.Id);
            appCache.Remove(cacheKey);
            appCache.Remove(CACHEKEY_ALLACCOUNTS);  // remove cache to refresh

            return account;
        }

        public override void Delete(Account account)
        {
            base.Delete(account);

            var cacheKey = string.Format(CACHEKEY_ACCOUNT, account.Id);
            appCache.Remove(cacheKey);
            appCache.Remove(CACHEKEY_ALLACCOUNTS);  // remove cache to refresh
        }
    }
}
