using System;

namespace SoraBot.Services.Utils
{
    public class RandomNumberService
    {
        private readonly Random _random;
        private readonly object _lock;

        public RandomNumberService()
        {
            _lock = new object();
            _random = new Random();
        }

        /// <summary>
        /// Number between minVal INCLUSIVE and maxVal EXCLUSIVE
        /// </summary>
        /// <param name="minVal"></param>
        /// <param name="maxVal"></param>
        /// <returns></returns>
        public int GetRandomNext(int minVal, int maxVal)
        {
            lock (_lock)
            {
                return _random.Next(minVal, maxVal);
            }
        }
        
        /// <summary>
        /// Number between 0 and maxVal EXCLUSIVE
        /// </summary>
        /// <param name="maxVal"></param>
        /// <returns></returns>
        public int GetRandomNext(int maxVal)
        {
            lock (_lock)
            {
                return _random.Next(maxVal);
            }
        }

        /// <summary>
        /// Double between 0.0 INCLUSIVE and 1.0 EXCLUSIVE
        /// </summary>
        /// <returns></returns>
        public double GetRandomDouble()
        {
            lock (_lock)
            {
                return _random.NextDouble();
            }
        }
    }
}