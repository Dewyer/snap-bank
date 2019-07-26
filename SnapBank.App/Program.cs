using System;
using System.Linq;
using SnapBank.App.Repositories;
using SnapBank.Core.Models;
using SnapBank.Core.Models.Operations;
using SnapBank.Core.Repositories;
using SnapBankCore.Services;

namespace SnapBank.App
{
    class Program
    {
        public static IAccountManager acManager;
        public static IAccountRepository acRepo;

        static void Main(string[] args)
        {
            Console.WriteLine("===WELCOME TO SNAP BANK, CLI VERSION BANK TELLER USE ONLY BECAUSE DB IS UNSAFE===");
            Console.WriteLine("");
            Console.WriteLine("Controlls:");
            Console.WriteLine("balance - Shows current balance");
            Console.WriteLine("history <date?> <in|out?>");
            Console.WriteLine("transfer <to> <amount>");
            Console.WriteLine("deposit <to> <amount>");
            Console.WriteLine("withdraw <amount>");
            Console.WriteLine();
            var art =
                "            /\\\r\n        .nNNNNNb. _______ \r\n       dN(o)NNNNNNNNN\"NNNNNNb.                     \r\n     dNNNNNNNNNNNNNP\" \"\"NNNNNNb _                             \r\n     YNNNN\"NNNNNNNN N NNNNNNNNN( )                                \r\n       \"\"'dNNNNNNNNb. \"YNNNNNNN_X_\r\n        \"YNNNNNNNNNNN N NNNNNNN\r\n            YNNNNNN.. .dNNNNNNP\r\n             \"YNNNNNN.NNNNNNP\"\r\n                NN \"\"\"\"\" NN\r\n                nn       nn";
            Console.WriteLine(art);

            acRepo = new AccountRepository();
            acManager = new AccountManager(acRepo);

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

            StartCommandLoop(credential);
        }

        public static bool MatchDirection(TransactionDirection d1, TransactionDirection d2)
        {
            return d1 == TransactionDirection.Any || (d2 == TransactionDirection.Any || (d1 == d2));
        }

        public static void PrintHistory(string credential,DateTime filterDate,TransactionDirection direction=TransactionDirection.Any)
        {
            var meRecent =
                acManager.GetBankAccountByCredentials(new AuthenticatedOperation() { Token = credential });
            var txes = meRecent.TransactionHistory.Where(x=>x.Timestamp>=filterDate && MatchDirection(direction,x.Direction)).OrderBy(x => DateTime.MaxValue - x.Timestamp).ToList();
            var bdy = "Transaction history: \n";
            bdy += "Time | Message | Amount | Balance Then | Recipient?\n";
            foreach (var tx in txes)
            {
                var balanceThen = meRecent.TransactionHistory.Where(x => x.Timestamp <= tx.Timestamp).Sum(x =>
                    x.Money.Amount * (x.Direction == TransactionDirection.Incoming ? 1 : -1));

                var dir = tx.Direction == TransactionDirection.Incoming ? "" : "-";
                var sentToPart = tx.Direction == TransactionDirection.Outgoing && tx.OtherPartyIban != meRecent.Iban
                    ? $" | Sent to :{tx.OtherPartyIban}"
                    : "";

                bdy += $"{tx.Timestamp.ToLocalTime().ToString("u")} | {tx.Message} | {dir}{tx.Money.Amount} HUF | {balanceThen} HUF {sentToPart} \n";
            }

            Console.WriteLine(bdy);

        }

        public static void StartCommandLoop(string credential)
        {
            var me = acManager.GetBankAccountByCredentials(new AuthenticatedOperation() { Token = credential });
            Console.WriteLine($"Logged in! as :{me.HolderName}, cred:{credential}");
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
                            Money = new Currency() { Amount = am },
                            OperationType = CashOperationType.Deposit,
                            Token = credential
                        };

                        var tr = acManager.CashOperation(depo);
                        Console.WriteLine($"Deposited to: {to}, am:{am}");
                    }

                    if (command == "withdraw")
                    {
                        var to = me.Iban;
                        var am = long.Parse(arguments[0]);

                        var with = new CashOperation
                        {
                            ToIban = to,
                            Money = new Currency() { Amount = am },
                            OperationType = CashOperationType.Withdraw,
                            Token = credential
                        };

                        var tr = acManager.CashOperation(with);
                        Console.WriteLine($"Withdrawn am:{am}");
                    }

                    if (command == "transfer")
                    {
                        var to = arguments[0];
                        var am = long.Parse(arguments[1]);
                        var msg = arguments[2];

                        var tx = new TransferOperation()
                        {
                            ToIban = to,
                            Money = new Currency() { Amount = am },
                            Message = msg,
                            Token = credential
                        };

                        var tr = acManager.InterbankTransfer(tx);
                        Console.WriteLine($"Transfered to: {to}, am:{am} | {msg}");
                    }

                    if (command == "balance")
                    {
                        var bal = acManager.GetBalanceByCredentials(new AuthenticatedOperation() { Token = credential });
                        Console.WriteLine($"Balance :{bal.Amount} HUF");
                    }

                    if (command == "history")
                    {
                        var filterDate = DateTime.MinValue;
                        var dir = TransactionDirection.Any;
                        if (arguments.Count >= 1)
                        {
                            filterDate = DateTime.Parse(arguments[0]);
                            if (arguments.Count >= 2)
                            {
                                dir = arguments[1].ToLower() == "in"
                                    ? TransactionDirection.Incoming
                                    : TransactionDirection.Outgoing;
                            }
                        }

                        PrintHistory(credential, filterDate, dir);
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



