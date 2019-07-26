using System;
using System.Linq;
using SnapBank.App.Repositories;
using SnapBank.Core.Models;
using SnapBank.Core.Models.Operations;
using SnapBankCore.Services;

namespace SnapBank.App
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("===WELCOME TO SNAP BANK, CLI VERSION ===");

            var acRepo = new AccountRepository();
            var acManager = new AccountManager(acRepo);

            var credential = "";
            Console.Write("Name : ");
            var iban = Console.ReadLine();
            while (credential == "")
            {
                Console.Write("Please enter your password: ");
                var pasw = Console.ReadLine();
                credential = acManager.LoginToAccount(iban, pasw);
                if (credential == "")
                {
                    Console.WriteLine($"Wrong password, for account :{iban}");
                }
            }
            Console.WriteLine($"Logged in! as :{iban}, cred:{credential}");

            var cmd = "";

            while (true)
            {
                try
                {
                    cmd = Console.ReadLine();
                    var tokens = cmd.Split(" ");
                    var command = tokens[0];
                    var arguments = tokens.ToList().GetRange(1, tokens.Length - 1);

                    if (command == "quit")
                    {
                        break;
                    }

                    if (command == "deposit")
                    {
                        var to = arguments[0];
                        var am = long.Parse(arguments[1]);

                        var depo = new CashOperation
                        {
                            ToIban = to,
                            Money = new Currency() { Amount = am},
                            OperationType = CashOperationType.Deposit,
                            Token = credential
                        };

                        var tr = acManager.CashOperation(depo);
                        Console.WriteLine($"Deposited to: {to}, am:{am}");
                    }

                    if (command == "balance")
                    {
                        var bal = acManager.GetBalanceByCredentials(new AuthenticatedOperation() {Token = credential});
                        Console.WriteLine($"Balance :{bal.Amount} HUF");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"error: {ex.Message}");
                }
            }

        }
    }
}
