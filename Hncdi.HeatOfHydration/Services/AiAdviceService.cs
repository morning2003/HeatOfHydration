using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VolPro.Entity.DomainModels.Hoh.partial;

namespace Hncdi.HeatOfHydration.Services
{
    public class AiAdviceService
    {
        private readonly Hoh_ProjectService _hohProjectService;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public AiAdviceService(
            Hoh_ProjectService hohProjectService,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _hohProjectService = hohProjectService;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetAiAdviceAsync(long hohProjectCode)
        {
            try
            {
                var data = GetMonitoringData(hohProjectCode);
                if (data == null)
                    return "无法获取监测数据，请确认部位信息是否正确。";

                string dataContext = BuildDataContext(data);
                string userQuestion = "请基于以上数据，按照以下格式输出混凝土温控处置建议：\n1. 项目整体情况（简要概括当前监测状态）\n2. 存在的问题（指出具体超标项或风险点）\n3. 解决措施（给出具体可操作的分步措施）";

                var messages = new List<ChatMessage>
                {
                    new ChatMessage { role = "system", content = $"你是一名大体积混凝土温控专家。\n\n{dataContext}" },
                    new ChatMessage { role = "user", content = userQuestion }
                };

                return await CallMimoApiAsync(messages);
            }
            catch (Exception ex)
            {
                return FormatError(ex);
            }
        }

        /// <summary>
        /// 多轮对话：前端传入完整对话历史，后端注入监测数据作为 system 上下文
        /// </summary>
        public async Task<string> ChatAsync(long hohProjectCode, List<ChatMessage> messages)
        {
            try
            {
                var data = GetMonitoringData(hohProjectCode);
                if (data == null)
                    return "无法获取监测数据，请确认部位信息是否正确。";

                string dataContext = BuildDataContext(data);

                // 组装完整消息：系统提示(含数据) + 历史对话
                var fullMessages = new List<ChatMessage>
                {
                    new ChatMessage { role = "system", content = $"你是一名大体积混凝土温控专家。你可以根据以下实时监测数据回答用户问题。\n\n{dataContext}" }
                };
                fullMessages.AddRange(messages);

                return await CallMimoApiAsync(fullMessages);
            }
            catch (Exception ex)
            {
                return FormatError(ex);
            }
        }

        // 获取数据并转换为监测数据模型

        private HohMonitoringData GetMonitoringData(long code)
        {
            var response = _hohProjectService.GetProjectDataInfo(code);
            if (response == null || response.Data == null)
                return null;

            var json = JsonConvert.SerializeObject(response.Data);
            var data = JsonConvert.DeserializeObject<HohMonitoringData>(json);
            if (data == null || data.tMDate == null || data.tCData == null)
                return null;

            return data;
        }

        private async Task<string> CallMimoApiAsync(List<ChatMessage> messages)
        {
            string apiKey = _configuration["Mimo:ApiKey"];
            string apiUrl = _configuration["Mimo:ApiUrl"] ?? "https://api.xiaomimimo.com/v1/chat/completions";
            string model = _configuration["Mimo:Model"] ?? "mimo-v2.5-pro";

            if (string.IsNullOrEmpty(apiKey))
                return "Mimo API Key 未配置，请联系管理员。";

            var requestBody = new
            {
                model = model,
                messages = messages,
                temperature = 0.7,
                max_completion_tokens = 2048
            };

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(60);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            httpRequest.Headers.Add("api-key", apiKey);
            httpRequest.Content = new StringContent(
                JsonConvert.SerializeObject(requestBody),
                Encoding.UTF8,
                "application/json");

            var httpResponse = await client.SendAsync(httpRequest);
            httpResponse.EnsureSuccessStatusCode();

            string responseBody = await httpResponse.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(responseBody);
            string content = result?.choices?[0]?.message?.content?.ToString();

            return !string.IsNullOrEmpty(content)
                ? content
                : "AI 未返回有效建议，请稍后重试。";
        }

        /// <summary>
        /// 构建监测数据上下文（注入到 system 提示词中）
        /// </summary>
        private string BuildDataContext(HohMonitoringData data)
        {
            var tm = data.tMDate;
            var tc = data.tCData;

            var warnings = BuildWarnings(tc);

            var sb = new StringBuilder();
            sb.AppendLine($"项目：{data.title}");
            sb.AppendLine($"说明：{data.info}");
            sb.AppendLine($"浇筑开始：{data.pouringStartTime}，浇筑结束：{data.pouringEndTime}");
            sb.AppendLine($"已监测时长：{data.monitoringDuration} 小时");
            sb.AppendLine();
            sb.AppendLine("【当前温度监测数据】");
            sb.AppendLine($"大气温度：{tm.atmosphericTemperature}");
            sb.AppendLine($"入模温度：{tm.moldTemperature}");
            sb.AppendLine($"进水温度：{tm.waterInTemperature}");
            sb.AppendLine($"出水温度：{tm.waterOutTemperature}");
            sb.AppendLine($"顶表面温度：{tm.topSurfaceTemperature}");
            sb.AppendLine($"内部管位温度：{tm.internalPipeTemperature}");
            sb.AppendLine($"混凝土核心温度：{tm.concreteCoreTemperature}");
            sb.AppendLine($"混凝土绝对升温：{tm.concreteAbsoluteTemperatureRise}");
            sb.AppendLine();
            sb.AppendLine("【温控指标】");
            sb.AppendLine($"水管1进出口温差：{tc.pipe1TemperatureDifference}");
            sb.AppendLine($"水管2进出口温差：{tc.pipe2TemperatureDifference}");
            sb.AppendLine($"表气温差（顶表面-大气）：{tc.surfaceAirTemperatureDifference}");
            sb.AppendLine($"里表温差（核心-表面）：{tc.surfaceCoreTemperatureDifference}");
            sb.AppendLine($"升温速率：{tc.temperatureRiseRate}");
            sb.AppendLine($"降温速率：{tc.temperatureDropRate}");
            sb.AppendLine();
            sb.AppendLine("【超标判断】");
            sb.AppendLine(warnings);
            sb.AppendLine();
            sb.AppendLine($"核心测点 {data.coreMeasurementPoints} 个，表面测点 {data.surfaceMeasurementPoints} 个");
            sb.AppendLine($"温度预警 {data.temperatureWarning} 次，通水预警 {data.waterWarning} 次");

            return sb.ToString();
        }

        private string BuildWarnings(TCData tc)
        {
            decimal TryParseValue(string str)
            {
                if (string.IsNullOrEmpty(str) || str == "--")
                    return 0;
                var cleaned = str.Replace("℃", "").Trim();
                if (cleaned.Contains("～"))
                {
                    var parts = cleaned.Split('～');
                    if (parts.Length == 2 &&
                        decimal.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out decimal a) &&
                        decimal.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out decimal b))
                        return (a + b) / 2;
                }
                decimal.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal val);
                return val;
            }

            decimal internalExternalDiff = TryParseValue(tc.surfaceCoreTemperatureDifference);
            decimal dropRate = TryParseValue(tc.temperatureDropRate);
            decimal riseRate = TryParseValue(tc.temperatureRiseRate);
            decimal pipe1Diff = TryParseValue(tc.pipe1TemperatureDifference);
            decimal pipe2Diff = TryParseValue(tc.pipe2TemperatureDifference);

            var warnings = new List<string>();
            if (internalExternalDiff > 25)
                warnings.Add($"里表温差 {internalExternalDiff}℃ 超过 25℃ 限值");
            if (Math.Abs(dropRate) > 0.08m)
                warnings.Add($"降温速率 {dropRate}℃/h 偏快（相当于 {Math.Abs(dropRate) * 24:F1}℃/天，超过 2℃/天 限值）");
            if (riseRate > 2)
                warnings.Add($"升温速率 {riseRate}℃/h 偏快");
            if (pipe1Diff > 5)
                warnings.Add($"水管1进出口温差 {pipe1Diff}℃ 超过 5℃ 限值");
            if (pipe2Diff > 5)
                warnings.Add($"水管2进出口温差 {pipe2Diff}℃ 超过 5℃ 限值");

            return warnings.Count > 0
                ? string.Join("；", warnings)
                : "各项指标均在规范限值内，未发现超标情况。";
        }

        private static string FormatError(Exception ex)
        {
            Console.WriteLine($"AI Advice Error: {ex.GetType().Name}: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"  Inner: {ex.InnerException.Message}");
            return $"AI 服务暂不可用，请稍后重试。（{ex.GetType().Name}: {ex.Message}）";
        }
    }

    // 请求/消息模型

    public class ChatMessage
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    public class ChatRequest
    {
        public long code { get; set; }
        public List<ChatMessage> messages { get; set; }
    }
}
