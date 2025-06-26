using MediatR;

namespace Perform.Web;

public class ButtonEventHandler(ShowService showService, ILogger<ButtonEvent> logger)
    : INotificationHandler<ButtonEvent>
{
    public Task Handle(ButtonEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation($"ButtonEvent: Id={notification.ButtonId}, Type:{notification.PressType}");
        return showService.Handle(notification, cancellationToken);
    }
}