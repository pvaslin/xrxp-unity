using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace XRXP.Modules.SceneController
{
    /// <summary>
    /// Message structure for WebSocket communication between XRXP clients and server.
    /// </summary>
    public struct Message
    {
        public string Protocol;
        public Dictionary<string, string> Properties;

        /// <summary>
        /// Converts the message to JSON format.
        /// </summary>
        public string ToJson()
        {
            return ConvertMessageToJson(this);
        }

        /// <summary>
        /// Converts a Message to JSON string.
        /// </summary>
        public static string ConvertMessageToJson(Message message)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"protocol\": \"" + EscapeString(message.Protocol) + "\",");
            sb.Append("\"properties\": {");

            int count = 0;
            foreach (KeyValuePair<string, string> property in message.Properties)
            {
                sb.Append("\"" + EscapeString(property.Key) + "\": \"" + EscapeString(property.Value) + "\"");
                count++;

                if (count < message.Properties.Count)
                    sb.Append(",");
            }

            sb.Append("}");
            sb.Append("}");

            return sb.ToString();
        }

        /// <summary>
        /// Escapes special characters in a string for JSON.
        /// </summary>
        public static string EscapeString(string input)
        {
            return input.Replace("\"", "\\\"");
        }

        /// <summary>
        /// Converts a JSON string to a Message.
        /// </summary>
        public static Message ConvertJsonToMessage(string json)
        {
            Message message = new Message();

            // Remove white spaces and new lines
            json = Regex.Replace(json, @"\s+", "");

            // Check if JSON starts with '{' and ends with '}'
            if (json.StartsWith("{") && json.EndsWith("}"))
            {
                // Remove opening and closing brackets
                json = json.Substring(1, json.Length - 2);

                string[] protocolProperties = json.Split('{', 2);
                string protocol = protocolProperties[0].Split(",")[0];
                string[] keyValue = protocol.Split(':');
                if (keyValue[0].Contains("protocol"))
                {
                    message.Protocol = keyValue[1].Trim('"');
                }

                // Split JSON into key-value pairs
                string[] keyValuePairs = protocolProperties[1].Replace("}", string.Empty).Split(',');

                message.Properties = new Dictionary<string, string>();

                foreach (string keyValuePair in keyValuePairs)
                {
                    // Split key-value pair into key and value
                    string[] pair = keyValuePair.Split(':');

                    if (pair.Length == 2)
                    {
                        string key = pair[0].Trim('"').Replace("\"", string.Empty);
                        string value = pair[1].Trim('"').Replace("\"", string.Empty);
                        message.Properties.Add(key, value);
                    }
                }
            }

            return message;
        }
    }
}
