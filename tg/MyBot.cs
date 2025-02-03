using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Linq;

namespace tg
{
    public class MyBot
    {
        static ITelegramBotClient _botClient;

        static ReceiverOptions _receiverOptions;

        static string[] _names;

        static readonly string _token = "7793280867:AAG1BrS99gO10i0mcASf4OLYko_b1JqDetk";

        public MyBot(string token)
        {
            _botClient = new TelegramBotClient(token);
            _names = null;
        }

        static async Task Main()
        {
            _botClient = new TelegramBotClient(_token);
            _receiverOptions = new ReceiverOptions // настройки бота
            {
                AllowedUpdates = new[] //апдейты
                {
                UpdateType.Message,
            },
                DropPendingUpdates = true,
            };
            var cts = new CancellationTokenSource();
            _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token);
            var me = await _botClient.GetMe(); //инфа о боте
            await Task.Delay(-1); //задержка для постоянной работы
        }
        private static Task ErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            throw new NotImplementedException();
        }
        private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        {
                            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
                            var message = update.Message;
                            var user = message.From;
                            Console.WriteLine($"{user.FirstName} ({user.Id}) написал сообщение: {message.Text}");
                            var chat = message.Chat;
                            switch (message.Type)
                            {
                                case MessageType.Text:
                                    if (message.Text == "/start")
                                    {
                                        await botClient.SendMessage(
                                        chatId: chat.Id,
                                        text: "Привет я могу посчитать гсм  🚗 и все");
                                        var startButton = new ReplyKeyboardMarkup(new[]
                                        {
                                            new[] {new KeyboardButton("Посчитать гсм")},
                                            new[] {new KeyboardButton("Просто кнопка")},
                                            new[] {new KeyboardButton("HELP")}
                                        })
                                        {
                                            ResizeKeyboard = true,
                                            OneTimeKeyboard = false
                                        };
                                        await botClient.SendMessage(message.Chat.Id, "Выбери действие или начинай вводить фамилии через пробел", replyMarkup: startButton);
                                    }
                                    else if (message.Text == "Посчитать гсм")
                                    {
                                        await _botClient.SendMessage(message.Chat.Id, "Вводи фамилии через пробел:");
                                        _names = null;
                                    }
                                    else if (message.Text == "Просто кнопка")
                                    {
                                        await _botClient.SendMessage(message.Chat.Id, "Сказал же просто кнопка");
                                    }
                                    else if (message.Text == "HELP")
                                    {
                                        await botClient.SendMessage(message.Chat.Id, "ну что ты растерялся, выбери другое действие");
                                    }
                                    else if (_names == null)
                                    {
                                        try
                                        {
                                            if (message.Text.All(c => char.IsLetter(c) || c == ' '))
                                            {
                                                _names = message.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                                await _botClient.SendMessage(message.Chat.Id, "Теперь километраж через пробел:");
                                            }
                                            else if (message.Text.All(char.IsLetter) == false)
                                            {
                                                throw new Exception("Можно ввести только буквы");
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            await _botClient.SendMessage(message.Chat.Id, "Ошибка: Убедиcь, что введены только буквы");
                                        }
                                    }

                                    else if (_names != null)
                                    {
                                        try
                                        {
                                            if (message.Text.All(c => char.IsDigit(c) || c == ' '))
                                            {
                                                MyCalculate calculate = new MyCalculate(message.Text);
                                                if (_names.Length != calculate.Getkilometers().Length)
                                                {
                                                    throw new FormatException("ошибка, не для всех введен километраж");
                                                }
                                                string response = "ГСМ  🚗\n";
                                                response += "\n";

                                                for (int i = 0; i < _names.Length; i++)
                                                {
                                                    response += $"{textInfo.ToTitleCase(_names[i])} {calculate.Getkilometers()[i]}км  {calculate.Calculete()[i]} ₽ 💰 \n";
                                                }
                                                await _botClient.SendMessage(
                                                    chatId: message.Chat.Id,
                                                    text: $"```\n{response}\n```",
                                                    parseMode: ParseMode.MarkdownV2
                                                    );
                                            }
                                            else if (message.Text.All(char.IsDigit) == false)
                                            {
                                                throw new FormatException("ошибка, неправильно введен километраж");
                                            }
                                        }
                                        catch (FormatException)
                                        {
                                            await _botClient.SendMessage(message.Chat.Id, "Ошибка: неправильный формат ввода километража, введи заново фамилии и затем километраж");
                                        }

                                        finally
                                        {
                                            _names = null;
                                        }
                                    }
                                    break;

                                default:
                                    // Обработка не текстовых сообщений (фото, голосовые и т.д.)
                                    break;
                            }
                            return;
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical error: {ex.Message}");
            }
        }
    }
}