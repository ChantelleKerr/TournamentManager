using System;
using System.Collections.Generic;
using System.Text;

namespace TournamentManager
{
    public class Team
    {
        public string Name { get; set; }
        public int Score { get; set; }
        public int Placement { get; set; }
        public bool Eliminated { get; set; }
    }
}
