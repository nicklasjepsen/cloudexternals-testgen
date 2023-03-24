using System.ComponentModel;
using System.Runtime.InteropServices;
using Community.VisualStudio.Toolkit;

namespace Nicks.UnitTester.VsExtension
{
    internal partial class OptionsProvider
    {
        // Register the options with this attribute on your package class:
        // [ProvideOptionPage(typeof(OptionsProvider.GeneralOptions), "Nicks.UnitTester.VsExtension", "General", 0, 0, true, SupportsProfiles = true)]
        [ComVisible(true)]
        public class GeneralOptions : BaseOptionPage<General> { }
    }

    public class General : BaseOptionModel<General>
    {
        [Category("OpenAI")]
        [DisplayName("API Key")]
        [Description("If you haven't already, sign up for an API key at OpenAI: https://platform.openai.com/account/api-keys")]
        [DefaultValue("")]
        public string ApiKey{ get; set; } = "";
    }
}
