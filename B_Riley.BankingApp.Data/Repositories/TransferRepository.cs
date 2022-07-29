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


        public TransferRepository(BankingAppContext context, IAppCache appCache) : base(context)
        {
            this.appCache = appCache;
        }

        public async Task<IEnumerable<Transfer>> GetAllTransfersAsync()
        {
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
                appCache.Set(cacheKey, transfer);
            }

            if (transfer == null)
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


        public async Task SaveTransferAsync(Transfer transfer)
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

            var cacheKey = string.Format(CACHEKEY_TRANSFER, transfer.Id);
            appCache.Remove(cacheKey);
            appCache.Remove(CACHEKEY_ALLTRANSFERS);  // remove cache to refresh
        }

        public override void Delete(Transfer transfer)
        {
            base.Delete(transfer);

            var cacheKey = string.Format(CACHEKEY_TRANSFER, transfer.Id);
            appCache.Remove(cacheKey);
            appCache.Remove(CACHEKEY_ALLTRANSFERS);  // remove cache to refresh
        }
    }
}
