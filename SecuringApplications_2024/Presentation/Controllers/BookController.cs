using Common.Models;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Presentation.ActionFilters;
using Presentation.Models.ViewModels;
using Presentation.Utilities;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Presentation.Controllers
{
    [Authorize]
    public class BookController : Controller
    {
        private BooksRepository _booksRepository;
        private CategoriesRepository _categoriesRepository;

        public BookController(BooksRepository booksRepository, CategoriesRepository categoriesRepository)
        {
            this._booksRepository = booksRepository;
            this._categoriesRepository = categoriesRepository;
        }

        public async Task<IActionResult> Index([FromServices] UserManager<CustomUser> userRepository, [FromServices] Encryption encryptionTool)
        {
            string passwordHash = (await userRepository.GetUserAsync(User)).Id.ToString();
            //Encryption encryptionTool = new Encryption();

            IQueryable<Book> books = this._booksRepository.GetAllBooks();
            List<BookViewModel> bookViewModels = (from book in books
                                                         select new BookViewModel()
                                                         {
                                                             EncryptedId = HttpUtility.UrlEncode(
                                                                 Convert.ToBase64String(encryptionTool.SymmetricEncrypt(
                                                                     System.Text.UTF32Encoding.UTF32.GetBytes(
                                                                         Convert.ToString(book.Id)), passwordHash
                                                                 ))),
                                                             Id = book.Id,
                                                             Name = book.Name,
                                                             Author = book.Author,
                                                             Year = book.Year,
                                                             Filename = book.Filename,
                                                             CategoryFK = book.CategoryFK,
                                                             CategoryName = book.Category.Name
                                                         }).ToList();

            return View(bookViewModels);
        }

        [Authorize(Roles = "Librarian,Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            BookViewModel bookViewModel = new BookViewModel(_categoriesRepository); 
            return View(bookViewModel);
        }

        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian,Admin")]
        [HttpPost]
        public IActionResult Create(BookViewModel bookViewModel, [FromServices] IWebHostEnvironment host, [FromServices] UserManager<CustomUser> usersRepository, [FromServices] Encryption encryptionTool, [FromServices] PermissionsRepository permissionsRepository)
        {
            ModelState.Remove("EncryptedId");
            ModelState.Remove("Id");
            ModelState.Remove("Categories");
            ModelState.Remove("Filename");
            ModelState.Remove("EncryptedSymmetricKey");
            ModelState.Remove("EncryptedSymmetricIV");
            ModelState.Remove("DigitalSignature");
            ModelState.Remove("CategoryName");
   
            bookViewModel.Filename = string.Empty;

            string message = string.Empty;

            if (ModelState.IsValid)
            {
                if (bookViewModel.File.Length > 0 && bookViewModel.File.Length < (10 * 1024 * 1024))
                {
                    int[] pdfWhiteList = new int[] { 37, 80, 68, 70 };
                    bool fileCheck = true;
                    int counter = 0;
                    int readByte;

                    using (Stream stream = bookViewModel.File.OpenReadStream())
                    {
                        do
                        {
                            readByte = stream.ReadByte();
                            if (readByte == -1)
                            {
                                fileCheck = false;
                            }
                            if (pdfWhiteList[counter] != readByte)
                            {
                                fileCheck = false;
                            }

                            counter++;
                        }
                        while (fileCheck && counter < pdfWhiteList.Length);

                        stream.Position = 0;
                    }


                    if (!fileCheck)
                    {
                        TempData["error"] = "File not allowed, only PDFs are!";
                        ModelState.AddModelError("File", "File not allowed, only PDFs are!");
                    }
                    else
                    {
                        var user = usersRepository.GetUserAsync(User).Result;

                        if (user == null)
                        {
                            TempData["error"] = "User not found!";
                            ModelState.AddModelError("", "User not found!");
                            bookViewModel.Categories = this._categoriesRepository.GetCategories();
                            return View(bookViewModel);
                        }

                        string encryptionPassword = user.Id;
                        byte[] encryptedPrivateKeyBytes = Convert.FromBase64String(user.EncryptedPrivateKey);
                        byte[] decryptedPrivateKeyBytes = encryptionTool.SymmetricDecrypt(encryptedPrivateKeyBytes, encryptionPassword);
                        string userPrivateKey = Encoding.UTF8.GetString(decryptedPrivateKeyBytes);

                        byte[] originalFileBytes;
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            bookViewModel.File.CopyTo(memoryStream);
                            originalFileBytes = memoryStream.ToArray();
                        }

                        var (encryptedStream, encryptedSymmetricKeyBase64, encryptedIVBase64) = encryptionTool.HybridEncrypt(originalFileBytes, user.PublicKeyXml);

                        string filename = Guid.NewGuid().ToString() + Path.GetExtension(bookViewModel.File.FileName);

                        if (!Directory.Exists(Path.Combine(host.ContentRootPath, "data")))
                        {
                            Directory.CreateDirectory(Path.Combine(host.ContentRootPath, "data"));
                        }

                        string absolutePath = Path.Combine(host.ContentRootPath, "data", filename);

                        System.IO.File.WriteAllBytes(absolutePath, encryptedStream.ToArray());

                        bookViewModel.Filename = Path.Combine("data", filename);
                        message += "File uploaded and encrypted with hybrid encryption successfully; ";

                        byte[] digitalSignature = encryptionTool.DigitalSign(originalFileBytes, userPrivateKey);
                        string digitalSignatureBase64 = Convert.ToBase64String(digitalSignature);

                        Book book = new Book()
                        {
                            Name = bookViewModel.Name,
                            Author = bookViewModel.Author,
                            Filename = bookViewModel.Filename,
                            Year = bookViewModel.Year,
                            CategoryFK = bookViewModel.CategoryFK,
                            DigitalSignature = digitalSignatureBase64,
                            EncryptedSymmetricKey = encryptedSymmetricKeyBase64,
                            EncryptedSymmetricIV = encryptedIVBase64
                        };

                        this._booksRepository.AddBook(book);

                        permissionsRepository.AddPermissions(new Permission()
                        {
                            BookIdFK = book.Id,
                            UserIdFK = user.Id,
                            Read = true,
                            Write = false,
                            IpAddress = HttpContext.Connection.RemoteIpAddress.ToString()
                        });

                        message += "Details saved in database successfully!";
                        TempData["message"] = message;
                    }
                }
                else
                {
                    TempData["error"] = "File size not allowed, file size must be lower than 10 MB!";
                    ModelState.AddModelError("File", "File size not allowed, file size must be lower than 10 MB!");
                }
            }

            bookViewModel.Categories = this._categoriesRepository.GetCategories();
            return View(bookViewModel);
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(string id, [FromServices] IHostEnvironment host, [FromServices] Encryption encryptionTool, [FromServices] UserManager<CustomUser> usersRepository, [FromServices] PermissionsRepository permissionsRepository) 
        {
            string safeInput = HttpUtility.UrlDecode(id);
            int originalId = 0;

            try
            {
                byte[] cipherAsBytes = Convert.FromBase64String(safeInput);

                string passwordHash = (await usersRepository.GetUserAsync(User)).Id.ToString();
                byte[] originalDataAsBytes = encryptionTool.SymmetricDecrypt(cipherAsBytes, passwordHash);

                originalId = Convert.ToInt32(UTF32Encoding.UTF32.GetString(originalDataAsBytes));

                Book book = this._booksRepository.GetBook(originalId);

                if (book != null)
                {
                    IQueryable<Permission> permissions = this._booksRepository.GetBookPermissions(book.Id);

                    if (permissions.Count() > 0)
                    {
                        if(permissions.Any(permission => permission.User.UserName == User.Identity.Name))
                        {
                            Permission userPermission = permissions.SingleOrDefault();
                            permissionsRepository.DeletePermissions(userPermission);
                        }
                        else
                        {
                            TempData["error"] = "You cannot delete this book because you have permissions associated with it.";
                            return RedirectToAction("Index", "Book");
                        }
                    }

                    this._booksRepository.DeleteBook(book);
                    string absolutePath = Path.Combine(host.ContentRootPath, book.Filename);

                    if (System.IO.File.Exists(absolutePath))
                        System.IO.File.Delete(absolutePath);

                    TempData["message"] = "Book deleted!";
                }
                else
                {
                    TempData["error"] = "Book not in database!";
                }
            }
            catch(Exception)
            {
                TempData["error"] = "Failed to read the ID!";
            }

            return RedirectToAction("Index", "Book");
        }

        [BookAccessActionFilter(true)]
        public IActionResult Download(string bookId, [FromServices] IHostEnvironment host, [FromServices] Encryption encryptionTool, [FromServices] UserManager<CustomUser> usersRepository)
        {
            if (!HttpContext.Items.TryGetValue("DecryptedBookId", out object idObj) || !(idObj is int originalId))
            {
                TempData["error"] = "Invalid or missing book ID.";
                return RedirectToAction("Index", "Book");
            }

            Book book = this._booksRepository.GetBook(originalId);

            if (book != null) 
            {
                string absolutePathToBook = Path.Combine(host.ContentRootPath, book.Filename);
                byte[] encryptedFileBytes = System.IO.File.ReadAllBytes(absolutePathToBook);

                CustomUser user = usersRepository.GetUserAsync(User).Result;

                if (user == null)
                {
                    TempData["error"] = "User not found.";
                    return RedirectToAction("Index", "Book");
                }

                byte[] encryptedPrivateKeyBytes = Convert.FromBase64String(user.EncryptedPrivateKey);
                byte[] decryptedPrivateKeyBytes = encryptionTool.SymmetricDecrypt(encryptedPrivateKeyBytes, user.Id);
                string userPrivateKeyXml = Encoding.UTF8.GetString(decryptedPrivateKeyBytes);

                byte[] encryptedSymmetricKey = Convert.FromBase64String(book.EncryptedSymmetricKey);
                byte[] encryptedIV = Convert.FromBase64String(book.EncryptedSymmetricIV);

                byte[] aesKey = encryptionTool.AsymmetricDecrypt(encryptedSymmetricKey, userPrivateKeyXml);
                byte[] aesIV = encryptionTool.AsymmetricDecrypt(encryptedIV, userPrivateKeyXml);

                byte[] decryptedFileBytes;
                using (Aes aes = Aes.Create())
                {
                    aes.Key = aesKey;
                    aes.IV = aesIV;

                    using (MemoryStream input = new MemoryStream(encryptedFileBytes))
                    using (CryptoStream cryptoStream = new CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    using (MemoryStream output = new MemoryStream())
                    {
                        cryptoStream.CopyTo(output);
                        decryptedFileBytes = output.ToArray();
                    }
                }

                if (!string.IsNullOrEmpty(book.DigitalSignature))
                {
                    byte[] signature = Convert.FromBase64String(book.DigitalSignature);
                    bool isValid = encryptionTool.DigitalVerify(decryptedFileBytes, signature, user.PublicKeyXml);

                    if (!isValid)
                    {
                        TempData["error"] = "File integrity check failed. The file may have been tampered with.";
                        return RedirectToAction("Index", "Book");
                    }
                }
                else
                {
                    TempData["error"] = "Missing digital signature. Cannot verify file integrity.";
                    return RedirectToAction("Index", "Book");
                }

                return File(decryptedFileBytes, "application/pdf", Guid.NewGuid() + Path.GetExtension(book.Filename));
            }
            else
            {
                TempData["error"] = "Book not found!";
                return RedirectToAction("Index", "Book");
            }
        }
    }
}