using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarListApp.Data;
using StarListApp.Models;
using StarListApp.ViewModels;

namespace StarListApp.Controllers
{
    public class SetlistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<StarListUser> _userManager;

        public SetlistController(ApplicationDbContext context, UserManager<StarListUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var setlists = await _context.Setlists
                .Where(s => s.UserId == user.Id)
                .Include(s => s.Songs)
                .ToListAsync();

            setlists ??= new List<Setlist>();

            return View(setlists);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Setlist setlist)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            setlist.UserId = user.Id;
            _context.Setlists.Add(setlist);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var setlist = await _context.Setlists
                .FirstOrDefaultAsync(s => s.Id == id);

            if (setlist == null || setlist.UserId != _userManager.GetUserId(User))
                return NotFound();

            var viewModel = new EditSetlistViewModel
            {
                Id = setlist.Id,
                Name = setlist.Name
            };

            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(EditSetlistViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            var setlist = await _context.Setlists.FindAsync(viewModel.Id);

            if (setlist == null || setlist.UserId != _userManager.GetUserId(User))
                return NotFound();

            setlist.Name = viewModel.Name;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var setlist = await _context.Setlists
                .Include(s => s.Songs)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (setlist == null || setlist.UserId != _userManager.GetUserId(User))
                return NotFound();

            var viewModel = new SetlistDetailsViewModel
            {
                SetlistId = setlist.Id,
                Name = setlist.Name,
                Songs = setlist.Songs
                    .OrderBy(s => s.Order)
                    .Select(s => new SetlistDetailsViewModel.SongItem
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Duration = s.Duration,
                        BPM = s.BPM,
                        Key = s.Key,
                    }).ToList()
            };

            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateSongs([FromBody] List<SetlistDetailsViewModel.SongItem> updatedSongs)
        {
            foreach (var songVm in updatedSongs)
            {
                var song = await _context.Songs.FindAsync(songVm.Id);
                if (song != null)
                {
                    song.Title = songVm.Title;
                    song.Duration = songVm.Duration;
                    song.Order = songVm.Order;
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpGet]
        public IActionResult AddSong(int setlistId)
        {
            var viewModel = new AddSongViewModel
            {
                SetlistId = setlistId
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddSong(AddSongViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Details", new { id = model.SetlistId });
            }

            var userId = _userManager.GetUserId(User);
            var setlist = await _context.Setlists.FindAsync(model.SetlistId);
            if (setlist == null || setlist.UserId != userId)
                return NotFound();


            var song = new Song
            {
                Title = model.Title,
                Artist = model.Artist,
                Duration = model.Duration,
                BPM = model.BPM,
                Key = model.Key,
                Order = model.Order,
                SetlistId = model.SetlistId,
                UserId = userId
            };

            _context.Songs.Add(song);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = model.SetlistId });
        }





    }
}

