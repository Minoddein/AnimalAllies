using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBotService.States.Info;

public class InfoState: IState
{
    public async Task<IState> HandleAsync(
        Message message,
        ITelegramBotClient botClient,
        CancellationToken cancellationToken = default)
    {
        var infoMessage = "\ud83d\udcda Справочник по командам бота AnimalAllies\n\n" +
                          "Добро пожаловать в бота AnimalAllies! Здесь ты можешь получить " +
                          "информацию о волонтёрстве, статусе заявки и связаться с поддержкой. " +
                          " \n\n\ud83d\udd39 Доступные команды: \n\n-\ud83d\udc4b " +
                          "/start – Начало работы с ботом. Приветствие и краткая инструкция." +
                          "  \n- \ud83d\udd10/authorize – Авторизация на платформе (для волонтёров и партнёров)." +
                          "  \n- \ud83d\udcc4/status – Проверить статус своей заявки на волонтёрство. " +
                          " \n- \ud83d\udcda/info – Открыть этот справочник с описанием команд. " +
                          " \n- \ud83d\udee0\ufe0f/support – Получить контакты технической поддержки " +
                          "для решения проблем." +
                          "  \n\n\ud83d\udccc Как пользоваться ботом? " +
                          " \n1. Если ты новый пользователь, начни с команды /start. " +
                          " \n2. Для подачи заявки на волонтёрство используй /authorize. " +
                          " \n3. Хочешь узнать, как продвигается рассмотрение заявки? Жми /status." +
                          "  \n4. Остались вопросы? Обратись в /support.  \n\nМы рады, что ты с нами! \ud83d\udc3e  ";
        
        await botClient.SendMessage(message.Chat.Id, infoMessage, cancellationToken: cancellationToken);
        
        return StateFactory.GetState(typeof(WaitingCommandState).FullName!);
    }
}