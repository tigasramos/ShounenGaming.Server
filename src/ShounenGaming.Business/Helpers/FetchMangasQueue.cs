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
        void AddToPriorityQueue(int mangaId);
        void AddToQueue(int mangaId);
        int Dequeue();
        List<int> GetNextInQueue();
    }
    public class FetchMangasQueue : IFetchMangasQueue
    {
        private readonly BlockingCollection<int> _mangasQueue;
        private readonly BlockingCollection<int> _priorityMangasQueue;

        public FetchMangasQueue()
        {
            _mangasQueue = new BlockingCollection<int>();
            _priorityMangasQueue = new BlockingCollection<int>();
        }

        public void AddToQueue(int mangaId)
        {
            if (_mangasQueue.Contains(mangaId)) return;

            _mangasQueue.Add(mangaId);
        }
        public void AddToPriorityQueue(int mangaId)
        {
            if (_priorityMangasQueue.Contains(mangaId)) return;

            _priorityMangasQueue.Add(mangaId);
        }

        public int Dequeue()
        {
            if (_priorityMangasQueue.Any()) 
                return _priorityMangasQueue.Take();

            return _mangasQueue.Take();
        }

        public List<int> GetNextInQueue()
        {
            var next = _priorityMangasQueue.ToList();
            next.AddRange(_mangasQueue.ToList());
            return next;
        }
    }
}
