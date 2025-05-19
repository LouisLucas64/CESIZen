
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CESIZenModel.Context;
using CESIZenModel.Entities;
using Microsoft.AspNetCore.Authorization;


namespace CesiNewsBackOfficeMVC.Controllers
{
    public class EmotionController : Controller
    {
        private readonly NewsDbContext _context;

        
        public EmotionController(NewsDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> ListeEmotion()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin")
                //return Forbid(); 
                RedirectToAction("Index", "Home");

            var emotions = await _context.Emotions.ToListAsync();
            return View(emotions);
        }

        //pas utile
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var emotion = await _context.Emotions.FindAsync(id);
            return emotion == null ? NotFound() : View(emotion);
        }

        public IActionResult AjouterEmotion() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AjouterEmotion(Emotion emotion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(emotion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ListeEmotion));
            }
            return View(emotion);
        }

        public async Task<IActionResult> ModifierEmotion(int? id)
        {
            if (id == null) return NotFound();
            var emotion = await _context.Emotions.FindAsync(id);
            return emotion == null ? NotFound() : View(emotion);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ModifierEmotion(int id, Emotion emotion)
        {
            if (id != emotion.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(emotion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ListeEmotion));
            }
            return View(emotion);
        }

        public async Task<IActionResult> SupprimerEmotion(int id)
        {
            var emotion = await _context.Emotions.FindAsync(id);
            if (emotion == null)
            {
                return NotFound();
            }

            // Vérifie si cette émotion est utilisée dans un tracker
            bool estUtilisee = await _context.Trackers.AnyAsync(t => t.EmotionId == id);
            if (estUtilisee)
            {
                TempData["Erreur"] = "Impossible de supprimer cette émotion car elle est utilisée dans au moins un tracker.";
                return RedirectToAction(nameof(ListeEmotion));
            }

            _context.Emotions.Remove(emotion);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Émotion supprimée avec succès.";
            return RedirectToAction(nameof(ListeEmotion));
        }
    }
}
