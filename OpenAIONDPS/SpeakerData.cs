using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenAIONDPS
{
    class SpeakerData
    {
        public Regex SpeakTextRegex = null;

        public string SpeakText = "";

        public SpeakerData(string PatternText, string SpeakText)
        {
            this.SpeakTextRegex = new Regex(PatternText, RegexOptions.Compiled);
            this.SpeakText = SpeakText;
        }
    }
}
