using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CESIZenModel.Entities;


public class Utilisateur
    {
        public int Id { get; set; }
        public string? Pseudo { get; set; }
        public string? Mdp { get; set; }
        public string? Nom { get; set; }
        public string? Prenom { get; set; }
        public string? Mail { get; set; }
        public string? Tel { get; set; }

        public bool Actif { get; set; }

        public string Role { get; set; } = "User";

    [InverseProperty(nameof(Tracker.Utilisateur))]
        public virtual List<Tracker>? Trackers { get; set; }
        
    }

