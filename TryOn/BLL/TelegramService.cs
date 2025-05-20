using ENTITIES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TryOn.BLL
{
    public class TelegramService
    {
        private static TelegramService _instance;
        private TelegramBotClient _botClient;
        private readonly string _botToken = "7715674518:AAFdrNEYnshBgFm8maLv5rt1KFeSJMUOCho"; // Token del bot
        private Dictionary<long, string> _chatIds = new Dictionary<long, string>(); // Almacena los chat_id de los usuarios

        private TelegramService()
        {
            _botClient = new TelegramBotClient(_botToken);
            StartReceiver();
        }

        public static TelegramService GetInstance()
        {
            if (_instance == null)
            {
                _instance = new TelegramService();
            }
            return _instance;
        }

        private async Task StartReceiver()
        {
            var token = new CancellationTokenSource();
            var cancelToken = token.Token;
            var receiverOptions = new ReceiverOptions { AllowedUpdates = { } };

            _botClient.StartReceiving(
                updateHandler: OnUpdateReceived,
                errorHandler: OnErrorReceived,
                receiverOptions: receiverOptions,
                cancellationToken: cancelToken
            );
        }

        private async Task OnUpdateReceived(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                // Manejar mensajes
                if (update.Message is Message message)
                {
                    // Guardar el chat_id del usuario
                    if (!_chatIds.ContainsKey(message.Chat.Id))
                    {
                        _chatIds.Add(message.Chat.Id, message.Chat.Username ?? message.Chat.FirstName);
                    }

                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("🛍 Recomendaciones"),
                            InlineKeyboardButton.WithCallbackData("🔥 Promociones")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("📞 Contacto tienda")
                        }
                    });

                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "¡Hola! Bienvenido a nuestro sistema de moda TryOn 👗\nSelecciona una opción:",
                        replyMarkup: inlineKeyboard,
                        cancellationToken: cancellationToken
                    );
                }

                // Manejar callbacks de botones
                if (update.CallbackQuery != null)
                {
                    var callbackQuery = update.CallbackQuery;
                    string respuesta = "";

                    if (callbackQuery.Data == "🛍 Recomendaciones")
                    {
                        respuesta = "👚 Blusa floral - Talla S - Roja\n👕 Camisa casual - Talla M - Azul\n👗 Vestido verano - Talla L - Estampado";
                    }
                    else if (callbackQuery.Data == "🔥 Promociones")
                    {
                        // Obtener promociones activas
                        var promocionService = new PromocionService();
                        var promocionesActivas = promocionService.GetPromocionesActivas();

                        if (promocionesActivas.Any())
                        {
                            respuesta = "🔥 Promociones activas:\n\n";
                            foreach (var promo in promocionesActivas)
                            {
                                respuesta += $"🏷️ {promo.Titulo}\n";
                                respuesta += $"   {promo.Descripcion}\n";
                                respuesta += $"   Descuento: {promo.PorcentajeDescuento}%\n";
                                respuesta += $"   Código: {promo.CodigoPromocion}\n";
                                respuesta += $"   Válido hasta: {promo.FechaFin.ToString("dd/MM/yyyy")}\n\n";
                            }
                        }
                        else
                        {
                            respuesta = "No hay promociones activas en este momento. ¡Vuelve pronto!";
                        }
                    }
                    else if (callbackQuery.Data == "📞 Contacto tienda")
                    {
                        respuesta = "📞 WhatsApp: +57 300 123 4567\n📍 Dirección: Calle 123 #45-67, Villanueva\n🕘 Horario: Lunes a sábado, 9 am a 7 pm";
                    }

                    await botClient.SendTextMessageAsync(
                        chatId: callbackQuery.Message.Chat.Id,
                        text: $"✅ {respuesta}",
                        cancellationToken: cancellationToken
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en OnUpdateReceived: {ex.Message}");
            }
        }

        private Task OnErrorReceived(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Error en el bot de Telegram: {exception.Message}");
            return Task.CompletedTask;
        }

        public async Task<bool> EnviarPromocion(Promocion promocion, string mensaje)
        {
            try
            {
                // Si no hay usuarios registrados, no se puede enviar la promoción
                if (_chatIds.Count == 0)
                {
                    return false;
                }

                // Construir el mensaje de la promoción
                string mensajePromocion = $"🔥 *NUEVA PROMOCIÓN* 🔥\n\n";
                mensajePromocion += $"*{promocion.Titulo}*\n\n";
                mensajePromocion += $"{promocion.Descripcion}\n\n";
                mensajePromocion += $"Descuento: *{promocion.PorcentajeDescuento}%*\n";

                if (promocion.Prenda != null)
                {
                    mensajePromocion += $"Producto: {promocion.Prenda.Nombre}\n";
                }
                else if (promocion.Categoria != null)
                {
                    mensajePromocion += $"Categoría: {promocion.Categoria.Nombre}\n";
                }

                mensajePromocion += $"Código: *{promocion.CodigoPromocion}*\n";
                mensajePromocion += $"Válido hasta: {promocion.FechaFin.ToString("dd/MM/yyyy")}\n\n";

                if (!string.IsNullOrEmpty(mensaje))
                {
                    mensajePromocion += $"{mensaje}\n\n";
                }

                mensajePromocion += "¡No te lo pierdas! 🛍️";

                // Enviar mensaje a todos los usuarios registrados
                foreach (var chatId in _chatIds.Keys)
                {
                    await _botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: mensajePromocion,
                        parseMode: ParseMode.Markdown
                    );
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar promoción: {ex.Message}");
                return false;
            }
        }

        public List<string> GetRegisteredUsers()
        {
            return _chatIds.Values.ToList();
        }

        public int GetRegisteredUsersCount()
        {
            return _chatIds.Count;
        }
    }
}