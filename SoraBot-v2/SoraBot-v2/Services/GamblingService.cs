using System;
using System.Collections.Generic;
using System.Text;

namespace SoraBot_v2.Services
{
    public class GamblingService
    {
        public int GetResult(string side)
        {
            int score = 0;
            var random = new Random();
            bool result = random.Next(100) % 2 == 0;
            switch (result)
            {
                case true:
                    if (string.Equals(side, "heads", StringComparison.CurrentCultureIgnoreCase))
                    {
                        score = 1;
                    }
                    break;

                case false:
                    if (string.Equals(side, "tails", StringComparison.CurrentCultureIgnoreCase))
                    {
                        score = 1;
                    }
                    break;
            }
            return score;
        }
    }
}