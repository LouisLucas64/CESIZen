using Xunit;
using Moq;
using CESIZenBackOfficeMVC.Controllers;
using CESIZenModel.Context;
using CESIZenModel.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

public class UtilisateurControllerTests
{
    private NewsDbContext GetDbContextWithData()
    {
        var options = new DbContextOptionsBuilder<NewsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new NewsDbContext(options);

        return context;
    }

    [Fact]
    public async Task ModifierMDP_MotDePasseValide_ModifieEtRetourneVue()
    {
        var context = GetDbContextWithData();

        var passwordHasher = new PasswordHasher<Utilisateur>();
        var utilisateur = new Utilisateur
        {
            Id = 1,
            Nom = "Test",
            Mail = "test@example.com",
        };
        utilisateur.Mdp = passwordHasher.HashPassword(utilisateur, "Ancien123");

        context.Utilisateurs.Add(utilisateur);
        await context.SaveChangesAsync();

        var controller = new UtilisateurController(context);
        var httpContext = new DefaultHttpContext();
        httpContext.Session = new TestSession();
        httpContext.Session.SetInt32("UtilisateurId", 1);
        controller.ControllerContext.HttpContext = httpContext;

        // On simule la vérification avec le mot de passe en clair, la méthode doit comparer le hash interne
        var result = await controller.ModifierMDP("Ancien123", "Nouveau456", "Nouveau456") as ViewResult;

        Assert.NotNull(result);
        Assert.True(result.ViewData.ContainsKey("Message"));
        Assert.Equal("Mot de passe modifié avec succès.", result.ViewData["Message"]);

        var utilisateurEnBase = await context.Utilisateurs.FindAsync(1);

        // Vérifie que le nouveau mot de passe est bien hashé et correspond au mot de passe "Nouveau456"
        var verification = passwordHasher.VerifyHashedPassword(utilisateurEnBase, utilisateurEnBase.Mdp, "Nouveau456");
        Assert.Equal(PasswordVerificationResult.Success, verification);
    }

    [Fact]
    public async Task ModifierMDP_AncienMotDePasseIncorrect_RetourneVueAvecMessageErreur()
    {
        var context = GetDbContextWithData();

        var passwordHasher = new PasswordHasher<Utilisateur>();
        var utilisateur = new Utilisateur
        {
            Id = 1,
            Nom = "Test",
            Mail = "test@example.com",
        };
        utilisateur.Mdp = passwordHasher.HashPassword(utilisateur, "Ancien123");

        context.Utilisateurs.Add(utilisateur);
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

    [Fact]
    public async Task ModifierMDP_NouveauEtConfirmationDifferent_RetourneVueAvecMessageErreur()
    {
        var context = GetDbContextWithData();

        var passwordHasher = new PasswordHasher<Utilisateur>();
        var utilisateur = new Utilisateur
        {
            Id = 1,
            Nom = "Test",
            Mail = "test@example.com",
        };
        utilisateur.Mdp = passwordHasher.HashPassword(utilisateur, "Ancien123");

        context.Utilisateurs.Add(utilisateur);
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