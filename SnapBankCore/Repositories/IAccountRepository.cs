using System;
using System.Collections.Generic;
using System.Text;
using SnapBank.Core.Models;

namespace SnapBank.Core.Repositories
{
    public interface IAccountRepository
    {
        BankAccount GetBankAccountByLogin(string holderName,string secret);
        BankAccount GetBankAccountById(string accountId);
        BankAccount GetBankAccountByIban(string iban);
        BankAccount GetBankAccountByLastToken(string token);
        void AddTransactionToAccount(BankAccount account, BankTransaction transaction);
        void RegisterTokenForBankAccount(BankAccount account, string token);

        void RegisterAccount(BankAccount account, string password);
    }
}
