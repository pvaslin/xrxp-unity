using System;

namespace XRXP
{
    /// <summary>
    /// Marks a method as a handler for a specific exchange control key.
    /// The method must accept a single string parameter (the value).
    /// Discovered automatically by XRXPExchangeManager on sibling and child components.
    /// </summary>
    /// <example>
    /// [ExchangeControl("changeScene")]
    /// public void OnChangeScene(string sceneName) { ... }
    /// </example>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ExchangeControlAttribute : Attribute
    {
        public string Key { get; }

        public ExchangeControlAttribute(string key)
        {
            Key = key;
        }
    }
}
