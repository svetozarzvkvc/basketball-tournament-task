using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace basketball_tournament_task
{
    public class Team
    {
        [JsonPropertyName("Team")]
        public string Name { get; set; }
        public string ISOCode { get; set; }
        public int FIBARanking { get; set; }
        public int Wins { get; set; } = 0;
        public int Loses { get; set; } = 0;
        public int Retreats { get; set; } = 0;
        public int Points
        {
            get
            {
                return Wins * 2 + (Loses - Retreats);
            }
        }
        public int PointsScored { get; set; } = 0;
        public int PointsConceded { get; set; } = 0;
        public double Form { get; set; } = 0;
        public int BasketDifference
        {
            get
            {
                return PointsScored - PointsConceded;
            }
        }
        public Group Group { get; set; }
        public List<Match> Matches { get; set; } = new List<Match>();
        public List<Exibition> Exibitions { get; set; } = new List<Exibition>();

        public override string ToString()
        {
            return $"{Name} \t {Wins} / {Loses} / {Points} / {PointsScored} / {PointsConceded} / {(BasketDifference >= 0 ? $"+{BasketDifference}" : BasketDifference.ToString())}";
        }

        public void GenerateInitialFormCoefficient(List<Team> teams)
        {
            var mecevi = this.Exibitions;
            foreach (var mec in mecevi)
            {
                var protivnik = teams.FirstOrDefault(x => x.ISOCode == mec.Opponent);
                var rezultati = mec.Result.Split('-');
                int tim1Poeni = int.Parse(rezultati[0]);
                int tim2Poeni = int.Parse(rezultati[1]);

                var razlikaPoena = tim1Poeni - tim2Poeni;
                if(tim1Poeni > tim2Poeni)
                {
                    this.Form += 0.025;
                    if(razlikaPoena >= 20)
                    {
                        this.Form += 0.025;
                    }
                    if (protivnik.FIBARanking < this.FIBARanking)
                    {
                        this.Form += 0.025;
                    }
                }
                else
                {
                    this.Form -= 0.025;
                    if (Math.Abs(razlikaPoena) >= 20)
                    {
                        this.Form -= 0.025;
                    }
                }
            }
        }

    }
}
