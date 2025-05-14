using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CESIZenModel.Context;
using CESIZenModel.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CESIZenBackOfficeMVC.Controllers
{
    public class RapportEmotionController : Controller
    {
        private readonly NewsDbContext _context;

        public RapportEmotionController(NewsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GenererRapport()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenererRapport(DateTime dateDebut, DateTime dateFin)
        {
            var utilisateurId = HttpContext.Session.GetInt32("UtilisateurId");
            if (utilisateurId == null)
                return RedirectToAction("Connexion", "Utilisateur");

            // Récupère tous les trackers de l'utilisateur entre les deux dates
            dateDebut = dateDebut.Date;
            dateFin = dateFin.Date.AddDays(1).AddTicks(-1);

            var trackers = await _context.Trackers
                .Where(t => t.UtilisateurId == utilisateurId &&
                            t.Date_Creation >= dateDebut && t.Date_Creation <= dateFin)
                .Include(t => t.Emotion)
                .ToListAsync();

            if (!trackers.Any())
            {
                ViewData["Message"] = "Aucune donnée pour cette période.";
                return View("GenererRapport");
            }

            // Statistiques de base
            var total = trackers.Count;
            var moyenneIntensite = trackers.Average(t => t.Note_Intensite);

            // Émotions de niveau 1 et 2 les plus fréquentes
            var emotionNiv1 = trackers
                .GroupBy(t => t.Emotion_Niveau1)
                .OrderByDescending(g => g.Count())
                .First().Key;

            var emotionNiv2 = trackers
                .GroupBy(t => t.Emotion_Niveau2)
                .OrderByDescending(g => g.Count())
                .First().Key;

            var emotionMoinsFrequente = trackers
                .GroupBy(t => t.Emotion_Niveau2)
                .OrderBy(g => g.Count())
                .First().Key;

            // Création du rapport
            var rapport = new RapportEmotion
            {
                Date_Debut = dateDebut,
                Date_Fin = dateFin,
                Emotion_Predominante_Niv1 = emotionNiv1,
                Emotion_Predominante_Niv2 = emotionNiv2,
                Emotion_Moins_Frequente = emotionMoinsFrequente,
                Moyenne_Intensite = (float)moyenneIntensite,
                Total_Tracker = total,
                Date_Generation = DateTime.Now
            };

            _context.RapportEmotions.Add(rapport);
            await _context.SaveChangesAsync();

            // Lier les trackers au rapport
            foreach (var tracker in trackers)
            {
                _context.Tracker_Rapports.Add(new Tracker_Rapport
                {
                    Tracker = tracker,
                    Rapport = rapport
                });
            }
            await _context.SaveChangesAsync();

            return View("VisualiserRapport", rapport);
        }
    }
    
}