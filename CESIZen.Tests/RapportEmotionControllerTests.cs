using Xunit;
using Moq;
using CESIZenBackOfficeMVC.Controllers;
using CESIZenModel.Context;
using CESIZenModel.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public class RapportEmotionControllerTests
{
    private NewsDbContext GetDbContextWithData()
    {
        var options = new DbContextOptionsBuilder<NewsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new NewsDbContext(options);

        // Aucun tracker ajouté ici → pour tester le cas "aucune donnée"

        return context;
    }

    [Fact]
    public async Task GenererRapport_AucunTracker_RetourneVueAvecMessage()
    {
        
        var context = GetDbContextWithData();
        var controller = new RapportEmotionController(context);

        var httpContext = new DefaultHttpContext();
        httpContext.Session = new TestSession(); // Mock session
        httpContext.Session.SetInt32("UtilisateurId", 1); // utilisateur ID fictif
        controller.ControllerContext.HttpContext = httpContext;

        var dateDebut = DateTime.Today.AddDays(-1);
        var dateFin = DateTime.Today;

        
        var result = await controller.GenererRapport(dateDebut, dateFin) as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("GenererRapport", result.ViewName); // Assure que la vue GenererRapport est retournée
        Assert.True(result.ViewData.ContainsKey("Message"));
        Assert.Equal("Aucune donnée pour cette période.", result.ViewData["Message"]);
    }


    //Cas avec trackers présents donc rapport généré
    [Fact]
    public async Task GenererRapport_TrackersPresents_GenereRapportEtRetourneVue()
    {
        
        var options = new DbContextOptionsBuilder<NewsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new NewsDbContext(options);

        // Ajout d'un tracker simulé
        context.Trackers.Add(new Tracker
        {
            Id = 1,
            UtilisateurId = 1,
            Titre = "Test Tracker",
            Date_Creation = DateTime.Today,
            Emotion_Niveau1 = "Joie",
            Emotion_Niveau2 = "Content",
            Note_Intensite = 5
        });
        await context.SaveChangesAsync();

        var controller = new RapportEmotionController(context);
        var httpContext = new DefaultHttpContext();
        httpContext.Session = new TestSession();
        httpContext.Session.SetInt32("UtilisateurId", 1);
        controller.ControllerContext.HttpContext = httpContext;

        
        var result = await controller.GenererRapport(DateTime.Today, DateTime.Today) as ViewResult;

        
        Assert.NotNull(result);
        Assert.Equal("VisualiserRapport", result.ViewName);
        var rapport = result.Model as RapportEmotion;
        Assert.NotNull(rapport);
        Assert.Equal(1, rapport.Total_Tracker);
        Assert.Equal("Joie", rapport.Emotion_Predominante_Niv1);
    }

    //Utilisateur non connecté donc redirection
    [Fact]
    public async Task GenererRapport_UtilisateurNonConnecte_RedirectionVersConnexion()
    {
     
        var context = new NewsDbContext(
            new DbContextOptionsBuilder<NewsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options
        );

        var controller = new RapportEmotionController(context);
        var httpContext = new DefaultHttpContext();
        httpContext.Session = new TestSession(); // PAS de SetInt32 alors simulateur non connecté
        controller.ControllerContext.HttpContext = httpContext;

        
        var result = await controller.GenererRapport(DateTime.Today, DateTime.Today);

        
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Connexion", redirect.ActionName);
        Assert.Equal("Utilisateur", redirect.ControllerName);
    }


    //Plusieurs émotions donc vérifie la prédominance correcte
    [Fact]
    public async Task GenererRapport_VerifieEmotionPredominante()
    {
        
        var options = new DbContextOptionsBuilder<NewsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new NewsDbContext(options);

        context.Trackers.AddRange(new List<Tracker>
    {
        new Tracker {Titre = "Test1", Id = 1, UtilisateurId = 1, Date_Creation = DateTime.Today, Emotion_Niveau1 = "Joie", Emotion_Niveau2 = "Heureux", Note_Intensite = 7 },
        new Tracker {Titre = "Test2", Id = 2, UtilisateurId = 1, Date_Creation = DateTime.Today, Emotion_Niveau1 = "Joie", Emotion_Niveau2 = "Heureux", Note_Intensite = 8 },
        new Tracker {Titre = "Test3", Id = 3, UtilisateurId = 1, Date_Creation = DateTime.Today, Emotion_Niveau1 = "Tristesse", Emotion_Niveau2 = "Déçu", Note_Intensite = 4 }
    });
        await context.SaveChangesAsync();

        var controller = new RapportEmotionController(context);
        var httpContext = new DefaultHttpContext();
        httpContext.Session = new TestSession();
        httpContext.Session.SetInt32("UtilisateurId", 1);
        controller.ControllerContext.HttpContext = httpContext;

        
        var result = await controller.GenererRapport(DateTime.Today, DateTime.Today) as ViewResult;

       
        var rapport = result.Model as RapportEmotion;
        Assert.NotNull(rapport);
        Assert.Equal("Joie", rapport.Emotion_Predominante_Niv1);
        Assert.Equal("Heureux", rapport.Emotion_Predominante_Niv2);
        Assert.Equal("Déçu", rapport.Emotion_Moins_Frequente);
    }


    //le mot de passe est modifié si l'ancien est correct et que les nouveaux correspondent
    [Fact]
    public async Task ModifierMDP_MotDePasseValide_ModifieEtRetourneVue()
    {
        
        var options = new DbContextOptionsBuilder<NewsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new NewsDbContext(options);

        // Création d’un utilisateur existant avec mot de passe "ancien123"
        context.Utilisateurs.Add(new Utilisateur
        {
            Id = 1,
            Nom = "Test",
            Mail = "test@example.com",
            Mdp = "Ancien123"
        });
        await context.SaveChangesAsync();

        var controller = new UtilisateurController(context);
        var httpContext = new DefaultHttpContext();
        httpContext.Session = new TestSession();
        httpContext.Session.SetInt32("UtilisateurId", 1);
        controller.ControllerContext.HttpContext = httpContext;

        
        var result = await controller.ModifierMDP("Ancien123", "Nouveau456", "Nouveau456") as ViewResult;

        
        Assert.NotNull(result);
        Assert.True(result.ViewData.ContainsKey("Message"));
        Assert.Equal("Mot de passe modifié avec succès.", result.ViewData["Message"]);

        // Vérifie que le mot de passe a bien été modifié en base
        var utilisateur = await context.Utilisateurs.FindAsync(1);
        Assert.Equal("Nouveau456", utilisateur.Mdp);
    }

    // Ancien mot de passe incorrect
    [Fact]
    public async Task ModifierMDP_AncienMotDePasseIncorrect_RetourneVueAvecMessageErreur()
    {
        
        var options = new DbContextOptionsBuilder<NewsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new NewsDbContext(options);

        context.Utilisateurs.Add(new Utilisateur
        {
            Id = 1,
            Nom = "Test",
            Mail = "test@example.com",
            Mdp = "Ancien123"
        });
        await context.SaveChangesAsync();

        var controller = new UtilisateurController(context);
        var httpContext = new DefaultHttpContext();
        httpContext.Session = new TestSession();
        httpContext.Session.SetInt32("UtilisateurId", 1);
        controller.ControllerContext.HttpContext = httpContext;

     
        var result = await controller.ModifierMDP("MauvaisMdp", "Nouveau456", "Nouveau456") as ViewResult;

        
        Assert.NotNull(result);
        Assert.True(result.ViewData.ContainsKey("Message"));
        Assert.Equal("Mot de passe actuel incorrect.", result.ViewData["Message"]);
    }


    //Nouveau mot de passe et confirmation ne correspondent pas
    [Fact]
    public async Task ModifierMDP_NouveauEtConfirmationDifferent_RetourneVueAvecMessageErreur()
    {
        
        var options = new DbContextOptionsBuilder<NewsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new NewsDbContext(options);

        context.Utilisateurs.Add(new Utilisateur
        {
            Id = 1,
            Nom = "Test",
            Mail = "test@example.com",
            Mdp = "Ancien123"
        });
        await context.SaveChangesAsync();

        var controller = new UtilisateurController(context);
        var httpContext = new DefaultHttpContext();
        httpContext.Session = new TestSession();
        httpContext.Session.SetInt32("UtilisateurId", 1);
        controller.ControllerContext.HttpContext = httpContext;

       
        var result = await controller.ModifierMDP("Ancien123", "Nouveau456", "MauvaiseConfirmation") as ViewResult;

       
        Assert.NotNull(result);
        Assert.True(result.ViewData.ContainsKey("Message"));
        Assert.Equal("Les mots de passe ne correspondent pas.", result.ViewData["Message"]);
    }

}