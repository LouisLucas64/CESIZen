using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CESIZenModel.Entities;

public class Tracker_Rapport
    {

        public int Id { get; set; }


        [InverseProperty(nameof(Tracker.TrackerRapports))]
        public Tracker Tracker { get; set; }

        public int Id_Rapport { get; set; }

        [InverseProperty(nameof(RapportEmotion.TrackerRapports))]
        public RapportEmotion Rapport { get; set; }
    }

