using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MYAcademy_IdentityEmailProject.Context;
using MYAcademy_IdentityEmailProject.Entities;
using MYAcademy_IdentityEmailProject.Helpers;
using MYAcademy_IdentityEmailProject.Models;
using MYAcademy_IdentityEmailProject.Services;
using System.Net;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    // [Authorize] Program.cs'teki global filtreden gelir: tüm action'lar giriş ister.
    public class MessageController : Controller
    {
        private readonly EmailContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<MessageController> _logger;

        public MessageController(EmailContext context, UserManager<AppUser> userManager,
            IEmailService emailService, ILogger<MessageController> logger)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        // =====================================================================
        // KUTULAR
        // =====================================================================

        public Task<IActionResult> Inbox() => Box(MessageBoxes.Inbox);
        public Task<IActionResult> Sendbox() => Box(MessageBoxes.Sent);
        public Task<IActionResult> Starred() => Box(MessageBoxes.Starred);
        public Task<IActionResult> Drafts() => Box(MessageBoxes.Drafts);
        public Task<IActionResult> Trash() => Box(MessageBoxes.Trash);

        private async Task<IActionResult> Box(string box)
        {
            var me = await _userManager.GetUserAsync(User);
            if (me?.Email == null) return Challenge();
            var email = me.Email;

            var messages = await _context.Messages
                .AsNoTracking()
                .Include(m => m.Category)
                .ForBox(box, email)
                .OrderByDescending(m => m.SendDate)
                .ToListAsync();

            var users = await UsersByEmailAsync(messages.SelectMany(m => new[] { m.SenderEmail, m.ReceiverEmail }));

            var model = new MessageListViewModel
            {
                Box = box,
                Title = MessageBoxes.Title(box),
                UserEmail = email,
                Categories = await _context.Categories.AsNoTracking().OrderBy(c => c.CategoryName).ToListAsync(),
                Items = messages.Select(m => ToListItem(m, email, box, users)).ToList()
            };
            (model.Icon, model.EmptyTitle, model.EmptyText) = box switch
            {
                MessageBoxes.Sent => ("bi-send", "Henüz mesaj göndermediniz", "Gönderdiğiniz mesajlar burada listelenecek."),
                MessageBoxes.Starred => ("bi-star", "Yıldızlı mesajınız yok", "Önemli mesajları yıldızlayarak burada toplayabilirsiniz."),
                MessageBoxes.Drafts => ("bi-file-earmark-text", "Taslağınız yok", "Kaydettiğiniz taslaklar burada listelenecek."),
                MessageBoxes.Trash => ("bi-trash3", "Çöp kutusu boş", "Sildiğiniz mesajlar burada listelenir; geri alabilir veya kalıcı silebilirsiniz."),
                _ => ("bi-inbox", "Gelen kutunuz boş", "Yeni mesajlar geldiğinde burada listelenecek.")
            };

            return View("MessageList", model);
        }

        // =====================================================================
        // MESAJ DETAYI
        // =====================================================================

        public async Task<IActionResult> Detail(int id, string? box)
        {
            var me = await _userManager.GetUserAsync(User);
            if (me?.Email == null) return Challenge();
            var email = me.Email;

            var message = await _context.Messages.Include(m => m.Category).FirstOrDefaultAsync(m => m.MessageID == id);
            if (message == null) return NotFound();

            // Mesaj var ama bu kullanıcıya ait değil → 403
            var accessible = await _context.Messages.Accessible(email).AnyAsync(m => m.MessageID == id);
            if (!accessible) return Forbid();

            if (message.IsDraft) return RedirectToAction(nameof(CreateMessage), new { draftId = id });

            // Alıcı mesajı açtığında okundu olarak işaretle
            if (message.IsReceiver(email) && !message.IsRead)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
            }

            var isInTrash = message.IsInTrashFor(email);
            box = isInTrash ? MessageBoxes.Trash
                : MessageBoxes.All.Contains(box) && box != MessageBoxes.Drafts && box != MessageBoxes.Trash ? box!
                : message.IsReceiver(email) ? MessageBoxes.Inbox : MessageBoxes.Sent;

            // Aynı kutudaki önceki / sonraki mesajlar
            var ids = await _context.Messages.ForBox(box, email)
                .OrderByDescending(m => m.SendDate).Select(m => m.MessageID).ToListAsync();
            var index = ids.IndexOf(id);

            var nextIds = index >= 0 ? ids.Skip(index + 1).Take(4).ToList() : new List<int>();
            var nextMessages = await _context.Messages.AsNoTracking().Include(m => m.Category)
                .Where(m => nextIds.Contains(m.MessageID)).ToListAsync();

            var users = await UsersByEmailAsync(nextMessages.SelectMany(m => new[] { m.SenderEmail, m.ReceiverEmail })
                .Append(message.SenderEmail).Append(message.ReceiverEmail));
            users.TryGetValue(message.SenderEmail.ToLowerInvariant(), out var sender);
            users.TryGetValue(message.ReceiverEmail.ToLowerInvariant(), out var receiver);

            var model = new MessageDetailViewModel
            {
                Message = message,
                Box = box,
                BoxTitle = MessageBoxes.Title(box),
                SenderName = sender != null ? UiHelpers.FullName(sender.Name, sender.Surname, sender.Email) : message.SenderEmail,
                SenderImage = sender?.ImageUrl,
                ReceiverName = receiver != null ? UiHelpers.FullName(receiver.Name, receiver.Surname, receiver.Email) : message.ReceiverEmail,
                IsOutgoing = message.IsSender(email) && !message.IsReceiver(email),
                IsStarred = message.IsStarredFor(email),
                IsInTrash = isInTrash,
                SafeBody = MessageHtml.Sanitize(message.MessageDetail),
                Position = index + 1,
                Total = ids.Count,
                PrevId = index > 0 ? ids[index - 1] : null,
                NextId = index >= 0 && index < ids.Count - 1 ? ids[index + 1] : null,
                NextUp = nextIds.Select(nid => nextMessages.FirstOrDefault(m => m.MessageID == nid))
                    .Where(m => m != null).Select(m => ToListItem(m!, email, box, users)).ToList()
            };

            return View(model);
        }

        // =====================================================================
        // YENİ MESAJ / TASLAK / YANITLA / İLET
        // =====================================================================

        [HttpGet]
        public async Task<IActionResult> CreateMessage(int? draftId, int? replyTo, int? forwardId, string? to)
        {
            var me = await _userManager.GetUserAsync(User);
            if (me?.Email == null) return Challenge();
            var email = me.Email;

            var model = new ComposeMessageViewModel { ReceiverEmail = to };

            if (draftId.HasValue)
            {
                var draft = await _context.Messages.Drafts(email).FirstOrDefaultAsync(m => m.MessageID == draftId);
                if (draft == null) return await NotFoundOrForbidAsync(draftId.Value);

                model.Mode = "draft";
                model.MessageID = draft.MessageID;
                model.ParentMessageID = draft.ParentMessageID;
                model.ReceiverEmail = draft.ReceiverEmail;
                model.CategoryID = draft.CategoryID;
                model.MessageSubject = draft.MessageSubject;
                model.MessageDetail = MessageHtml.Sanitize(draft.MessageDetail);
            }
            else if (replyTo.HasValue || forwardId.HasValue)
            {
                var sourceId = replyTo ?? forwardId!.Value;
                var source = await _context.Messages.Accessible(email).FirstOrDefaultAsync(m => m.MessageID == sourceId);
                if (source == null || source.IsDraft) return await NotFoundOrForbidAsync(sourceId);

                var users = await UsersByEmailAsync(new[] { source.SenderEmail, source.ReceiverEmail });
                string NameOf(string mail) => users.TryGetValue(mail.ToLowerInvariant(), out var u)
                    ? $"{UiHelpers.FullName(u.Name, u.Surname, mail)} &lt;{WebUtility.HtmlEncode(mail)}&gt;"
                    : WebUtility.HtmlEncode(mail);

                var date = UiHelpers.LongDate(source.SendDate);
                var body = MessageHtml.Sanitize(source.MessageDetail);
                model.ParentMessageID = source.MessageID;
                model.CategoryID = source.CategoryID;

                if (replyTo.HasValue)
                {
                    model.Mode = "reply";
                    // Gelen mesaja yanıt → gönderene; kendi gönderdiğine yanıt → alıcıya
                    model.ReceiverEmail = source.IsSender(email) && !source.IsReceiver(email) ? source.ReceiverEmail : source.SenderEmail;
                    model.MessageSubject = WithPrefix("Re:", source.MessageSubject);
                    model.MessageDetail = $"<p><br></p><p>{date} tarihinde {NameOf(source.SenderEmail)} şunu yazdı:</p><blockquote>{body}</blockquote>";
                }
                else
                {
                    model.Mode = "forward";
                    model.MessageSubject = WithPrefix("Fwd:", source.MessageSubject);
                    model.MessageDetail = "<p><br></p><p>---------- İletilen mesaj ----------<br>" +
                                          $"<b>Kimden:</b> {NameOf(source.SenderEmail)}<br>" +
                                          $"<b>Tarih:</b> {date}<br>" +
                                          $"<b>Konu:</b> {WebUtility.HtmlEncode(source.MessageSubject)}<br>" +
                                          $"<b>Kime:</b> {NameOf(source.ReceiverEmail)}</p>{body}";
                }
            }

            await FillComposeViewBagAsync(me);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(ComposeMessageViewModel model, string? submitAction)
        {
            var me = await _userManager.GetUserAsync(User);
            if (me?.Email == null) return Challenge();
            var email = me.Email;
            var isDraft = submitAction == "draft";

            model.ReceiverEmail = model.ReceiverEmail?.Trim();
            model.MessageSubject = model.MessageSubject?.Trim();
            model.MessageDetail = MessageHtml.Sanitize(model.MessageDetail);   // XSS'e karşı temizle
            var plainText = MessageHtml.ToPlainText(model.MessageDetail);

            // Düzenlenen taslak gerçekten bu kullanıcıya mı ait?
            Message? entity = null;
            if (model.MessageID.HasValue)
            {
                entity = await _context.Messages.Drafts(email).FirstOrDefaultAsync(m => m.MessageID == model.MessageID);
                if (entity == null) return await NotFoundOrForbidAsync(model.MessageID.Value);
            }

            // Yanıtlanan / iletilen mesaj bu kullanıcıya ait değilse bağlantıyı kaldır
            if (model.ParentMessageID.HasValue &&
                !await _context.Messages.Accessible(email).AnyAsync(m => m.MessageID == model.ParentMessageID))
                model.ParentMessageID = null;

            if (isDraft)
            {
                // Taslakta alanlar zorunlu değil; ama tamamen boş taslak kaydedilmez
                foreach (var key in new[] { nameof(model.ReceiverEmail), nameof(model.MessageSubject), nameof(model.MessageDetail), nameof(model.CategoryID) })
                    ModelState.Remove(key);
                if (string.IsNullOrEmpty(model.ReceiverEmail) && string.IsNullOrEmpty(model.MessageSubject) && plainText.Length == 0)
                    ModelState.AddModelError(string.Empty, "Boş bir taslak kaydedilemez.");
            }
            else if (plainText.Length == 0)
            {
                ModelState.Remove(nameof(model.MessageDetail));
                ModelState.AddModelError(nameof(model.MessageDetail), "Mesaj içeriği boş olamaz.");
            }

            if (model.CategoryID.HasValue && !await _context.Categories.AnyAsync(c => c.CategoryID == model.CategoryID))
            {
                if (isDraft) model.CategoryID = null;
                else ModelState.AddModelError(nameof(model.CategoryID), "Geçersiz kategori.");
            }

            if (!ModelState.IsValid)
            {
                await FillComposeViewBagAsync(me);
                return View(model);
            }

            entity ??= new Message();
            entity.SenderEmail = email;
            entity.ReceiverEmail = model.ReceiverEmail ?? string.Empty;
            entity.MessageSubject = model.MessageSubject ?? string.Empty;
            entity.MessageDetail = model.MessageDetail ?? string.Empty;
            entity.CategoryID = model.CategoryID;
            entity.ParentMessageID = model.ParentMessageID;
            entity.SendDate = DateTime.Now;

            if (isDraft)
            {
                entity.IsDraft = true;
                if (entity.MessageID == 0) _context.Messages.Add(entity);
                await _context.SaveChangesAsync();
                TempData["Toast"] = "Taslak kaydedildi.";
                return RedirectToAction(nameof(Drafts));
            }

            var error = await DeliverAsync(entity, me);
            if (error != null)
            {
                ModelState.AddModelError(string.Empty, error);
                await FillComposeViewBagAsync(me);
                return View(model);
            }

            TempData["Toast"] = entity.IsExternal
                ? $"Mesaj {entity.ReceiverEmail} adresine e-posta olarak gönderildi."
                : "Mesajınız gönderildi.";
            return RedirectToAction(nameof(Sendbox));
        }

        // Detay sayfasındaki hızlı yanıt kutusu
        [HttpPost]
        public async Task<IActionResult> QuickReply(int id, string? text)
        {
            var me = await _userManager.GetUserAsync(User);
            if (me?.Email == null) return Challenge();
            var email = me.Email;

            var source = await _context.Messages.Accessible(email).FirstOrDefaultAsync(m => m.MessageID == id);
            if (source == null || source.IsDraft) return await NotFoundOrForbidAsync(id);

            if (string.IsNullOrWhiteSpace(text))
            {
                TempData["Toast"] = "Yanıt boş olamaz.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            var html = string.Join("", text.Trim().Split('\n').Select(line => $"<p>{WebUtility.HtmlEncode(line.TrimEnd('\r'))}</p>"));
            var reply = new Message
            {
                SenderEmail = email,
                ReceiverEmail = source.IsSender(email) && !source.IsReceiver(email) ? source.ReceiverEmail : source.SenderEmail,
                MessageSubject = WithPrefix("Re:", source.MessageSubject),
                MessageDetail = html,
                CategoryID = source.CategoryID,
                ParentMessageID = source.MessageID,
                SendDate = DateTime.Now
            };

            var error = await DeliverAsync(reply, me);
            TempData["Toast"] = error ?? "Yanıtınız gönderildi.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        // =====================================================================
        // DURUM İŞLEMLERİ (AJAX veya normal form)
        // =====================================================================

        [HttpPost]
        public Task<IActionResult> ToggleStar(int id, bool starred, string? returnUrl) =>
            Apply(new[] { id }, returnUrl, (m, email) => m.SetStar(email, starred),
                starred ? "Mesaj yıldızlandı." : "Yıldız kaldırıldı.");

        [HttpPost]
        public Task<IActionResult> MarkAsRead(int[] ids, string? returnUrl) =>
            Apply(ids, returnUrl, (m, email) => { if (m.IsReceiver(email)) m.IsRead = true; }, "Okundu olarak işaretlendi.");

        [HttpPost]
        public Task<IActionResult> MarkAsUnread(int[] ids, string? returnUrl) =>
            Apply(ids, returnUrl, (m, email) => { if (m.IsReceiver(email)) m.IsRead = false; }, "Okunmadı olarak işaretlendi.");

        [HttpPost]
        public Task<IActionResult> MoveToTrash(int[] ids, string? returnUrl) =>
            Apply(ids, returnUrl, (m, email) =>
            {
                if (m.IsDraft) m.DeletePermanently(email);   // taslaklar doğrudan silinir
                else m.SetTrash(email, true);
            }, "Çöp kutusuna taşındı.");

        [HttpPost]
        public Task<IActionResult> Restore(int[] ids, string? returnUrl) =>
            Apply(ids, returnUrl, (m, email) => m.SetTrash(email, false), "Mesaj geri alındı.");

        [HttpPost]
        public Task<IActionResult> DeleteForever(int[] ids, string? returnUrl) =>
            Apply(ids, returnUrl, (m, email) => m.DeletePermanently(email), "Kalıcı olarak silindi.");

        private async Task<IActionResult> Apply(int[] ids, string? returnUrl, Action<Message, string> action, string doneMessage)
        {
            var me = await _userManager.GetUserAsync(User);
            if (me?.Email == null) return Unauthorized();
            var email = me.Email;

            ids ??= Array.Empty<int>();
            // Yalnızca kullanıcının erişebildiği mesajlar yüklenir; başkasına ait id'ler sessizce yok sayılır
            var messages = await _context.Messages.Accessible(email).Where(m => ids.Contains(m.MessageID)).ToListAsync();

            foreach (var m in messages)
            {
                action(m, email);
                if (m.CanBeRemoved()) _context.Messages.Remove(m);
            }
            await _context.SaveChangesAsync();

            if (IsAjax()) return Json(new { ok = true, count = messages.Count });

            if (ids.Length > 0 && messages.Count == 0) return await NotFoundOrForbidAsync(ids[0]);
            TempData["Toast"] = doneMessage;
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return LocalRedirect(returnUrl);
            return RedirectToAction(nameof(Inbox));
        }

        // =====================================================================
        // YARDIMCILAR
        // =====================================================================

        /// <summary>Alıcı uygulamada kayıtlıysa gelen kutusuna, değilse SMTP ile gerçek adrese gönderir.</summary>
        private async Task<string?> DeliverAsync(Message message, AppUser sender)
        {
            var receiverUser = await _userManager.FindByEmailAsync(message.ReceiverEmail);
            message.IsDraft = false;
            message.IsRead = false;
            message.IsExternal = receiverUser == null;

            if (message.IsExternal)
            {
                try
                {
                    var senderName = UiHelpers.FullName(sender.Name, sender.Surname, sender.UserName);
                    await _emailService.SendAsync(message.ReceiverEmail, message.MessageSubject,
                        EmailTemplates.ExternalMessage(senderName, sender.Email!, MessageHtml.Sanitize(message.MessageDetail)),
                        sender.Email, senderName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Harici e-posta gönderilemedi: {Email}", message.ReceiverEmail);
                    return "Mesaj e-posta olarak gönderilemedi. SMTP ayarlarını kontrol edip tekrar deneyin.";
                }
            }

            if (message.MessageID == 0) _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return null;
        }

        private async Task FillComposeViewBagAsync(AppUser me)
        {
            ViewBag.Categories = new SelectList(
                await _context.Categories.AsNoTracking().OrderBy(c => c.CategoryName).ToListAsync(), "CategoryID", "CategoryName");
            ViewBag.UserFullName = UiHelpers.FullName(me.Name, me.Surname, me.UserName);
            ViewBag.UserEmail = me.Email;
            ViewBag.UserImage = me.ImageUrl;
        }

        private async Task<Dictionary<string, AppUser>> UsersByEmailAsync(IEnumerable<string> emails)
        {
            var list = emails.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.ToUpperInvariant()).Distinct().ToList();
            var users = await _userManager.Users.AsNoTracking()
                .Where(u => u.NormalizedEmail != null && list.Contains(u.NormalizedEmail)).ToListAsync();
            return users.Where(u => u.Email != null)
                .GroupBy(u => u.Email!.ToLowerInvariant())
                .ToDictionary(g => g.Key, g => g.First());
        }

        private MessageListItem ToListItem(Message m, string email, string box, Dictionary<string, AppUser> users)
        {
            var outgoing = m.IsSender(email) && !m.IsReceiver(email);
            var other = outgoing || m.IsDraft ? m.ReceiverEmail : m.SenderEmail;
            users.TryGetValue((other ?? "").ToLowerInvariant(), out var u);

            return new MessageListItem
            {
                Id = m.MessageID,
                Url = m.IsDraft
                    ? Url.Action(nameof(CreateMessage), new { draftId = m.MessageID })!
                    : Url.Action(nameof(Detail), new { id = m.MessageID, box })!,
                CounterpartEmail = other ?? "",
                CounterpartName = u != null ? UiHelpers.FullName(u.Name, u.Surname, u.Email) : (string.IsNullOrEmpty(other) ? "(Alıcı yok)" : other),
                CounterpartImage = u?.ImageUrl,
                IsOutgoing = outgoing || m.IsDraft,
                Subject = string.IsNullOrWhiteSpace(m.MessageSubject) ? "(Konu yok)" : m.MessageSubject,
                Snippet = MessageHtml.Snippet(m.MessageDetail),
                Date = m.SendDate,
                IsUnread = m.IsUnreadFor(email),
                IsStarred = m.IsStarredFor(email),
                IsDraft = m.IsDraft,
                IsExternal = m.IsExternal,
                CategoryName = m.Category?.CategoryName,
                CategoryColor = m.Category?.CategoryColor
            };
        }

        // Mesaj hiç yoksa 404, var ama başkasınınsa 403
        private async Task<IActionResult> NotFoundOrForbidAsync(int id) =>
            await _context.Messages.AnyAsync(m => m.MessageID == id) ? Forbid() : NotFound();

        private bool IsAjax() => Request.Headers.XRequestedWith == "XMLHttpRequest";

        private static string WithPrefix(string prefix, string? subject)
        {
            subject = (subject ?? "").Trim();
            var result = subject.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? subject : $"{prefix} {subject}".Trim();
            return result.Length > 200 ? result[..200] : result;   // veritabanında konu en fazla 200 karakter
        }
    }
}
