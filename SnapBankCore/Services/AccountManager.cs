using System;
using System.Linq;
using SnapBank.Core.Helpers;
using SnapBank.Core.Models;
using SnapBank.Core.Models.Operations;
using SnapBank.Core.Repositories;

namespace SnapBankCore.Services
{

    public interface IAccountManager
    {
        string LoginToAccount(string iban, string secret);
        BankAccount GetBankAccountByCredentials(AuthenticatedOperation op);
        Currency GetBalanceByCredentials(AuthenticatedOperation op);
        BankTransaction CashOperation(CashOperation cashOperation);
        BankTransaction InterbankTransfer(TransferOperation transferOperation);
    }

    public class AccountManager : IAccountManager
    {
        private readonly IAccountRepository _accountRepository;

        public AccountManager(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public bool IsIbanReal(string iban)
        {
            return _accountRepository.GetBankAccountByIban(iban) != null;
        }

        public string LoginToAccount(string iban,string secret)
        {
            var account = _accountRepository.GetBankAccountByLogin(iban, secret);
            if (account != null)
            {
                var tkn = Crypto.GetCryptoKey();
                _accountRepository.RegisterTokenForBankAccount(account,tkn);
                return tkn;
            }
            else
            {
                return "";
            }
        }

        public Currency GetBalanceByCredentials(AuthenticatedOperation op)
        {
            var initiaterAccount = _accountRepository.GetBankAccountByLastToken(op.Token);
            if (initiaterAccount != null)
            {
                return this.GetBalanceOfAccount(initiaterAccount);
            }
            else
            {
                throw new Exception("UnAuth");
            }
        }

        public BankTransaction CashOperation(CashOperation cashOperation)
        {
            var initiaterAccount = _accountRepository.GetBankAccountByLastToken(cashOperation.Token);
            if (initiaterAccount != null)
            {
                var isDeposit = cashOperation.OperationType == CashOperationType.Deposit;
                if (!isDeposit && initiaterAccount.Iban != cashOperation.ToIban)
                {
                    throw new Exception("You cant withdraw cash from an other account.");
                }

                if (!IsIbanReal(cashOperation.ToIban))
                {
                    throw new Exception("Iban does not exist in the bank, you can only do inner bank cash transfer.");
                }

                if (cashOperation.OperationType == CashOperationType.Withdraw)
                {
                    var balance = GetBalanceOfAccount(initiaterAccount);
                    if (balance < cashOperation.Money)
                    {
                        throw new Exception("Insuficient fundz.");
                    }
                }

                var fromCashTransaction = new BankTransaction()
                {
                    Id=Guid.NewGuid().ToString(),
                    Message =  isDeposit?"Cash Deposit":"Cash Withdrawal" ,
                    Money = cashOperation.Money,
                    Timestamp = DateTime.UtcNow,
                    Direction = isDeposit?TransactionDirection.Incoming:TransactionDirection.Outgoing,
                    TransactionType = TransactionType.CashDeposit,
                    OtherPartyIban = cashOperation.ToIban
                };
                var recipientAccount = _accountRepository.GetBankAccountByIban(cashOperation.ToIban); 
                _accountRepository.AddTransactionToAccount(recipientAccount, fromCashTransaction);

                return fromCashTransaction;
            }
            else
            {
                throw new Exception("UnAuth");
            }
        }

        public BankTransaction InterbankTransfer(TransferOperation transferOperation)
        {
            var initiaterAccount = _accountRepository.GetBankAccountByLastToken(transferOperation.Token);
            if (initiaterAccount != null)
            {
                if (!IsIbanReal(transferOperation.ToIban))
                {
                    throw new Exception("Iban is not in the bank.");
                }

                var balance = GetBalanceOfAccount(initiaterAccount);
                if (balance < transferOperation.Money)
                {
                    throw new Exception("Insuficient funds.");
                }

                var timeStamp = DateTime.UtcNow;

                var outTransaction = new BankTransaction()
                {
                    Id = Guid.NewGuid().ToString(),
                    Message = transferOperation.Message,
                    Direction = TransactionDirection.Outgoing,
                    Money = transferOperation.Money,
                    OtherPartyIban = transferOperation.ToIban,
                    Timestamp = timeStamp,
                    TransactionType = TransactionType.InterbankTransaction
                };
                var inTransaction = new BankTransaction()
                {
                    Id = Guid.NewGuid().ToString(),
                    Message = transferOperation.Message,
                    Direction = TransactionDirection.Incoming,
                    Money = transferOperation.Money,
                    OtherPartyIban = initiaterAccount.Iban,
                    Timestamp = timeStamp,
                    TransactionType = TransactionType.InterbankTransaction
                };
                var recepient = _accountRepository.GetBankAccountByIban(transferOperation.ToIban);

                _accountRepository.AddTransactionToAccount(initiaterAccount,outTransaction);
                _accountRepository.AddTransactionToAccount(recepient,inTransaction);
                return outTransaction;
            }
            else
            {
                throw new Exception("UnAuth");
            }
        }

        public Currency GetBalanceOfAccount(BankAccount account)
        {
            var balance = account.TransactionHistory.Sum(x =>
                (x.Direction == TransactionDirection.Incoming ? 1 : -1) * x.Money.Amount);
            return new Currency(){Amount = balance};
        }

        public BankAccount GetBankAccountByCredentials(AuthenticatedOperation op)
        {
            var initiaterAccount = _accountRepository.GetBankAccountByLastToken(op.Token);
            return initiaterAccount;
        }
    }
}
