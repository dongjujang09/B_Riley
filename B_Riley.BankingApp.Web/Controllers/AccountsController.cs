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
using B_Riley.BankingApp.Utils;
using Microsoft.Extensions.Caching.Memory;

namespace B_Riley.BankingApp.Web.Controllers
{
    public class AccountsController : Controller
    {
        private readonly AccountRepository accountRepo;

        public AccountsController(BankingAppContext context, IMemoryCache memoryCache)
        {
            this.accountRepo = new AccountRepository(context, new AppCache(memoryCache));
        }

        // GET: Accounts
        public async Task<IActionResult> Index()
        {
              return View(await accountRepo.GetAllAccountsAsync());
        }

        // GET: Accounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var account = await accountRepo.FindAsync(id.Value);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // GET: Accounts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Accounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AccountName,Balance,Id")] Account account)
        {
            if (ModelState.IsValid)
            {
                await accountRepo.SaveAccountAsync(account);
                return RedirectToAction(nameof(Index));
            }
            return View(account);
        }

        // GET: Accounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var account = await accountRepo.FindAsync(id.Value);
            if (account == null)
            {
                return NotFound();
            }
            return View(account);
        }

        // POST: Accounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AccountName,Balance,DateCreated,DateModified,Id")] Account account)
        {
            if (id != account.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await accountRepo.SaveAccountAsync(account);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountExists(account.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(account);
        }

        // GET: Accounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var account = await accountRepo.FindAsync(id.Value);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // POST: Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var account = await accountRepo.FindAsync(id);
            if (account != null)
            {
                accountRepo.Delete(account);
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool AccountExists(int id)
        {
          return accountRepo.GetQueryable().Any(e => e.Id == id);
        }
    }
}
