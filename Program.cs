
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;

namespace HW_bot
{
    class Program
    {
        private static readonly string BotToken = "7918846181:AAGpYTEQ_SDU-s4Okk4zPqqlzJ2Mpf7rSfQ";
        private static readonly TelegramBotClient BotClient = new TelegramBotClient(BotToken);
        private static int currentQuestionIndex = 0;

        private static List<Question> questions = new List<Question>
        {
                    new Question("Какой ученый предложил теорию относительности?",
                    new List<string> {"Исаак Ньютон", "Никола Тесла", "Альберт Эйнштейн", "Галилео Галилей"}, 2, "Правильно! Теория относительности была предложена Альбертом Эйнштейном."),

                    new Question("Какой элемент является основой органической химии?",
                    new List<string> {"Кислород", "Водород", "Углерод", "Азот"}, 2, "Верно! Углерод — основной элемент в органической химии."),

                    new Question("Кто изобрел первый электрический двигатель?",
                    new List<string> {"Майкл Фарадей", "Джеймс Уатт", "Александр Белл", "Томас Эдисон"}, 0, "Майкл Фарадей изобрел первый электрический двигатель."),

                    new Question("Как называется самая близкая к Земле звезда после Солнца?",
                    new List<string> {"Сириус", "Альфа Центавра", "Полярная звезда", "Бетельгейзе"}, 1, "Правильно! Альфа Центавра — ближайшая к Земле звезда после Солнца."),

                    new Question("Какое устройство считается первым вычислительным устройством?",
                    new List<string> {"Абак", "Аналитическая машина", "ЭНИАК", "Перфокарта"}, 0, "Верно! Абак — это первое известное вычислительное устройство."),
        };

        static async Task Main(string[] args)
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            BotClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: CancellationToken.None
            );

            Console.WriteLine("Бот запущен...");
            Console.ReadLine();
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                var message = update.Message;

                if (message.Text.ToLower() == "/start")
                {
                    await BotClient.SendTextMessageAsync(message.Chat.Id, "Привет! Добро пожаловать в викторину по искусству.");
                    await SendQuestion(message.Chat.Id);
                }
                else if (currentQuestionIndex < questions.Count)
                {
                    await CheckAnswer(message.Chat.Id, message.Text);
                }
                else
                {
                    await BotClient.SendTextMessageAsync(message.Chat.Id, "Викторина завершена! Спасибо за участие.");
                    currentQuestionIndex = 0;
                }
            }
        }

        private static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Произошла ошибка: {exception.Message}");
            return Task.CompletedTask;
        }

        private static async Task SendQuestion(long chatId)
        {
            var question = questions[currentQuestionIndex];
            var replyKeyboard = new ReplyKeyboardMarkup(GetReplyKeyboard(question.Options))
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };

            await BotClient.SendTextMessageAsync(
                chatId,
                $"Вопрос {currentQuestionIndex + 1}: {question.Text}",
                replyMarkup: replyKeyboard);
        }

        private static async Task CheckAnswer(long chatId, string answer)
        {
            var question = questions[currentQuestionIndex];
            if (question.Options[question.CorrectOptionIndex] == answer)
            {
                await BotClient.SendTextMessageAsync(chatId, question.CorrectMessage);
            }
            else
            {
                await BotClient.SendTextMessageAsync(chatId, $"Неверно. Правильный ответ: {question.Options[question.CorrectOptionIndex]}");
            }

            currentQuestionIndex++;
            if (currentQuestionIndex < questions.Count)
            {
                await SendQuestion(chatId);
            }
            else
            {
                await BotClient.SendTextMessageAsync(chatId, "Викторина завершена! Спасибо за участие.");
            }
        }

        private static KeyboardButton[][] GetReplyKeyboard(List<string> options)
        {
            var keyboard = new KeyboardButton[options.Count][];
            for (int i = 0; i < options.Count; i++)
            {
                keyboard[i] = new KeyboardButton[] { new KeyboardButton(options[i]) };
            }
            return keyboard;
        }
    }

    class Question
    {
        public string Text { get; }
        public List<string> Options { get; }
        public int CorrectOptionIndex { get; }
        public string CorrectMessage { get; }

        public Question(string text, List<string> options, int correctOptionIndex, string correctMessage)
        {
            Text = text;
            Options = options;
            CorrectOptionIndex = correctOptionIndex;
            CorrectMessage = correctMessage;
        }
    }
}