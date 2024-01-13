using DrathBot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SagarSlayer.DataStructure
{
    public class Misc
    {
        public class DistinctList<T>
        {
            public event Action ListUpdated;
            public DistinctList(IEnumerable<T> source, double RefreshPercent) 
            { 
                Source = source.ToList();
                refreshDec = RefreshPercent;
                ResetAll();
                rnd = new Random();
            }
            public DistinctList()
            {
                Source = new List<T>();
                refreshDec = 0.6;
                ResetAll();
                rnd = new Random();
            }
            public double refreshDec;
            public List<T> Source;
            public List<T> Unused = [];
            public List<T> Used = [];
            private Random rnd;
            [JsonIgnore]
            public int MaxUsed { get { return (int)(Source.Count * refreshDec); } }

            public T GetRandomUnused()
            {
                return GetUnused(rnd.Next(Unused.Count));
            }
            public T GetUnused(int Index)
            {
                T Candidate = Unused[Index];
                Used.Add(Candidate);
                Unused.RemoveAt(Index);
                RefreshOldest();
                if (ListUpdated is not null) { ListUpdated(); }
                return Candidate;
            }

            public void AddNew(T input)
            {
                Source.Add(input);
                Unused.Add(input);
                RefreshOldest();
                if (ListUpdated is not null) { ListUpdated(); }
            }

            public void Remove(T target)
            {
                Source.Remove(target);
                Unused.Remove(target);
                Used.Remove(target);
                RefreshOldest();
                if (ListUpdated is not null) { ListUpdated(); }
            }

            public void ResetAll()
            {
                Unused.Clear();
                Used.Clear();
                Unused = new List<T>(Source);
                if (ListUpdated is not null) { ListUpdated(); }
            }

            public T SetMessageUnused(int Index)
            {
                T Candidate = Used[Index];
                Unused.Add(Candidate);
                Used.RemoveAt(Index);
                RefreshOldest();
                if (ListUpdated is not null) { ListUpdated(); }
                return Candidate;
            }

            private void RefreshOldest()
            {
                if (Used.Count != 0 && Used.Count > MaxUsed)
                {
                    T Oldest = Used[0];
                    Used.RemoveAt(0);
                    Unused.Add(Oldest);
                }
            }

        }
    }
}
