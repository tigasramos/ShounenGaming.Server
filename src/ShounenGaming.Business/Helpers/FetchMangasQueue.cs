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
        private readonly Queue<int> _mangasQueue;
        private readonly BlockingCollection<int> _mainQueue;

        public FetchMangasQueue()
        {
            _mangasQueue = new Queue<int>();
            _mainQueue = new BlockingCollection<int>();
        }

        public void AddToQueue(int mangaId)
        {
            if (_mangasQueue.Contains(mangaId)) return;

            _mangasQueue.Enqueue(mangaId);

            CalculateMainQueue();
        }

        public void AddToPriorityQueue(int mangaId)
        {
            if (_mainQueue.Contains(mangaId)) return;

            _mainQueue.Add(mangaId);
        }

        private void CalculateMainQueue()
        {
            if (!_mainQueue.Any() && _mangasQueue.Any())
                _mainQueue.Add(_mangasQueue.Dequeue());
        }

        public int Dequeue()
        {
            CalculateMainQueue();
            return _mainQueue.Take();
        }

        public List<int> GetNextInQueue()
        {
            var next = _mainQueue.ToList();
            next.AddRange(_mangasQueue.ToList());
            return next;
        }
    }
}
