
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace CESIZenModel.Entities;

public class Tracker
    {
        public int Id { get; set; }
        public DateTime Date_Creation { get; set; }
        public string Titre { get; set; }

        [StringLength(400)]
        public string? Description { get; set; }
        public string? Emotion_Niveau1 { get; set; }
        public string? Emotion_Niveau2 { get; set; }

        [StringLength(120)]
        public string? Commentaire { get; set; }
        public int? Note_Intensite { get; set; }

        // Relations
        public int UtilisateurId { get; set; }

        [InverseProperty(nameof(Utilisateur.Trackers))]
        public virtual Utilisateur? Utilisateur { get; set; }

        [InverseProperty(nameof(Tracker_Rapport.Tracker))]
        public virtual List<Tracker_Rapport>? TrackerRapports { get; set; }

        // Optionnel : liaison à Emotion si tu veux une navigation directe
        public int? EmotionId { get; set; }

        [InverseProperty(nameof(Emotion.Trackers))]
        public virtual Emotion? Emotion { get; set; }

    }

