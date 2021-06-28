using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace SimpleSlackMessage
{
  class Program
  {
    // Работаем через HTTP.
    private static readonly HttpClient client = new HttpClient();

    // Класс для ответа со стороны Слак.
    public class SlackMessageResponse
    {
      public bool ok { get; set; }
      public string error { get; set; }
      public string channel { get; set; }
      public string ts { get; set; }
    }

    // Класс определяет структуру сообщения.
    public class SlackMessage
    {
      public string channel { get; set; }
      public string text { get; set; }
      public bool as_user { get; set; }
      public SlackAttachment[] attachments { get; set; }
    }

    // Класс определяет структуру вложения.
    public class SlackAttachment
    {
      public string fallback { get; set; }
      public string text { get; set; }
      public string image_url { get; set; }
      public string color { get; set; }
    }

    // Отправка ассинхронного сообщения в сервис.
    // Возвращает ошибку, если не удалось отправить сообщение.
    public static async Task SendMessageAsync(string token, SlackMessage msg)
    {
      // serialize method parameters to JSON
      var content = JsonConvert.SerializeObject(msg);
      var httpContent = new StringContent(
          content,
          Encoding.UTF8,
          "application/json"
      );

      // Заголовк тоента авторизации. Сделай приложение тут https://api.slack.com/apps?new_app=1 и получи токен. Токен харнится в конфигурации приложения.
      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

      // Отправка сообщения в API Слака.
      var response = await client.PostAsync("https://slack.com/api/chat.postMessage", httpContent);

      // Получаем ответ от API Слака.
      var responseJson = await response.Content.ReadAsStringAsync();

      // Конвертируем JSON ответ в класс для удобной обработки.
      SlackMessageResponse messageResponse =
          JsonConvert.DeserializeObject<SlackMessageResponse>(responseJson);

      // Возвращаем ошибку, если что-то пошло не так.
      if (messageResponse.ok == false)
      {
        throw new Exception(
            "Не удалось отправить сообщение. Текст ошибки: " + messageResponse.error
        );
      }
    }

    static void Main(string[] args) // Пример вызова: SimpleSlackMessage.exe "имя_канала_где_все" "текст очень важного сообщения"
    {
      var msg = new SlackMessage
      {
        // TODO Не красиво, надо доработать.
        channel = args[0], // Укажи имя канала. Можно использовать и кириллицу.
        text = args[1], // Укажи текст твоего сообщения.
        as_user = true,
        attachments = new SlackAttachment[]
          {
            /*  === Пример вложения === 
                    new SlackAttachment
                    {
                        fallback = "Оставь тут комментарий. Например, Тут есть ошибка, внимательнее будь.",
                        text = "Назови вложение",
                        color = "good" // Укрась цветом, чтобы видно было. Еще есть color = "danger"
                    },
                    */
          }
      };

      SendMessageAsync(
          ConfigurationManager.AppSettings["SlackBotApiToken"],  // Для удобства храни в конфиге, чтобы потом поправить.
          msg
      ).Wait();

    }
  }
}
