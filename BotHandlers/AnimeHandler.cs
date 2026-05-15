using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using WebApi.Services;
using WebApi.Models;
using Microsoft.Extensions.DependencyInjection;

namespace WebApi.BotHandlers
{
    public class AnimeHandler
    {
        private static readonly Dictionary<long, string> UserStates = new();
        private static readonly Dictionary<long, favorModel> PendingFavorites = new();

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, IServiceProvider serviceProvider)
        {
            if (update.Message is { Text: { } } message)
            {
                await HandleMessageAsync(botClient, message, serviceProvider);
                return;
            }

            if (update.CallbackQuery is { } callbackQuery)
            {
                await HandleCallbackQueryAsync(botClient, callbackQuery, serviceProvider);
                return;
            }
        }

        private async Task HandleMessageAsync(ITelegramBotClient botClient, Message message, IServiceProvider serviceProvider)
        {
            var chatId = message.Chat.Id;
            var text = message.Text;

            using var scope = serviceProvider.CreateScope();
            var animeSer = scope.ServiceProvider.GetRequiredService<AnimeSer>();
            var mangaSer = scope.ServiceProvider.GetRequiredService<MangaSer>();
            var favorDb = scope.ServiceProvider.GetRequiredService<FavorDbService>();

            switch (text)
            {
                case "/start":
                    UserStates.Remove(chatId);
                    PendingFavorites.Remove(chatId);
                    string welcomeText = "Привіт! Я твій помічник у світі аніме та манги ⛩\n\n" +
                                         "Ось що я вмію робити:\n" +
                                         "📺 Аніме — пошук тайтлів за назвою\n" +
                                         "📚 Манга — пошук манги за назвою\n" +
                                         "🎭 Пошук за жанром — топ найкращих в улюбленому жанрі\n" +
                                         "⭐️ Мої списки та улюблене — збереження улюбленого\n" +
                                         "Оберіть потрібну дію на клавіатурі нижче 👇";
                    await SendMainMenu(botClient, chatId, welcomeText);
                    return;

                case "📺 Аніме":
                    UserStates[chatId] = "WaitingAnimeName";
                    await botClient.SendMessage(chatId, "Напиши назву аніме (англійською):");
                    return;

                case "📚 Манга":
                    UserStates[chatId] = "WaitingMangaName";
                    await botClient.SendMessage(chatId, "Введи назву манги (англійською):");
                    return;

                case "🎭 Пошук за жанром":
                    UserStates.Remove(chatId);
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new [] { InlineKeyboardButton.WithCallbackData("📺 Жанри Аніме", "genre_cat_anime"), InlineKeyboardButton.WithCallbackData("📚 Жанри Манги", "genre_cat_manga") }
                    });
                    await botClient.SendMessage(chatId, "Що саме будемо шукати?", replyMarkup: inlineKeyboard);
                    return;

                case "⭐ Мої списки та улюблене":
                    UserStates.Remove(chatId);
                    var favorites = await favorDb.GetAllFavoritesAsync();

                    if (favorites.Count == 0)
                    {
                        await botClient.SendMessage(chatId, "Твої списки порожні! 🥺");
                    }
                    else
                    {
                        string favListText = "📂 *Твої списки:*\n\n";
                        var groups = favorites.GroupBy(f => f.Status ?? "Без статусу");

                        foreach (var group in groups)
                        {
                            favListText += $"🔹 *{group.Key}:*\n";
                            foreach (var f in group)
                            {
                                string icon = f.Type == "Anime" ? "🎬" : "📖";
                                string myScore = f.Score.HasValue ? $" (⭐ {f.Score})" : "";
                                string myNote = !string.IsNullOrEmpty(f.PersonalNote) ? $" 📝 {f.PersonalNote}" : "";
                                favListText += $"{icon} *{f.Title}*{myScore}{myNote}\n";
                            }
                            favListText += "\n";
                        }

                        var manageKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new [] { InlineKeyboardButton.WithCallbackData("🗑️ Видалити тайтл зі списку", "manage_del") }
                        });

                        await botClient.SendMessage(chatId, favListText, parseMode: ParseMode.Markdown, replyMarkup: manageKeyboard);
                    }
                    return;
            }

            if (UserStates.TryGetValue(chatId, out var state))
            {
                if (state == "WaitingAnimeName")
                {
                    var response = await animeSer.SearchAnimeAsync(text);
                    if (response?.data != null && response.data.Any())
                    {
                        var buttons = response.data.Take(5).Select(a => new[] { InlineKeyboardButton.WithCallbackData($"🎬 {a.title} (⭐ {a.score})", $"info_{a.mal_id}") }).ToArray();
                        await botClient.SendMessage(chatId, "Ось що я знайшов:", replyMarkup: new InlineKeyboardMarkup(buttons));
                    }
                    else { await botClient.SendMessage(chatId, "Нічого не знайдено."); }
                    UserStates.Remove(chatId);
                }
                else if (state == "WaitingMangaName")
                {
                    var response = await mangaSer.SearchMangaAsync(text);
                    if (response?.data != null && response.data.Any())
                    {
                        var buttons = response.data.Take(5).Select(m => new[] { InlineKeyboardButton.WithCallbackData($"📖 {m.title} (⭐ {m.score})", $"minfo_{m.mal_id}") }).ToArray();
                        await botClient.SendMessage(chatId, "Ось що я знайшов по манзі:", replyMarkup: new InlineKeyboardMarkup(buttons));
                    }
                    else { await botClient.SendMessage(chatId, "Нічого не знайдено."); }
                    UserStates.Remove(chatId);
                }
                else if (state == "WaitingForScore")
                {
                    if (PendingFavorites.TryGetValue(chatId, out var item))
                    {
                        if (float.TryParse(text.Replace(".", ","), out float score) && score >= 1 && score <= 10)
                        {
                            item.Score = score;
                            UserStates[chatId] = "WaitingForNote";
                            await botClient.SendMessage(chatId, "Оцінку записано! Тепер додай нотатку (або `-`):");
                        }
                        else if (text == "-")
                        {
                            item.Score = null;
                            UserStates[chatId] = "WaitingForNote";
                            await botClient.SendMessage(chatId, "Пропущено. Додай нотатку (або `-`):");
                        }
                        else
                        {
                            await botClient.SendMessage(chatId, "Будь ласка, введи число від 1 до 10 (або відправ `-`, щоб пропустити):");
                        }
                    }
                }
                else if (state == "WaitingForNote")
                {
                    if (PendingFavorites.TryGetValue(chatId, out var item))
                    {
                        item.PersonalNote = text != "-" ? text : null;
                        await favorDb.InsertFavoriteAsync(item);
                        UserStates.Remove(chatId);
                        PendingFavorites.Remove(chatId);
                        await botClient.SendMessage(chatId, "✅ Успішно збережено!");
                    }
                }
            }
        }

        private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, IServiceProvider serviceProvider)
        {
            var data = callbackQuery.Data;
            var chatId = callbackQuery.Message!.Chat.Id;

            using var scope = serviceProvider.CreateScope();
            var animeSer = scope.ServiceProvider.GetRequiredService<AnimeSer>();
            var mangaSer = scope.ServiceProvider.GetRequiredService<MangaSer>();
            var favorDb = scope.ServiceProvider.GetRequiredService<FavorDbService>();

      
            if (data == "manage_del")
            {
                var favorites = await favorDb.GetAllFavoritesAsync();
                var buttons = favorites.Select(f => new[]
                {
                    InlineKeyboardButton.WithCallbackData($"❌ {f.Title}", $"delete_id_{f.Id}")
                }).ToArray();

                await botClient.SendMessage(chatId, "Обери тайтл, який хочеш видалити:", replyMarkup: new InlineKeyboardMarkup(buttons));
                await botClient.AnswerCallbackQuery(callbackQuery.Id);
            }
            else if (data!.StartsWith("delete_id_"))
            {
                int dbId = int.Parse(data.Replace("delete_id_", ""));
                await favorDb.DeleteFavoriteAsync(dbId);

                await botClient.AnswerCallbackQuery(callbackQuery.Id, "Видалено з бази даних!", showAlert: true);
                await botClient.DeleteMessage(chatId, callbackQuery.Message.MessageId);
            }


            else if (data == "genre_cat_anime" || data == "genre_cat_manga")
            {
                string prefix = data == "genre_cat_anime" ? "agenre_" : "mgenre_";
                var keyboard = new InlineKeyboardMarkup(new[]
                {
                    new [] { InlineKeyboardButton.WithCallbackData("Action", $"{prefix}1"), InlineKeyboardButton.WithCallbackData("Adventure", $"{prefix}2") },
                    new [] { InlineKeyboardButton.WithCallbackData("Comedy", $"{prefix}4"), InlineKeyboardButton.WithCallbackData("Drama", $"{prefix}8") },
                    new [] { InlineKeyboardButton.WithCallbackData("Fantasy", $"{prefix}10"), InlineKeyboardButton.WithCallbackData("Romance", $"{prefix}22") }
                });
                await botClient.EditMessageText(new ChatId(chatId), callbackQuery.Message.MessageId, "Обери жанр:", replyMarkup: keyboard);
            }

            else if (data.StartsWith("agenre_"))
            {
                int genreId = int.Parse(data.Split('_')[1]);
                await botClient.AnswerCallbackQuery(callbackQuery.Id, "Шукаю найкращі тайтли...");
                var response = await animeSer.SearchAnimeByGenreAsync(genreId);
                if (response?.data != null && response.data.Any())
                {
                    var buttons = response.data.Take(5).Select(a => new[] { InlineKeyboardButton.WithCallbackData($"🎬 {a.title} (⭐ {a.score})", $"info_{a.mal_id}") }).ToArray();
                    await botClient.SendMessage(chatId, "🏆 Топ-5 аніме у цьому жанрі:", replyMarkup: new InlineKeyboardMarkup(buttons));
                }
            }
            else if (data.StartsWith("mgenre_"))
            {
                int genreId = int.Parse(data.Split('_')[1]);
                await botClient.AnswerCallbackQuery(callbackQuery.Id, "Шукаю найкращу мангу...");
                var response = await mangaSer.SearchMangaByGenreAsync(genreId);
                if (response?.data != null && response.data.Any())
                {
                    var buttons = response.data.Take(5).Select(m => new[] { InlineKeyboardButton.WithCallbackData($"📖 {m.title} (⭐ {m.score})", $"minfo_{m.mal_id}") }).ToArray();
                    await botClient.SendMessage(chatId, "🏆 Топ-5 манги у цьому жанрі:", replyMarkup: new InlineKeyboardMarkup(buttons));
                }
            }

            else if (data.StartsWith("info_"))
            {
                int malId = int.Parse(data.Split('_')[1]);
                var res = await animeSer.GetAnimeByIdAsync(malId);
                var a = res.data;
                bool isLong = a.synopsis?.Length > 400;
                string info = $"🌟 *{a.title}*\n\n📅 Статус: {a.status}\n\n📝 *Опис:* {(isLong ? a.synopsis.Substring(0, 400) + "..." : a.synopsis)}";

                var buttons = new List<InlineKeyboardButton[]>();
                if (isLong) buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("📝 Читати повністю", $"adesc_{malId}") });
                buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("👥 Персонажі", $"chars_{malId}"), InlineKeyboardButton.WithCallbackData("🎶 Теми", $"themes_{malId}") });
                buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("⭐ Додати", $"add_fav_{malId}") });
                await botClient.SendMessage(chatId, info, parseMode: ParseMode.Markdown, replyMarkup: new InlineKeyboardMarkup(buttons));
            }
            else if (data.StartsWith("minfo_"))
            {
                int malId = int.Parse(data.Split('_')[1]);
                var res = await mangaSer.GetMangaByIdAsync(malId);
                var m = res.data;
                string info = $"📖 *{m.title}*\n\n📊 Оцінка: {m.score}\n📅 Статус: {m.status}\n\n📝 *Опис:* {(m.synopsis?.Length > 400 ? m.synopsis.Substring(0, 400) + "..." : m.synopsis)}";

                var buttons = new List<InlineKeyboardButton[]>();
                if (m.synopsis?.Length > 400) buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("📝 Читати повністю", $"mdesc_{malId}") });
                buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("⭐ Додати мангу", $"m_add_fav_{malId}") });

                await botClient.SendMessage(chatId, info, parseMode: ParseMode.Markdown, replyMarkup: new InlineKeyboardMarkup(buttons));
            }

            else if (data.StartsWith("add_fav_") || data.StartsWith("m_add_fav_"))
            {
                bool isAnime = data.StartsWith("add_fav_");
                int malId = int.Parse(data.Replace(isAnime ? "add_fav_" : "m_add_fav_", ""));
                var keyboard = new InlineKeyboardMarkup(new[]
                {
                    new [] { InlineKeyboardButton.WithCallbackData(isAnime ? "📺 Дивлюсь" : "📖 Читаю", $"set_st_{(isAnime ? "a":"m")}_{malId}_watching") },
                    new [] { InlineKeyboardButton.WithCallbackData("⏳ У планах", $"set_st_{(isAnime ? "a":"m")}_{malId}_plan") },
                    new [] { InlineKeyboardButton.WithCallbackData("✅ Завершено", $"set_st_{(isAnime ? "a":"m")}_{malId}_done") }
                });
                await botClient.SendMessage(chatId, "Обери статус:", replyMarkup: keyboard);
            }
            else if (data.StartsWith("set_st_"))
            {
                var parts = data.Split('_');
                string type = parts[2];
                int malId = int.Parse(parts[3]);
                string statusRaw = parts[4];
                string statusName = statusRaw == "done" ? "Завершено" : statusRaw == "watching" ? "В процесі" : "У планах";

                string title = type == "a" ? (await animeSer.GetAnimeByIdAsync(malId)).data.title : (await mangaSer.GetMangaByIdAsync(malId)).data.title;

                PendingFavorites[chatId] = new favorModel { MalId = malId, Title = title, Type = type == "a" ? "Anime" : "Manga", Status = statusName };

                await botClient.DeleteMessage(chatId, callbackQuery.Message.MessageId);
                if (statusRaw == "done")
                {
                    UserStates[chatId] = "WaitingForScore";
                    await botClient.SendMessage(chatId, $"Твоя оцінка для *{title}* (1-10)? (або `-` щоб пропустити)", parseMode: ParseMode.Markdown);
                }
                else
                {
                    UserStates[chatId] = "WaitingForNote";
                    await botClient.SendMessage(chatId, $"Додай нотатку для *{title}* (або `-`):", parseMode: ParseMode.Markdown);
                }
            }


            else if (data.StartsWith("adesc_"))
            {
                var res = await animeSer.GetAnimeByIdAsync(int.Parse(data.Split('_')[1]));
                await botClient.AnswerCallbackQuery(callbackQuery.Id);
                await botClient.SendMessage(chatId, $"*Повний опис:*\n\n{res.data.synopsis}", parseMode: ParseMode.Markdown);
            }
            else if (data.StartsWith("mdesc_"))
            {
                var res = await mangaSer.GetMangaByIdAsync(int.Parse(data.Split('_')[1]));
                await botClient.AnswerCallbackQuery(callbackQuery.Id);
                await botClient.SendMessage(chatId, $"*Повний опис:*\n\n{res.data.synopsis}", parseMode: ParseMode.Markdown);
            }
            else if (data.StartsWith("chars_"))
            {
                var res = await animeSer.GetCharactersAsync(int.Parse(data.Split('_')[1]));
                await botClient.AnswerCallbackQuery(callbackQuery.Id);
                await botClient.SendMessage(chatId, $"👥 *Персонажі:*\n{string.Join(", ", res.data.Take(10).Select(c => c.character.name))}", parseMode: ParseMode.Markdown);
            }
            else if (data.StartsWith("themes_"))
            {
                var res = await animeSer.GetThemesAsync(int.Parse(data.Split('_')[1]));
                await botClient.AnswerCallbackQuery(callbackQuery.Id);
                await botClient.SendMessage(chatId, $"🎶 *Опенінги:*\n{(res.data?.openings != null && res.data.openings.Any() ? string.Join("\n", res.data.openings.Take(3)) : "Не знайдено")}", parseMode: ParseMode.Markdown);
            }
        }

        private async Task SendMainMenu(ITelegramBotClient botClient, long chatId, string text)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "📺 Аніме", "📚 Манга" },
                new KeyboardButton[] { "🎭 Пошук за жанром" },
                new KeyboardButton[] { "⭐ Мої списки та улюблене" }
            })
            { ResizeKeyboard = true };
            await botClient.SendMessage(chatId, text, replyMarkup: keyboard, parseMode: ParseMode.Markdown);
        }

        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception ex, Telegram.Bot.Polling.HandleErrorSource s, CancellationToken ct) => Task.CompletedTask;
    }
}