using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace basketball_tournament_task
{
    public class Match
    {
        public Team OponentA { get; set; }
        public Team OponentB { get; set; }
        public int OponentAScore { get; set; } = 0;
        public int OponentBScore { get; set; } = 0;
        public Team Winner { get; set; }
        public Team Looser { get; set; }

    }
}
