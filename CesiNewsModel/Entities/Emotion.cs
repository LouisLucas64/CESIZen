
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CESIZenModel.Entities;

public class Emotion
{
    public int Id { get; set; }
    
    [StringLength(300)]
    public string? Description { get; set; }

    public string? Nom_Emotion_Niveau1 { get; set; }


    public string? Nom_Emotion_Niveau2 { get; set; }


    [InverseProperty(nameof(Tracker.Emotion))]
    public virtual List<Tracker>? Trackers { get; set; }
}