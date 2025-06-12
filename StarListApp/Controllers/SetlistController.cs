using Microsoft.AspNetCore.Authorization;
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
                Songs = setlist.Songs
                    .OrderBy(s => s.Order)
                    .Select(s => new SetlistDetailsViewModel.SongItem
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Duration = s.Duration.ToString(@"mm\:ss"),
                        BPM = s.BPM,
                        Key = s.Key,
                        Order = s.Order
                    }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSongs(SetlistDetailsViewModel model)
        {
            Console.WriteLine("UpdateSongs POST called.");
            Console.WriteLine($"Received {model.Songs.Count} songs in the form.");

            var userId = _userManager.GetUserId(User);
            var setlist = await _context.Setlists
                .Include(s => s.Songs)
                .FirstOrDefaultAsync(s => s.Id == model.SetlistId && s.UserId == userId);

            if (setlist == null)
                return NotFound();

            // Validate all durations
            var parsedDurations = new Dictionary<int, TimeSpan>(); // for existing songs
            var newSongDurations = new List<TimeSpan>(); // for new songs 

            for (int i = 0; i < model.Songs.Count; i++)
            {
                var songVm = model.Songs[i];
                var durationInput = songVm.Duration?.Trim();
                var durationParts = durationInput?.Split(':');

                if (durationParts == null || durationParts.Length != 2 ||
                    !int.TryParse(durationParts[0], out int minutes) ||
                    !int.TryParse(durationParts[1], out int seconds) ||
                    minutes < 0 || seconds < 0 || seconds >= 60 || minutes >= 60)
                {
                    ModelState.AddModelError($"Duration-{songVm.Title}", $"Invalid duration for song '{songVm.Title}'. Use mm:ss format.");
                }
                else
                {
                    var parsedDuration = new TimeSpan(0, minutes, seconds);
                    if (songVm.Id != 0)
                    {
                        parsedDurations[songVm.Id] = parsedDuration;
                    }
                    else
                    {
                        newSongDurations.Add(parsedDuration);
                    }
                }
            }

           if (!ModelState.IsValid)
            {
                model.Songs = setlist.Songs.OrderBy(s => s.Order).Select(s => new SetlistDetailsViewModel.SongItem
                {
                    Id = s.Id,
                    Title = s.Title,
                    Duration = s.Duration.Hours > 0 ? s.Duration.ToString(@"hh\:mm\:ss") : s.Duration.ToString(@"mm\:ss"),
                    BPM = s.BPM,
                    Key = s.Key,
                    Order = s.Order
                }).ToList();

                return View("Details", model);
            }


            // Remove songs
            var postedSongIds = model.Songs.Where(s => s.Id != 0).Select(s => s.Id).ToList();
            var songsToRemove = setlist.Songs.Where(s => !postedSongIds.Contains(s.Id)).ToList();
            _context.Songs.RemoveRange(songsToRemove);

            // Add or update songs
            Console.WriteLine($"Received {model.Songs.Count} songs in the form.");
            foreach (var songVm in model.Songs)
            {
                Console.WriteLine($"Song: Id={songVm.Id}, Title='{songVm.Title}', Duration={songVm.Duration}");
            }

            int newSongIndex = 0;
            foreach (var songVm in model.Songs)
            {
                if (songVm.Id != 0)
                {
                    // Update existing song
                    var existingSong = setlist.Songs.FirstOrDefault(s => s.Id == songVm.Id);
                    if (existingSong != null)
                    {
                        existingSong.Title = songVm.Title;
                        existingSong.Duration = parsedDurations[songVm.Id];
                        existingSong.BPM = songVm.BPM;
                        existingSong.Key = songVm.Key;
                        existingSong.Order = songVm.Order;
                        existingSong.SetlistId = songVm.SetlistId;
                    }
                }
                else
                {
                    // Add new song
                    if (!string.IsNullOrWhiteSpace(songVm.Title))
                    {
                        var newSong = new Song
                        {
                            Title = songVm.Title,
                            Duration = newSongDurations[newSongIndex],
                            BPM = songVm.BPM,
                            Key = songVm.Key,
                            Order = songVm.Order,
                            UserId = userId,
                            SetlistId = setlist.Id
                        };
                        _context.Songs.Add(newSong);
                        newSongIndex++;
                    }
                }
            }

            await _context.SaveChangesAsync();

            var debugSongs = await _context.Songs.Where(s => s.SetlistId == setlist.Id).ToListAsync();
            Console.WriteLine($"Setlist has {debugSongs.Count} songs after saving.");

            return RedirectToAction("Details", new { id = setlist.Id });
        }

        [HttpPost]
        public async Task<IActionResult> TogglePublic(int id)
        {
            var userId = _userManager.GetUserId(User);
            var setlist = await _context.Setlists.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if(setlist == null)
            {
                return NotFound();
            }

            setlist.IsPublic = !setlist.IsPublic;

            await _context.SaveChangesAsync();

            return Redirect("Index");

        }

        [HttpGet]
        public async Task<IActionResult> Public()
        {
            var publicSetlists = await _context.Setlists
                .Include(s => s.User)
                .Where(s => s.IsPublic)
                .ToListAsync();

            return View(publicSetlists);
        }

        [AllowAnonymous]
        public async Task<IActionResult> PublicDetails(int id)
        {
            var setlist = await _context.Setlists
                .Include(s => s.Songs.OrderBy(song => song.Order))
                .FirstOrDefaultAsync(s => s.Id == id && s.IsPublic);

            if (setlist == null)
                return NotFound();

            var viewModel = new SetlistDetailsViewModel
            {
                SetlistId = setlist.Id,
                Songs = setlist.Songs
                    .OrderBy(s => s.Order)
                    .Select(s => new SetlistDetailsViewModel.SongItem
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Duration = s.Duration.ToString(@"mm\:ss"),
                        BPM = s.BPM,
                        Key = s.Key,
                        Order = s.Order,
                        SetlistId = s.SetlistId
                    }).ToList()
            };


            return View(viewModel);
        }

    }
}
