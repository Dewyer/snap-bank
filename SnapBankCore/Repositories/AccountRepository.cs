using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;
using SnapBank.Core.Helpers;
using SnapBank.Core.Models;
using SnapBank.Core.Repositories;

namespace SnapBank.App.Repositories
{
    
    public class AccountRepository : IAccountRepository
    {
        private MongoClient _client;
        private IMongoDatabase _database;
        private IMongoCollection<BankAccount> _accountsCollection;

        public AccountRepository()
        {
            var con = "mongodb+srv://root:rooter@cluster0-m3otq.mongodb.net/test?retryWrites=true&w=majority&ssl=true";
            _client = new MongoClient(con); // random atlas mongodb
            var databases = _client.ListDatabases();
            _database = _client.GetDatabase("snap-bank");
            _accountsCollection = _database.GetCollection<BankAccount>("BankAccounts");

            Seed();
        }

        public void Seed()
        {
            if (this.GetBankAccountByIban("123456789") == null)
            {
                this.RegisterAccount(new BankAccount()
                {
                    Id= Guid.NewGuid().ToString(),
                    HolderName = "Test Joska",
                    Iban = "123456789",
                    TransactionHistory = new List<BankTransaction>()
                },"Asd1234");
            }
            if (this.GetBankAccountByIban("0000") == null)
            {
                this.RegisterAccount(new BankAccount()
                {
                    Id = Guid.NewGuid().ToString(),
                    HolderName = "Test bandi",
                    Iban = "0000",
                    TransactionHistory = new List<BankTransaction>()
                }, "Lol1234");
            }
        }

        public BankAccount GetBankAccountByLogin(string iban, string secret)
        {
            var ac = GetBankAccountByIban(iban);
            if (ac.Credentials.SecretLogin == Crypto.GetSecretHash(secret, ac.Credentials.SecretSalt))
            {
                return ac;
            }

            return null;
        }

        public BankAccount GetBankAccountById(string accountId)
        {
            return _accountsCollection.Find(Builders<BankAccount>.Filter.Eq("Id", accountId)).FirstOrDefault();
        }

        public BankAccount GetBankAccountByIban(string iban)
        {
            return _accountsCollection.Find(Builders<BankAccount>.Filter.Eq("Iban",iban)).FirstOrDefault();
        }

        public BankAccount GetBankAccountByLastToken(string token)
        {
            var ac = _accountsCollection.Find(Builders<BankAccount>.Filter.Where(x =>
                x.Credentials.LastOperationalToken == token));

            return ac.FirstOrDefault();
        }

        public void AddTransactionToAccount(BankAccount account, BankTransaction transaction)
        {
            var newT = new List<BankTransaction>();
            newT.AddRange(account.TransactionHistory);
            newT.Add(transaction);
            _accountsCollection.UpdateOne(Builders<BankAccount>.Filter.Eq("Id", account.Id),
                Builders<BankAccount>.Update.Set("TransactionHistory", newT));
        }

        public void RegisterTokenForBankAccount(BankAccount account, string token)
        {
            account.Credentials.LastOperationalToken = token;
            _accountsCollection.ReplaceOne(Builders<BankAccount>.Filter.Eq("Id", account.Id),account);
        }

        public void RegisterAccount(BankAccount account,string password)
        {
            var salt = Crypto.GetCryptoKey();
            var secret = Crypto.GetSecretHash(password,salt);
            account.Credentials = new BankAccountCredentials()
            {
                LastOperationalToken = "",
                SecretSalt = salt,
                SecretLogin = secret,
                TokenExpiration = DateTime.MaxValue
            };
            _accountsCollection.InsertOne(account);
        }
    }
    
}
