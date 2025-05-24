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
            var setlist = await _context.Setlists.Include(s => s.Songs.OrderBy(song => song.Order)).FirstOrDefaultAsync(s => s.Id == id);

            if(setlist == null)
            {
                return NotFound();
            }

            var detailsVm = new SetlistDetailsViewModel
            {
                SetlistId = setlist.Id,
                Name = setlist.Name,
                Songs = setlist.Songs.Select(song => new SetlistDetailsViewModel.SongItem
                {
                    Id = song.Id,
                    Title = song.Title,
                    Duration = song.Duration,
                    Order = song.Order
                }).ToList()
            };

            return View(detailsVm);
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


    }
}

