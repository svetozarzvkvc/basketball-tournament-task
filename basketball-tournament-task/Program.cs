using System;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace basketball_tournament_task
{
    internal class Program
    {
        private const string GroupsFilePath = @"Data\groups.json";
        private const string ExhibitionsFilePath = @"Data\exibitions.json";
        static void Main(string[] args)
        {
            TournamentHandler tournamentHandler = new TournamentHandler();

            tournamentHandler.LoadData(GroupsFilePath, ExhibitionsFilePath);
            tournamentHandler.RunGroupPhase();
            tournamentHandler.SortTeamsAndPrintTables();
            tournamentHandler.RunDraw();
            tournamentHandler.RunEliminationPhaseMatches();
            tournamentHandler.PrintEliminationPhaseResults();
        }
    }
    
}
