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

        public TransfersController(BankingAppContext context, IAppCache cache)
        {
            this.transferRepo = new TransferRepository(context, cache);
            this.accountRepo = new AccountRepository(context, cache);
        }

        // GET: Transfers
        public async Task<IActionResult> Index()
        {
            return View(await transferRepo.GetAllAsync());
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
            var allAccount = await accountRepo.GetAllAsync();
            ViewData["FromAccountId"] = new SelectList(allAccount, "Id", "AccountName");
            ViewData["ToAccountId"] = new SelectList(allAccount, "Id", "AccountName");
            return View();
        }

        // POST: Transfers/Create
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

                    await transferRepo.InsertOrUpdateAsync(transfer);   // insert transfer
                    
                    await accountRepo.InsertOrUpdateAsync(fromAccount); // update the from-account
                    await accountRepo.InsertOrUpdateAsync(toAccount);   // update the to-account

                    return RedirectToAction(nameof(Index));
                }
                 
                ModelState.AddModelError(nameof(transfer.Amount), "The From Account does not have sufficient funds.");
            }

            var allAccount = await accountRepo.GetAllAsync();
            ViewData["FromAccountId"] = new SelectList(allAccount, "Id", "AccountName", transfer.FromAccountId);
            ViewData["ToAccountId"] = new SelectList(allAccount, "Id", "AccountName", transfer.ToAccountId);
            return View(transfer);
        }
    }
}
