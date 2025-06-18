using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MosefakApp.Core.Dtos.Notification;
public class SendNotificationToAllRequest
{
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
}
