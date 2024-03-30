using System.Collections.Concurrent;

namespace ShounenGaming.Business.Helpers
{
    public class QueuedManga
    {
        public int MangaId { get; set; }
        public int? QueuedByUser { get; set; }
        public DateTime QueuedAt { get; set; }
    }
    public interface IFetchMangasQueue
    {
        void AddToPriorityQueue(QueuedManga mangaId);
        void AddToQueue(QueuedManga mangaId);
        QueuedManga Dequeue();
        List<QueuedManga> GetNextInQueue();
    }
    public class FetchMangasQueue : IFetchMangasQueue
    {
        private readonly Queue<QueuedManga> _mangasQueue;
        private readonly BlockingCollection<QueuedManga> _mainQueue;

        public FetchMangasQueue()
        {
            _mangasQueue = new Queue<QueuedManga>();
            _mainQueue = new BlockingCollection<QueuedManga>();
        }

        public void AddToQueue(QueuedManga manga)
        {
            if (_mangasQueue.Any(m => m.MangaId == manga.MangaId)) return;

            _mangasQueue.Enqueue(manga);

            CalculateMainQueue();
        }

        public void AddToPriorityQueue(QueuedManga manga)
        {
            if (_mainQueue.Any(m => m.MangaId == manga.MangaId)) return;

            _mainQueue.Add(manga);
        }

        private void CalculateMainQueue()
        {
            if (!_mainQueue.Any() && _mangasQueue.Any())
                _mainQueue.Add(_mangasQueue.Dequeue());
        }

        public QueuedManga Dequeue()
        {
            CalculateMainQueue();
            return _mainQueue.Take();
        }

        public List<QueuedManga> GetNextInQueue()
        {
            var queued = new List<QueuedManga>();
            queued.AddRange(_mainQueue.ToList());
            queued.AddRange(_mangasQueue.ToList());
            return queued;
        }
    }
}
