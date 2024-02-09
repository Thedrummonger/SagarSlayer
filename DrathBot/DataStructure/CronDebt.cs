using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SagarSlayer.DataStructure
{
    public class CronDebt
    {
        public ulong SilverDebt = 0;
        public static readonly ulong SilverConversionRate = 2000000;
        public List<ChronDebtTransaction> TransactionList = new List<ChronDebtTransaction>();
        public void UpdateSilverDebt(ulong silver, DiscordUser user)
        {
            var SilverAdded = silver;
            ChronDebtTransaction transaction = new(this, SilverAdded, user);
            SilverDebt = transaction.NewValue;
            TransactionList.Add(transaction);
        }
        public void SetSilverDebt(ulong silver, DiscordUser user)
        {
            ulong DIFF = silver - SilverDebt;
            ChronDebtTransaction transaction = new(this, DIFF, user);
            SilverDebt = transaction.NewValue;
            TransactionList.Add(transaction);
        }
        public ulong GetSilverDebt()
        {
            return SilverDebt;
        }
        public ulong GetCronDebt()
        {
            return SilverDebt / SilverConversionRate;
        }

        public enum Currency
        {
            Cron,
            Silver
        }
    }
    public class ChronDebtTransaction
    {
        public ChronDebtTransaction(CronDebt parent, ulong SilverAmountAdded, DiscordUser discordUser)
        {
            PreviousValue = parent.SilverDebt;
            Date = DateTime.Now;
            SilverAdded = SilverAmountAdded;
            NewValue = PreviousValue + SilverAmountAdded;
            UpdatedBy = discordUser.Id;
        }
        public ulong PreviousValue;
        public ulong SilverAdded;
        public ulong NewValue;
        public DateTime Date;
        public ulong UpdatedBy;
    }
}
