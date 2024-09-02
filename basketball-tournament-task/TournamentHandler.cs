using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace basketball_tournament_task
{
    public class TournamentHandler
    {
        public List<Group> Groups { get; set; } = new List<Group>();
        public List<Team> Teams { get; set; } = new List<Team>();
        public List<Match> QuarterFinalMatches { get; set; } = new List<Match>();
        public List<Match> SemiFinalMatches { get; set; } = new List<Match>();
        public Match ThirdPlaceMatch { get; set; } = new Match();
        public Match FinalMatch { get; set; } = new Match();

        public void LoadData(string groupsFilePath, string exhibitionsFilePath)
        {
            string groupsContent = File.ReadAllText(groupsFilePath);
            string exhibitionsContent = File.ReadAllText(exhibitionsFilePath);

            var groupsObjects = JsonSerializer.Deserialize<Dictionary<string, List<Team>>>(groupsContent);
            var exhibitionsObjects = JsonSerializer.Deserialize<Dictionary<string, List<Exibition>>>(exhibitionsContent);

            foreach (var groupEntry in groupsObjects)
            {
                var group = new Group
                {
                    Name = groupEntry.Key,
                    Teams = groupEntry.Value
                };
                Groups.Add(group);
            }

            foreach (var group in Groups)
            {
                foreach (var team in group.Teams)
                {
                    team.Group = group;
                    Teams.Add(team);
                }
            }

            foreach (var groupEntry2 in exhibitionsObjects)
            {
                foreach (var tekme in groupEntry2.Value)
                {
                    foreach (var item in Teams)
                    {
                        if (groupEntry2.Key == item.ISOCode)
                        {
                            item.Exibitions.Add(tekme);
                        }
                    }
                }
            }

            foreach (var item in Teams)
            {
                item.GenerateInitialFormCoefficient(Teams);
            }
        }

        public void RunGroupPhase()
        {
            for (int i = 0; i < Groups.First().Teams.Count - 1; i++)
            {
                Console.WriteLine($"Grupna faza - {i + 1}. kolo:\n");

                for (int g = 0; g < Groups.Count; g++)
                {
                    Console.WriteLine($"Grupa {Groups[g].Name}:");
                    List<Team> teams = Groups[g].Teams;

                    switch (i)
                    {
                        case 0:
                            Console.WriteLine($"\t{PrintMatchData(MatchSimulator, teams[0], teams[3])}");
                            Console.WriteLine($"\t{PrintMatchData(MatchSimulator, teams[1], teams[2])}");
                            break;

                        case 1:
                            Console.WriteLine($"\t{PrintMatchData(MatchSimulator, teams[2], teams[0])}");
                            Console.WriteLine($"\t{PrintMatchData(MatchSimulator, teams[3], teams[1])}");
                            break;

                        case 2:
                            Console.WriteLine($"\t{PrintMatchData(MatchSimulator, teams[0], teams[1])}");
                            Console.WriteLine($"\t{PrintMatchData(MatchSimulator, teams[2], teams[3])}");
                            break;
                    }
                }
                Console.WriteLine("\n\n");
            }
            Console.WriteLine("\n");
        }

        public void SortTeamsAndPrintTables()
        {
            Console.WriteLine("Konačan plasman u grupama:");
            for (int i = 0; i < Groups.Count; i++)
            {
                Console.WriteLine($"    Grupa {Groups[i].Name} (Ime - pobede/porazi/bodovi/postignuti koševi/primljeni koševi/koš razlika):");
                var sortTeamsByPoints = Groups[i].Teams.OrderByDescending(x => x.Points).ToList();
                var groupTeamsByPoints = sortTeamsByPoints.GroupBy(x => x.Points).ToList();
                if (groupTeamsByPoints.Count < 4)
                {
                    if (groupTeamsByPoints.Count == 2)
                    {
                        if (groupTeamsByPoints.Any(x => x.Count() == 3))
                        {
                            for (int z = 0; z < groupTeamsByPoints.Count; z++)
                            {
                                if (groupTeamsByPoints[z].Count() == 3)
                                {
                                    var currentElementsGroup = groupTeamsByPoints[z].ToList();
                                    var elementsIndexes = currentElementsGroup.Select(z => sortTeamsByPoints.IndexOf(z)).ToList();
                                    var teamNotInGroup = sortTeamsByPoints.FirstOrDefault(t => !currentElementsGroup.Any(e => e.Name == t.Name));
                                    var teamNotInGroupIndex = sortTeamsByPoints.IndexOf(teamNotInGroup);
                                    var query = currentElementsGroup.OrderByDescending(x =>
                                    {
                                        var match = x.Matches.FirstOrDefault(y => y.OponentB.Name == teamNotInGroup.Name || y.OponentA.Name == teamNotInGroup.Name);
                                        if (match != null)
                                        {
                                            var teamAPoints = match.OponentA.Name == x.Name ? match.OponentAScore : match.OponentBScore;
                                            var teamBPoints = match.OponentB.Name == x.Name ? match.OponentAScore : match.OponentBScore;
                                            var pointdifference = x.BasketDifference - teamAPoints + teamBPoints;
                                            return x.BasketDifference - teamAPoints + teamBPoints;
                                        }
                                        return x.BasketDifference;
                                    }).ThenByDescending(x => x.BasketDifference).ToList();

                                    for (int s = 0; s < query.Count; s++)
                                    {
                                        sortTeamsByPoints[elementsIndexes[s]] = query[s];
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (int j = 0; j < groupTeamsByPoints.Count; j++)
                            {
                                var element = groupTeamsByPoints[j];
                                for (int k = 0; k < element.Count(); k++)
                                {
                                    for (int l = k + 1; l < element.Count(); l++)
                                    {
                                        if (element.ElementAt(k).Matches.Any(x => x.OponentA.Name.Contains(element.ElementAt(l).Name) || x.OponentB.Name.Contains(element.ElementAt(l).Name)))
                                        {
                                            var match = element.ElementAt(k).Matches.FirstOrDefault(x => x.OponentA.Name.Contains(element.ElementAt(l).Name) || x.OponentB.Name.Contains(element.ElementAt(l).Name));
                                            var matchWinner = match?.Winner;
                                            if (matchWinner != element.ElementAt(k))
                                            {
                                                int indexK = sortTeamsByPoints.IndexOf(element.ElementAt(k));
                                                int indexL = sortTeamsByPoints.IndexOf(element.ElementAt(l));
                                                var temp = sortTeamsByPoints[indexK];
                                                sortTeamsByPoints[indexK] = sortTeamsByPoints[indexL];
                                                sortTeamsByPoints[indexL] = temp;
                                            }
                                            else
                                            {
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                for (int j = 0; j < sortTeamsByPoints.Count; j++)
                {
                    Console.WriteLine($"        {j + 1}. {sortTeamsByPoints[j]}");
                }
                Console.WriteLine("\n");

                Groups[i].Teams = sortTeamsByPoints;
            }
        }

        public void RunDraw()
        {
            var firstPlaceTeams = new List<Team>();
            var SecondPlaceTeams = new List<Team>();
            var thirdPlaceTeams = new List<Team>();

            for (int i = 0; i < Groups.Count; i++)
            {
                var teams = Groups[i].Teams.ToList();
                for (int t = 0; t < teams.Count; t++)
                {
                    if (t == 0)
                    {
                        firstPlaceTeams.Add(teams[t]);
                    }
                    if (t == 1)
                    {
                        SecondPlaceTeams.Add(teams[t]);
                    }

                    if (t == 2)
                    {
                        thirdPlaceTeams.Add(teams[t]);
                    }
                }

            }

            var finalEight = new List<Team>();

            finalEight.AddRange(firstPlaceTeams.OrderByDescending(x => x.Points)
                                       .ThenByDescending(x => x.BasketDifference)
                                       .ThenByDescending(x => x.PointsScored)
                                       .ToList());

            finalEight.AddRange(SecondPlaceTeams.OrderByDescending(x => x.Points)
                                               .ThenByDescending(x => x.BasketDifference)
                                               .ThenByDescending(x => x.PointsScored)
                                               .ToList());

            finalEight.AddRange(thirdPlaceTeams.OrderByDescending(x => x.Points)
                                                .ThenByDescending(x => x.BasketDifference)
                                                .ThenByDescending(x => x.PointsScored)
                                                .ToList());
            finalEight.RemoveAt(8);

            var pots = new Dictionary<string, List<Team>>
            {
                { "Sesir D", new List<Team> { finalEight[0], finalEight[1] } },
                { "Sesir E", new List<Team> { finalEight[2], finalEight[3] } },
                { "Sesir F", new List<Team> { finalEight[4], finalEight[5] } },
                { "Sesir G", new List<Team> { finalEight[6], finalEight[7] } }
            };

            Console.WriteLine("Šeširi:");

            foreach (var pot in pots)
            {
                Console.WriteLine($"  {pot.Key}");
                foreach (var team in pot.Value)
                {
                    Console.WriteLine($"      {team.Name}");
                }
            }

            Console.WriteLine("\n");


            DrawQuarterFinalTeams("D", "G", pots["Sesir D"], pots["Sesir G"]);
            DrawQuarterFinalTeams("E", "F", pots["Sesir E"], pots["Sesir F"]);

            Console.WriteLine("Eliminaciona faza:");

            foreach (var item in QuarterFinalMatches)
            {
                Console.WriteLine($"   {item.OponentA.Name} - {item.OponentB.Name}");
            }

        }

        public void RunEliminationPhaseMatches()
        {
            for (int i = 0; i < QuarterFinalMatches.Count; i++)
            {
                QuarterFinalMatches[i] = MatchSimulator(QuarterFinalMatches[i].OponentA, QuarterFinalMatches[i].OponentB);
                QuarterFinalMatches[i].Winner = QuarterFinalMatches[i].Winner;
                QuarterFinalMatches[i].Looser = QuarterFinalMatches[i].Looser;
            }

            Random rand = new Random();
            int firstIndex = rand.Next(2, 4);
            var firstChoice = QuarterFinalMatches[firstIndex];

            int secondIndex = firstIndex == 2 ? 3 : 2;
            var secondChoice = QuarterFinalMatches[secondIndex];

            var semifinals = new List<Match>
            {
                new Match
                {
                    OponentA = QuarterFinalMatches[0].Winner,
                    OponentB = firstChoice.Winner
                },
                new Match
                {
                    OponentA = QuarterFinalMatches[1].Winner,
                    OponentB = secondChoice.Winner
                } 
            };
            SemiFinalMatches.AddRange(semifinals);

            SemiFinalMatches[0] = MatchSimulator(SemiFinalMatches[0].OponentA, SemiFinalMatches[0].OponentB);
            SemiFinalMatches[1] = MatchSimulator(SemiFinalMatches[1].OponentA, SemiFinalMatches[1].OponentB);
             
            FinalMatch = new Match
            {
                OponentA = SemiFinalMatches[0].Winner,
                OponentB = SemiFinalMatches[1].Winner
            };

            FinalMatch = MatchSimulator(FinalMatch.OponentA, FinalMatch.OponentB);
            
            ThirdPlaceMatch = new Match
            {
                OponentA = SemiFinalMatches[0].Looser,
                OponentB = SemiFinalMatches[1].Looser
            };

            ThirdPlaceMatch = MatchSimulator(ThirdPlaceMatch.OponentA, ThirdPlaceMatch.OponentB);
        }

        public void PrintEliminationPhaseResults()
        {
            Console.WriteLine("\n");

            Console.WriteLine("Cetvrtfinale:");
            for (int i = 0; i < QuarterFinalMatches.Count; i++)
            {
                Console.WriteLine($"   {QuarterFinalMatches[i].OponentA.Name} - {QuarterFinalMatches[i].OponentB.Name} ({QuarterFinalMatches[i].OponentAScore} - {QuarterFinalMatches[i].OponentBScore})");
            }

            Console.WriteLine("\n");

            Console.WriteLine("Polufinale:");

            Console.WriteLine($"   {SemiFinalMatches[0].OponentA.Name} - {SemiFinalMatches[0].OponentB.Name} ({SemiFinalMatches[0].OponentAScore} - {SemiFinalMatches[0].OponentBScore})");
            Console.WriteLine($"   {SemiFinalMatches[1].OponentA.Name} - {SemiFinalMatches[1].OponentB.Name} ({SemiFinalMatches[1].OponentAScore} - {SemiFinalMatches[1].OponentBScore})");

            Console.WriteLine("\n");

            Console.WriteLine("Finale:");

            Console.WriteLine($"   {FinalMatch.OponentA.Name} - {FinalMatch.OponentB.Name} ({FinalMatch.OponentAScore} - {FinalMatch.OponentBScore})");


            Console.WriteLine("\n");

            Console.WriteLine("Utakmica za trece mesto:");

            Console.WriteLine($"   {ThirdPlaceMatch.OponentA.Name} - {ThirdPlaceMatch.OponentB.Name} ({ThirdPlaceMatch.OponentAScore} - {ThirdPlaceMatch.OponentBScore})");


            Console.WriteLine("\n");

            Console.WriteLine("Medalje:");

            Console.WriteLine($"   1. {FinalMatch.Winner.Name}");
            Console.WriteLine($"   2. {FinalMatch.Looser.Name}");
            Console.WriteLine($"   3. {ThirdPlaceMatch.Winner.Name}");

        }

        public Match MatchSimulator(Team teamA, Team teamB)
        {
            Random random = new Random();

            double winProbability = random.Next(1, 101) / 100.0;

            var rankingDifference = Math.Abs(teamA.FIBARanking - teamB.FIBARanking);

            var formDifference = Math.Abs(teamA.Form - teamB.Form);

            var winProbabilityTeamA = 0.5;
            var winProbabilityTeamB = 0.5;

            
            if (teamA.FIBARanking < teamB.FIBARanking)
            {

                winProbabilityTeamA += rankingDifference / 100.0;
                winProbabilityTeamB -= rankingDifference / 100.0;
            }
            else
            {
                winProbabilityTeamA -= rankingDifference / 100.0;
                winProbabilityTeamB += rankingDifference / 100.0;
            }

            if (teamA.Form > teamB.Form)
            {
                winProbabilityTeamA += formDifference;
                winProbabilityTeamB -= formDifference;
            }
            else
            {
                winProbabilityTeamB += formDifference;
                winProbabilityTeamA -= formDifference;
            }

            var winnerPoints = random.Next(71, 120);
            var looserPoints = random.Next(70, winnerPoints);


            if (winProbability <= winProbabilityTeamA)
            {

                teamA.Wins++;
                teamA.PointsScored += winnerPoints;
                teamA.PointsConceded += looserPoints;
                teamA.Form += 0.025;

                if (teamB.FIBARanking < teamA.FIBARanking)
                {
                    teamA.Form += 0.025;
                }
                if ((winnerPoints - looserPoints) >= 25 && (winnerPoints - looserPoints) < 45)
                {
                    teamA.Form += 0.025;
                    teamB.Form -= 0.025;
                }

                if ((winnerPoints - looserPoints) >= 45)
                {
                    teamA.Form += 0.025;
                    teamB.Form -= 0.025;
                    teamB.Retreats++;
                }

                teamB.Loses++;
                teamB.PointsScored += looserPoints;
                teamB.PointsConceded += winnerPoints;
                teamB.Form -= 0.025;

                var match = new Match
                {
                    OponentA = teamA,
                    OponentB = teamB,
                    OponentAScore = winnerPoints,
                    OponentBScore = looserPoints,
                    Winner = teamA,
                    Looser = teamB
                };


                teamA.Matches.Add(match);
                teamB.Matches.Add(match);

                return match;

            }
            else
            {
                teamB.Wins++;
                teamB.PointsScored += winnerPoints;
                teamB.PointsConceded += looserPoints;
                teamB.Form += 0.025;

                if (teamA.FIBARanking < teamB.FIBARanking)
                {
                    teamB.Form += 0.025;
                }
                if ((winnerPoints - looserPoints) >= 25 && (winnerPoints - looserPoints) < 45)
                {
                    teamB.Form += 0.025;
                    teamA.Form -= 0.025;
                }

                if ((winnerPoints - looserPoints) >= 45)
                {
                    teamB.Form += 0.025;
                    teamA.Form -= 0.025;
                    teamA.Retreats++;
                }


                teamA.Loses++;
                teamA.PointsScored += looserPoints;
                teamA.PointsConceded += winnerPoints;
                teamA.Form -= 0.025;

                var match = new Match
                {
                    OponentA = teamA,
                    OponentB = teamB,
                    OponentAScore = looserPoints,
                    OponentBScore = winnerPoints,
                    Winner = teamB,
                    Looser = teamA
                };

                teamA.Matches.Add(match);
                teamB.Matches.Add(match);

                return match;
            }

        }

        public string PrintMatchData(Func<Team, Team, Match> simulator, Team teamA, Team teamB)
        {
            Match matchResult = simulator(teamA, teamB);

            var matchResultText = $"{matchResult.OponentA.Name} - {matchResult.OponentB.Name} ({matchResult.OponentAScore} - {matchResult.OponentBScore})";
            return matchResultText;
        }

        public void DrawQuarterFinalTeams(string firstPot, string secondPot, List<Team> firstPotTeams, List<Team> secondPotTeams)
        {
            var success = false;
            while (!success)
            {
                success = true;
                var shuffledFirstPot = firstPotTeams.OrderBy(x => Guid.NewGuid()).ToList();
                var shuffledSecondPot = secondPotTeams.OrderBy(x => Guid.NewGuid()).ToList();
                foreach (var team in shuffledFirstPot)
                {
                    var opponent = shuffledSecondPot.FirstOrDefault(g => !team.Matches.Any(m => m.OponentB.Name == g.Name || m.OponentA.Name == g.Name));
                    if (opponent == null)
                    {
                        success = false;

                        if (firstPot == "D" || secondPot == "G")
                        {
                            QuarterFinalMatches.Clear();
                        }
                        else
                        {
                            QuarterFinalMatches.RemoveAt(2);
                        }
                        break;
                    }
                    else
                    {
                        QuarterFinalMatches.Add(new Match
                        {
                            OponentA = team,
                            OponentB = opponent
                        });
                        shuffledSecondPot.Remove(opponent);
                    }
                }
            }
        }
    }

}
