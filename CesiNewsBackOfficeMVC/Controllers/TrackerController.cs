using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CESIZenModel.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using CESIZenModel.Context;
using Microsoft.AspNetCore.Authorization;

namespace CESIZenBackOfficeMVC.Controllers
{
    public class TrackerController : Controller
    {
        private readonly NewsDbContext _context;

        //[Authorize]
        public TrackerController(NewsDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> JournalDeBord()
        {
            var utilisateurId = HttpContext.Session.GetInt32("UtilisateurId");
            if (utilisateurId == null)
                return RedirectToAction("Connexion", "Utilisateur");

            var trackers = await _context.Trackers
                .Include(t => t.Emotion)
                .Where(t => t.UtilisateurId == utilisateurId)
                .ToListAsync();

            return View(trackers);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var tracker = await _context.Trackers
                .Include(t => t.Utilisateur)
                .Include(t => t.Emotion)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tracker == null) return NotFound();

            return View(tracker);
        }

        public IActionResult AjouterTracker() 
        {
            

            var emotions = _context.Emotions
            .Select(e => new
            {
                Id = e.Id,
                Display = e.Nom_Emotion_Niveau1 + " - " + e.Nom_Emotion_Niveau2
            })
            .ToList();

            ViewData["EmotionsSelect"] = new SelectList(emotions, "Id", "Display");
            return View(new Tracker());

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AjouterTracker(Tracker tracker)
        {

            bool champManquant = false;

            if (string.IsNullOrWhiteSpace(tracker.Titre))
            {
                ViewData["MessageTitre"] = "Le titre est requis.";
                champManquant = true;
            }

            if (tracker.Note_Intensite == 0)
            {
                ViewData["MessageIntensite"] = "L’intensité doit être renseignée.";
                champManquant = true;
            }

            if (tracker.EmotionId == 0)
            {
                ViewData["MessageEmotion"] = "Veuillez sélectionner une émotion.";
                champManquant = true;
            }

            if (string.IsNullOrWhiteSpace(tracker.Commentaire))
            {
                ViewData["MessageCommentaire"] = "Le commentaire est requis.";
                champManquant = true;
            }

            if (champManquant)
            {
                ViewData["EmotionsSelect"] = new SelectList(
                    _context.Emotions.Select(e => new
                    {
                        e.Id,
                        NomComplet = e.Nom_Emotion_Niveau1 + " - " + e.Nom_Emotion_Niveau2
                    }),
                    "Id",
                    "NomComplet",
                    tracker.EmotionId
                );

                return View(tracker);
            }

            var emotion = await _context.Emotions.FindAsync(tracker.EmotionId);
            if (emotion == null)
            {
                ViewData["MessageEmotion"] = "Veuillez séléctionner une émotion.";
                ViewData["EmotionsSelect"] = new SelectList(
                    _context.Emotions.Select(e => new
                    {
                        e.Id,
                        NomComplet = e.Nom_Emotion_Niveau1 + " - " + e.Nom_Emotion_Niveau2
                    }),
                    "Id",
                    "NomComplet",
                    tracker.EmotionId
                );
                return View(tracker);
            }

            tracker.Emotion_Niveau1 = emotion.Nom_Emotion_Niveau1;
            tracker.Emotion_Niveau2 = emotion.Nom_Emotion_Niveau2;
            tracker.Description = emotion.Description;
            tracker.Date_Creation = DateTime.Now;

            var utilisateurId = HttpContext.Session.GetInt32("UtilisateurId");
            if (utilisateurId == null)
            {
                return RedirectToAction("Connexion", "Utilisateur");
            }

            tracker.UtilisateurId = utilisateurId.Value;

            _context.Add(tracker);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(JournalDeBord));
        }



        public async Task<IActionResult> ModifierTracker(int? id)
        {
            if (id == null) return NotFound();

            var tracker = await _context.Trackers
                .Include(t => t.Emotion)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tracker == null) return NotFound();

            var emotions = _context.Emotions
                .Select(e => new
                {
                    Id = e.Id,
                    Display = e.Nom_Emotion_Niveau1 + " - " + e.Nom_Emotion_Niveau2
                })
                .ToList();

            ViewData["EmotionsSelect"] = new SelectList(emotions, "Id", "Display", tracker.EmotionId);

            return View(tracker);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModifierTracker(int id, Tracker tracker)
        {

            if (id != tracker.Id)
                return NotFound();



            var utilisateurId = HttpContext.Session.GetInt32("UtilisateurId");
            if (utilisateurId == null)
                return RedirectToAction("Connexion", "Utilisateur");


            // Validation manuelle
            bool champInvalide = false;

            if (string.IsNullOrWhiteSpace(tracker.Titre))
            {
                ViewData["MessageTitre"] = "Le titre est requis.";
                champInvalide = true;
            }

            if (string.IsNullOrWhiteSpace(tracker.Commentaire))
            {
                ViewData["MessageCommentaire"] = "Le commentaire est requis.";
                champInvalide = true;
            }

            if (champInvalide)
            {
                var emotionsErreur = _context.Emotions
                    .Select(e => new
                    {
                        Id = e.Id,
                        Display = e.Nom_Emotion_Niveau1 + " - " + e.Nom_Emotion_Niveau2
                    })
                    .ToList();

                ViewData["EmotionsSelect"] = new SelectList(emotionsErreur, "Id", "Display", tracker.EmotionId);
                return View(tracker);
            }



            if (ModelState.IsValid)
            {
                try
                {
                    var trackerExistant = await _context.Trackers.FindAsync(id);
                    if (trackerExistant == null)
                        return NotFound();

                    // Mise à jour uniquement des champs modifiables
                    trackerExistant.Titre = tracker.Titre;
                    trackerExistant.EmotionId = tracker.EmotionId;
                    trackerExistant.Note_Intensite = tracker.Note_Intensite;
                    trackerExistant.Commentaire = tracker.Commentaire;

                    // Met à jour les infos liées à l’émotion choisie
                    var emotion = await _context.Emotions.FindAsync(tracker.EmotionId);
                    if (emotion != null)
                    {
                        trackerExistant.Emotion_Niveau1 = emotion.Nom_Emotion_Niveau1;
                        trackerExistant.Emotion_Niveau2 = emotion.Nom_Emotion_Niveau2;
                        trackerExistant.Description = emotion.Description;
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction("JournalDeBord");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Trackers.Any(e => e.Id == id))
                        return NotFound();
                    else
                        throw;
                }
            }

            // En cas d’erreur, on recharge la liste déroulante
            var emotions = _context.Emotions
                .Select(e => new
                {
                    Id = e.Id,
                    Display = e.Nom_Emotion_Niveau1 + " - " + e.Nom_Emotion_Niveau2
                })
                .ToList();

            ViewData["EmotionsSelect"] = new SelectList(emotions, "Id", "Display", tracker.EmotionId);

            return View(tracker);
        }

        [HttpPost]
        public async Task<IActionResult> SupprimerTracker(int id)
        {
            var tracker = await _context.Trackers.FindAsync(id);
            if (tracker != null)
            {
                _context.Trackers.Remove(tracker);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(JournalDeBord));
        }
   }
}