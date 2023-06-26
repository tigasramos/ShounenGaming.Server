using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Helpers
{
    public interface IFetchMangasQueue
    {
        void AddToQueue(int mangaId);
        int Dequeue();
    }
    public class FetchMangasQueue : IFetchMangasQueue
    {
        private readonly BlockingCollection<int> _mangasQueue;

        public FetchMangasQueue()
        {
            _mangasQueue = new BlockingCollection<int>();
        }

        public void AddToQueue(int mangaId)
        {
            if (_mangasQueue.Contains(mangaId)) return;

            _mangasQueue.Add(mangaId);
        }
        public int Dequeue()
        {
            return _mangasQueue.Take();
        }
    }
}
