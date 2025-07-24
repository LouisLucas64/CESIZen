using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CESIZenModel.Entities;
using CESIZenModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using CESIZenModel.Context;
using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Identity;  // Ajout pour PasswordHasher

namespace CESIZenBackOfficeMVC.Controllers
{
    public class UtilisateurController : Controller
    {
        private readonly NewsDbContext _context;
        private readonly PasswordHasher<Utilisateur> _passwordHasher = new();

        public UtilisateurController(NewsDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> ListeUtilisateurs()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin")
                // return Forbid(); 
                RedirectToAction("Index", "Home");

            var utilisateurs = await _context.Utilisateurs.ToListAsync();
            return View(utilisateurs);
        }

        public IActionResult CreationCompte() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreationCompte(Utilisateur utilisateur)
        {
            bool champManquant = false;

            if (string.IsNullOrWhiteSpace(utilisateur.Pseudo))
            {
                ViewData["MessagePseudo"] = "Le pseudo est requis.";
                champManquant = true;
            }

            if (string.IsNullOrWhiteSpace(utilisateur.Mail))
            {
                ViewData["MessageMail"] = "L'adresse email est requise.";
                champManquant = true;
            }

            if (string.IsNullOrWhiteSpace(utilisateur.Mdp))
            {
                ViewData["MessageMdp"] = "Le mot de passe est requis.";
                champManquant = true;
            }

            if (string.IsNullOrWhiteSpace(utilisateur.Nom))
            {
                ViewData["MessageNom"] = "Le nom est requis.";
                champManquant = true;
            }

            if (string.IsNullOrWhiteSpace(utilisateur.Prenom))
            {
                ViewData["MessagePrenom"] = "Le prénom est requis.";
                champManquant = true;
            }

            if (champManquant)
            {
                ViewData["Message"] = "Merci de remplir tous les champs requis.";
                return View(utilisateur);
            }

            if (!ModelState.IsValid)
            {
                ViewData["Message"] = "Certains champs ne sont pas valides.";
                return View(utilisateur);
            }

            // Vérification du mot de passe
            string mdp = utilisateur.Mdp ?? "";
            if (mdp.Length < 8 ||
                !mdp.Any(char.IsUpper) ||
                !mdp.Any(char.IsDigit))
            {
                ViewData["Message"] = "Le mot de passe doit contenir au moins 8 caractères, une majuscule et un chiffre.";
                return View(utilisateur);
            }

            bool pseudoExiste = await _context.Utilisateurs
                .AnyAsync(u => u.Pseudo == utilisateur.Pseudo);
            if (pseudoExiste)
            {
                ViewData["Message"] = "Ce pseudo est déjà utilisé.";
                return View(utilisateur);
            }

            bool emailExiste = await _context.Utilisateurs
                .AnyAsync(u => u.Mail == utilisateur.Mail);
            if (emailExiste)
            {
                ViewData["Message"] = "Cet email est déjà utilisé.";
                return View(utilisateur);
            }

            utilisateur.Role = "Utilisateur";

            // **hachage du mot de passe**
            utilisateur.Mdp = _passwordHasher.HashPassword(utilisateur, utilisateur.Mdp);

            _context.Add(utilisateur);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> ModifierCompte(int? id)
        {
            if (id == null) return NotFound();

            var utilisateur = await _context.Utilisateurs.FindAsync(id);
            if (utilisateur == null) return NotFound();

            //ViewData["RoleId"] = new SelectList(_context.Roles, "Id_Role", "Description", utilisateur.RoleId);
            return View(utilisateur);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModifierCompte(Utilisateur utilisateur)
        {
            bool champManquant = false;

            if (string.IsNullOrWhiteSpace(utilisateur.Mail))
            {
                ViewData["MessageMail"] = "L'adresse email est requise.";
                champManquant = true;
            }

            if (string.IsNullOrWhiteSpace(utilisateur.Nom))
            {
                ViewData["MessageNom"] = "Le nom est requis.";
                champManquant = true;
            }

            if (string.IsNullOrWhiteSpace(utilisateur.Prenom))
            {
                ViewData["MessagePrenom"] = "Le prénom est requis.";
                champManquant = true;
            }

            if (champManquant)
            {
                ViewData["Message"] = "Merci de remplir tous les champs requis.";
                return View(utilisateur);
            }

            if (!ModelState.IsValid)
            {
                ViewData["Message"] = "Certains champs ne sont pas valides.";
                return View(utilisateur);
            }

            bool emailExiste = await _context.Utilisateurs
                .AnyAsync(u => u.Mail == utilisateur.Mail && u.Id != utilisateur.Id);
            if (emailExiste)
            {
                ViewData["Message"] = "Cet email est déjà utilisé.";
                return View(utilisateur);
            }

            if (!ModelState.IsValid)
                return View(utilisateur);

            var utilisateurEnBase = await _context.Utilisateurs.FindAsync(utilisateur.Id);
            if (utilisateurEnBase == null)
                return NotFound();

            utilisateurEnBase.Nom = utilisateur.Nom;
            utilisateurEnBase.Prenom = utilisateur.Prenom;
            utilisateurEnBase.Mail = utilisateur.Mail;
            utilisateurEnBase.Tel = utilisateur.Tel;

            await _context.SaveChangesAsync();
            return RedirectToAction("Compte");
        }

        public async Task<IActionResult> SupprimerCompte(int id)
        {
            var utilisateur = await _context.Utilisateurs.FindAsync(id);
            if (utilisateur == null)
                return NotFound();

            _context.Utilisateurs.Remove(utilisateur);
            await _context.SaveChangesAsync();

            return RedirectToAction("ListeUtilisateurs");
        }

        [HttpGet]
        public IActionResult Connexion()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Connexion(string Pseudo, string Mdp)
        {
            var utilisateur = await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.Pseudo == Pseudo);

            if (utilisateur != null)
            {
                var result = _passwordHasher.VerifyHashedPassword(utilisateur, utilisateur.Mdp, Mdp);
                if (result == PasswordVerificationResult.Success)
                {
                    HttpContext.Session.SetInt32("UtilisateurId", utilisateur.Id);
                    HttpContext.Session.SetString("Role", utilisateur.Role);
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewData["Message"] = "Pseudo ou mot de passe incorrect.";
            return View();
        }

        public IActionResult Deconnexion()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Connexion");
        }

        public async Task<IActionResult> Compte()
        {
            var utilisateurId = HttpContext.Session.GetInt32("UtilisateurId");
            if (utilisateurId == null)
                return RedirectToAction("Connexion");

            var utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Id == utilisateurId);
            if (utilisateur == null)
                return NotFound();

            return View(utilisateur);
        }

        [HttpGet]
        public async Task<IActionResult> ModifierMDP()
        {
            var utilisateurId = HttpContext.Session.GetInt32("UtilisateurId");
            if (utilisateurId == null)
                return RedirectToAction("Connexion");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModifierMDP(string ancienMdp, string nouveauMdp, string confirmationMdp)
        {
            var utilisateurId = HttpContext.Session.GetInt32("UtilisateurId");
            if (utilisateurId == null)
                return RedirectToAction("Connexion");

            var utilisateur = await _context.Utilisateurs.FindAsync(utilisateurId);
            if (utilisateur == null)
                return NotFound();

            var result = _passwordHasher.VerifyHashedPassword(utilisateur, utilisateur.Mdp, ancienMdp);
            if (result != PasswordVerificationResult.Success)
            {
                ViewData["Message"] = "Mot de passe actuel incorrect.";
                return View();
            }

            if (nouveauMdp != confirmationMdp)
            {
                ViewData["Message"] = "Les mots de passe ne correspondent pas.";
                return View();
            }

            if (nouveauMdp.Length < 8 || !nouveauMdp.Any(char.IsUpper) || !nouveauMdp.Any(char.IsDigit))
            {
                ViewData["Message"] = "Le nouveau mot de passe doit contenir au moins 8 caractères, une majuscule et un chiffre.";
                return View();
            }

            utilisateur.Mdp = _passwordHasher.HashPassword(utilisateur, nouveauMdp);
            await _context.SaveChangesAsync();

            ViewData["Message"] = "Mot de passe modifié avec succès.";
            return View();
        }
    }
}