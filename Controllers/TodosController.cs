using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;


namespace DotNetCoreSqlDb.Controllers
{
    public class TodosController : Controller
    {
        private readonly MyDatabaseContext _context;
        private readonly IConfiguration _configuration;
        private readonly IConfigurationSection _azureSection;
        private readonly string _baseUri;
        private readonly CloudBlobContainer _container;
        public TodosController(MyDatabaseContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _azureSection = _configuration.GetSection("Azure:Storage");
            _baseUri = _azureSection.GetValue<string>("BaseUri");

            CloudStorageAccount storageAccount = new CloudStorageAccount(
                        new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
                        _azureSection.GetValue<string>("AccountName"),
                        _azureSection.GetValue<string>("Key")), true);

            // Create a blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            // Get a reference to a container named "mycontainer."
            _container = blobClient.GetContainerReference("images");
           
        }

        // GET: Todos
        public async Task<IActionResult> Index()
        {
            return View(await _context.Todo.ToListAsync());
        }

        // GET: Todos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var todo = await _context.Todo
                .FirstOrDefaultAsync(m => m.ID == id);
            if (todo == null)
            {
                return NotFound();
            }


           var signature =  _container.GetSharedAccessSignature(new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-15),
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(15)
            });


            return View(new TodoViewModel
            {
                ID = todo.ID,
                CreatedDate = todo.CreatedDate,
                Description = todo.Description,
                UploadPicUrl = todo.UploadedImageId != null ? $"{_baseUri}images/{todo.UploadedImageId}{signature}" : string.Empty
            });
        }

        // GET: Todos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Todos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TodoModel todo)
        {
            if (ModelState.IsValid)
            {


                await _container.CreateIfNotExistsAsync();
                //To make the files within the container available to everyone, set the container to be public:
                //await container.SetPermissionsAsync(new BlobContainerPermissions
                //{
                //    PublicAccess = BlobContainerPublicAccessType.Blob
                //});

                // Get a reference to a blob named "myblob".
                var imageId = Guid.NewGuid().ToString();
                CloudBlockBlob blockBlob = _container.GetBlockBlobReference(imageId);
                // Create or overwrite the "myblob" blob with the contents of a local file
                // named "myfile".
                using (var fileStream = todo.UploadPic.OpenReadStream())
                {
                    await blockBlob.UploadFromStreamAsync(fileStream);
                }


                _context.Add(new Todo
                {
                    Description= todo.Description,
                    CreatedDate = todo.CreatedDate,
                    ID = todo.ID,
                    UploadedImageId= imageId
                });
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(todo);
        }

        // GET: Todos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var todo = await _context.Todo.FindAsync(id);
            if (todo == null)
            {
                return NotFound();
            }
            return View(todo);
        }

        // POST: Todos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Description,CreatedDate")] Todo todo)
        {
            if (id != todo.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(todo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TodoExists(todo.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(todo);
        }

        // GET: Todos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var todo = await _context.Todo
                .FirstOrDefaultAsync(m => m.ID == id);
            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);
        }

        // POST: Todos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var todo = await _context.Todo.FindAsync(id);
            _context.Todo.Remove(todo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TodoExists(int id)
        {
            return _context.Todo.Any(e => e.ID == id);
        }
    }
}
