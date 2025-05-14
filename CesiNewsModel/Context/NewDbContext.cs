

using CESIZenModel.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CESIZenModel.Context;

public class NewsDbContext : DbContext
{
    //public virtual DbSet<Contenu> Contenus { get; set; } = null!;

    public virtual DbSet<Emotion> Emotions { get; set; } = null!;

    public virtual DbSet<Tracker> Trackers { get; set; } = null!;

    public virtual DbSet<Utilisateur> Utilisateurs { get; set; } = null!;

    //public virtual DbSet<Role> Roles { get; set; } = null!;

    public virtual DbSet<RapportEmotion> RapportEmotions { get; set; } = null!;

    public virtual DbSet<Tracker_Rapport> Tracker_Rapports { get; set; } = null!;


    public NewsDbContext(DbContextOptions<NewsDbContext> options)
        : base(options)
    {
    }


    
}

