using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace XRXP
{
    public enum ExchangeFieldType
    {
        String,
        Float,
        Int,
        Bool
    }

    public enum ExchangeControlType
    {
        Button,
        Number,
        Text,
        Dropdown
    }

    [Serializable]
    public class ExchangeStatusField
    {
        public string Key;
        public string Label;
        public ExchangeFieldType Type = ExchangeFieldType.String;
        public string DefaultValue = "";
    }

    [Serializable]
    public class ExchangeControlField
    {
        public string Key;
        public string Label;
        public ExchangeControlType ControlType = ExchangeControlType.Text;
        public string[] DropdownOptions;
        public string DefaultValue = "";
    }

    [CreateAssetMenu(fileName = "ExchangeModality", menuName = "XRXP/Exchange Modality", order = 1)]
    public class ExchangeModality : ScriptableObject
    {
        [Header("Status Fields (Device -> Dashboard)")]
        [Tooltip("Fields that stream live values from the Unity app to the dashboard")]
        public List<ExchangeStatusField> StatusFields = new List<ExchangeStatusField>();

        [Header("Control Fields (Dashboard -> Device)")]
        [Tooltip("Fields that allow the dashboard to send commands to the Unity app")]
        public List<ExchangeControlField> ControlFields = new List<ExchangeControlField>();

        public string ToSchemaJson()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"statusFields\":[");

            for (int i = 0; i < StatusFields.Count; i++)
            {
                var f = StatusFields[i];
                sb.Append("{");
                sb.Append($"\"key\":\"{Escape(f.Key)}\",");
                sb.Append($"\"label\":\"{Escape(f.Label)}\",");
                sb.Append($"\"type\":\"{f.Type.ToString().ToLower()}\",");
                sb.Append($"\"defaultValue\":\"{Escape(f.DefaultValue)}\"");
                sb.Append("}");
                if (i < StatusFields.Count - 1) sb.Append(",");
            }

            sb.Append("],\"controlFields\":[");

            for (int i = 0; i < ControlFields.Count; i++)
            {
                var f = ControlFields[i];
                sb.Append("{");
                sb.Append($"\"key\":\"{Escape(f.Key)}\",");
                sb.Append($"\"label\":\"{Escape(f.Label)}\",");
                sb.Append($"\"controlType\":\"{f.ControlType.ToString().ToLower()}\",");

                if (f.DropdownOptions != null && f.DropdownOptions.Length > 0)
                {
                    sb.Append("\"dropdownOptions\":[");
                    for (int j = 0; j < f.DropdownOptions.Length; j++)
                    {
                        sb.Append($"\"{Escape(f.DropdownOptions[j])}\"");
                        if (j < f.DropdownOptions.Length - 1) sb.Append(",");
                    }
                    sb.Append("],");
                }

                sb.Append($"\"defaultValue\":\"{Escape(f.DefaultValue)}\"");
                sb.Append("}");
                if (i < ControlFields.Count - 1) sb.Append(",");
            }

            sb.Append("]}");
            return sb.ToString();
        }

        private static string Escape(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
