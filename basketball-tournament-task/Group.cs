using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace basketball_tournament_task
{
    public class Group
    {
        public string Name { get; set; }
        public List<Team> Teams { get; set; } = new List<Team>();
    }
}
