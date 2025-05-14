using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CESIZenModel.Entities;
public class RapportEmotion
    {
        
        public int Id { get; set; }
        public DateTime Date_Debut { get; set; }
        public DateTime Date_Fin { get; set; }

        public string Emotion_Predominante_Niv1 { get; set; }
        public string Emotion_Predominante_Niv2 { get; set; }
        public string Emotion_Moins_Frequente { get; set; }

        public float Moyenne_Intensite { get; set; }
        public int Total_Tracker { get; set; }

        public DateTime Date_Generation { get; set; }

        [InverseProperty(nameof(Tracker_Rapport.Rapport))]
        public virtual List<Tracker_Rapport>? TrackerRapports { get; set; }

        
    }

