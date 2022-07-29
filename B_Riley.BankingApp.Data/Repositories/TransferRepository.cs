using B_Riley.BankingApp.Models.Entities;
using B_Riley.BankingApp.Utils;
using Microsoft.EntityFrameworkCore;

namespace B_Riley.BankingApp.Data.Repositories
{
    public class TransferRepository: BaseRepository<Transfer>
    {
        private const string CACHEKEY_ALLTRANSFERS = "AllTransfers";
        private const string CACHEKEY_TRANSFER = "Transfer_{0}";

        private readonly IAppCache appCache;


        public TransferRepository(BankingAppContext context, IAppCache appCache) 
            : base(context)
        {
            this.appCache = appCache;
        }

        public override async Task<IEnumerable<Transfer>> GetAllAsync()
        {
            // try to get data from cache. if not, call database
            var accounts = await appCache.GetOrCreateAsync(CACHEKEY_ALLTRANSFERS, () =>
            {
                return GetQueryable()
                    .Include(transfer => transfer.FromAccount)
                    .Include(transfer => transfer.ToAccount)
                    .ToListAsync();
            });
            return accounts;
        }

        public override async Task<Transfer?> FindAsync(int id)
        {
            var cacheKey = string.Format(CACHEKEY_TRANSFER, id);
            Transfer? transfer = null;

            // try to get it from all transfers in cache
            var allTransfers = appCache.Get<IEnumerable<Transfer>>(CACHEKEY_ALLTRANSFERS);
            if (allTransfers != null)
            {
                transfer = allTransfers.FirstOrDefault(transfer => transfer.Id == id);
            }

            // store it in cache then return
            if (transfer != null)
            {
                appCache.Set(cacheKey, transfer);
            }
            else
            {
                transfer = await appCache.GetOrCreateAsync(cacheKey, () =>
                {
                    return Context.Transfers
                        .Include(transfer => transfer.FromAccount)
                        .Include(transfer => transfer.ToAccount)
                        .FirstOrDefaultAsync(transfer => transfer.Id == id);
                });
            }
            return transfer;
        }


        public override async Task<Transfer?> InsertOrUpdateAsync(Transfer transfer)
        {
            if (transfer == null) throw new ArgumentNullException(nameof(transfer));

            if (transfer.Id == 0) // new
            {
                transfer.TransactionTime = DateTime.Now;
                Context.Add(transfer);
            }
            else // update
            {
                SetAsUpdate(transfer);
            }

            await Context.SaveChangesAsync();

            // remove cache to refresh
            var cacheKey = string.Format(CACHEKEY_TRANSFER, transfer.Id);
            appCache.Remove(cacheKey);
            appCache.Remove(CACHEKEY_ALLTRANSFERS);

            return transfer;
        }

        public override void Delete(Transfer transfer)
        {
            if (transfer == null) throw new ArgumentNullException(nameof(transfer));
            if (transfer.Id < 0) throw new ArgumentOutOfRangeException(nameof(transfer.Id));

            base.Delete(transfer);

            // remove cache to refresh
            var cacheKey = string.Format(CACHEKEY_TRANSFER, transfer.Id);
            appCache.Remove(cacheKey);
            appCache.Remove(CACHEKEY_ALLTRANSFERS);
        }
    }
}
