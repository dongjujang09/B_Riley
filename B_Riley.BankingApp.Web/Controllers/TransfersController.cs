using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using B_Riley.BankingApp.Data;
using B_Riley.BankingApp.Models.Entities;
using B_Riley.BankingApp.Data.Repositories;
using Microsoft.Extensions.Caching.Memory;
using B_Riley.BankingApp.Utils;

namespace B_Riley.BankingApp.Web.Controllers
{
    public class TransfersController : Controller
    {
        private readonly TransferRepository transferRepo;
        private readonly AccountRepository accountRepo;

        public TransfersController(BankingAppContext context, IMemoryCache memoryCache)
        {
            this.transferRepo = new TransferRepository(context, new AppCache(memoryCache));
            this.accountRepo = new AccountRepository(context, new AppCache(memoryCache));
        }

        // GET: Transfers
        public async Task<IActionResult> Index()
        {
            return View(await transferRepo.GetAllTransfersAsync());
        }

        // GET: Transfers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var transfer = await transferRepo.FindAsync(id.Value);
            if (transfer == null)
            {
                return NotFound();
            }

            return View(transfer);
        }

        // GET: Transfers/Create
        public async Task<IActionResult> Create()
        {
            var allAccount = await accountRepo.GetAllAccountsAsync();
            ViewData["FromAccountId"] = new SelectList(allAccount, "Id", "AccountName");
            ViewData["ToAccountId"] = new SelectList(allAccount, "Id", "AccountName");
            return View();
        }

        // POST: Transfers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FromAccountId,ToAccountId,Amount,Id")] Transfer transfer)
        {
            if (ModelState.IsValid)
            {
                var fromAccount = await accountRepo.FindAsync(transfer.FromAccountId);
                if (fromAccount.Balance >= transfer.Amount)
                {
                    transfer.FromAccountBalance = fromAccount.Balance;
                    fromAccount.Balance -= transfer.Amount;

                    var toAccount = await accountRepo.FindAsync(transfer.ToAccountId);
                    transfer.ToAccountBalance = toAccount.Balance;
                    toAccount.Balance += transfer.Amount;

                    await transferRepo.SaveTransferAsync(transfer);
                    
                    await accountRepo.SaveAccountAsync(fromAccount);
                    await accountRepo.SaveAccountAsync(toAccount);

                    return RedirectToAction(nameof(Index));
                }
                 
                ModelState.AddModelError(nameof(transfer.Amount), "The From Account does not have sufficient funds.");
            }

            var allAccount = await accountRepo.GetAllAccountsAsync();
            ViewData["FromAccountId"] = new SelectList(allAccount, "Id", "AccountName", transfer.FromAccountId);
            ViewData["ToAccountId"] = new SelectList(allAccount, "Id", "AccountName", transfer.ToAccountId);
            return View(transfer);
        }


        private bool TransferExists(int id)
        {
          return transferRepo.GetQueryable().Any(e => e.Id == id);
        }
    }
}
